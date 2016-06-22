using System;

namespace MyWebSocket.Tcp.Protocol.HTTP
{
	public class HTTPReader : MyStream
	{
		const int LF = 0x0A;
		const int CR = 0x0D;
		const int CN = 0x3A;
		const int SPACE = 0x20; 
		const int STSTR = 1024;
		const int PARAM = 1024;
		const int VALUE = 1024;

		/// <summary>
		/// Допустимая длинна заголовков
		/// </summary>
		public static int LENHEAD;
		/// <summary>
		/// Допустимая длинна блока Transfer-Encoding
		/// </summary>
		public static int LENCHUNK;

		/// <summary>
		/// Заголвоки запрос
		/// </summary>
		public Header header;
		/// <summary>
		/// Информация о записи
		/// </summary>
		public HTTPFrame _Frame;
		/// <summary>
		/// Статический конструктор
		/// </summary>
		static HTTPReader()
		{
			LENHEAD = 36000;
			LENCHUNK = 64000;
		}
		/// <summary>
		/// Создает кольцевой поток указанной емкости
		/// </summary>
		/// <param name="length">длинна потока</param>
		public HTTPReader(int length) :
			base(length)
		{
			_Frame = new HTTPFrame();
		}
		/// <summary>
		/// считывает из потока тело http запроса и записывает их в header.Body
		/// </summary>
		/// <returns>-1 если тело не былио получено</returns>
		/// <exception cref="HTTPException">Ошибка http протокола</exception>
		public override int ReadBody()
		{
			if (_Frame.Handl == 0)
			{
				header.SetEnd();
				_Frame.GetBody = true;
				return 0;
			}

			int read = 0;
			int @char = 0;
			while (!Empty)
			{
				@char = Buffer[PointR];
				switch ( _Frame.Handl )
				{
					// Записывает тело запроса
					case 1:
						header.Body[_Frame.bpart] = (byte)@char;
						break;
					// Записывает окончание запроса
					case 2:
						if (@char == CR)
						{
							_Frame.Handl = 3;
							break;
						}
						else
							_Frame.Param += char.ToLower((char)@char);
						break;
					// Проверяет правильность окончания длинны
					case 3:
						if (@char != LF)
							throw new HTTPException("отсутсвует символ[LF]", HTTPCode._400_);
						if (!int.TryParse(_Frame.Param, out _Frame.bleng))
							throw new HTTPException("Неверная длинна тела.", HTTPCode._400_);
						
						_Frame.Handl = 1;
						if (_Frame.bleng > LENCHUNK)
							throw new HTTPException("Превышена длинна тела", HTTPCode._400_);
						else
							header.Body = new byte[_Frame.bleng];
						break;
					case 4:
						if (@char == CR)
							_Frame.Handl = 5;
						else
							throw new HTTPException("отсутсвует символ[CR]", HTTPCode._400_);
						break;
					case 5:
						if (@char == LF)
						{
							_Frame.Pcod = 1;
							_Frame.Handl = 6;
						}
						else
							throw new HTTPException("отсутсвует символ[LF]", HTTPCode._400_);
						break;
					case 6:
						if (@char == CR)
							_Frame.Handl = 7;
						else
						{
							_Frame.Param += char.ToLower((char)@char);
							return read;
						}
						break;
					case 7:
						if (@char == LF)
						{
							_Frame.Pcod = 0;
							return read;
						}
						else
							throw new HTTPException("отсутсвует символ[LF]", HTTPCode._400_);

				}
				read++;
				PointR++;
				_Frame.bpart++;
				_Frame.alleng++;

				if (_Frame.bpart == _Frame.bleng)
				{
					header.SegmentsBuffer.Enqueue(header.Body);
					if (!string.IsNullOrEmpty(  _Frame.Param  ))
						_Frame.Handl = 4;
					else
					{
						header.SetEnd();
						_Frame.GetBody = true;
						return read;
					}
				}
			}
			read = -1;
			return read;
		}
		/// <summary>
		/// считывает из потока заголовки http запроса и записывает их в header
		/// </summary>
		/// <returns>-1 если заголвки не были получены</returns>
		/// <exception cref="HTTPException">Ошибка http протокола</exception>
		public override int ReadHead()
		{
			int read = 0;
			int @char = 0;
			
			while (!Empty)
			{
				if (_Frame.hleng > LENHEAD)
					throw new HTTPException( "Превышена длинна заголовков", HTTPCode._400_ );

				@char = Buffer[PointR];
				switch ( _Frame.Handl )
				{
					// получает стартовую строку
					case 0:
						if (_Frame.ststr > STSTR)
							throw new HTTPException( "Длинна стартовой строки", HTTPCode._400_ );
						if (@char == CR)
						{
							_Frame.Handl = 4;
							header.StartString = _Frame.StStr;
								   _Frame.StStr = string.Empty;
						}
						else
						{
							_Frame.ststr++;
							_Frame.StStr += char.ToLower((char)@char);
							if (@char == SPACE)
							{
								_Frame.Hand++;
								if (_Frame.Hand == 2)
									ParsePath( header.Path, header );
									if (_Frame.Hand > 2)
										header.Http +=
										   char.ToLower((char)@char);
						}
							else
							{
								switch (_Frame.Hand)
								{
									case 1:
										header.Path +=
										   char.ToLower((char)@char);
										break;
									case 2:
										header.Http +=
										   char.ToLower((char)@char);
										break;
									case 0:
										header.Method +=
										   char.ToUpper((char)@char);
										break;
								}
							}
						}
						break;
					// получает параметр заголовка
					case 1:
						if (_Frame.param > PARAM)
							throw new HTTPException( "Длинна параметра заголовка", HTTPCode._400_ );
						if (@char == CN)
						{
                            _Frame.Handl = 2;
						}
						else
						{
							_Frame.param++;
							_Frame.Param += char.ToLower((char)@char);
						}
						break;
					// получает значение заголовка
					case 2:
						if (_Frame.value > VALUE)
							throw new HTTPException( "Длинна значения заголовка", HTTPCode._400_ );
						if (@char == CR)
						{
							_Frame.param = 0;
							_Frame.value = 0;
							_Frame.Handl = 4;
							header.AddHeader(_Frame.Param, _Frame.Value);
							_Frame.Param = string.Empty;
							_Frame.Value = string.Empty;
						}
						else
						{
							_Frame.value++;
							_Frame.Value += ( char )@char;
						}
						break;
					// проверяет получены или нет заголвоки
					case 3:
						if (@char == CR)
							_Frame.Handl = 5;
						else
						{
							_Frame.Handl = 1;
							if (@char == CN)
                            	_Frame.Handl = 2;
							else
							{
								_Frame.param++;
								_Frame.Param += char.ToLower((char)@char);
							}
						}
						break;
					// проверяет правильность окончания заголовка
					case 4:
						if (@char == LF)
							_Frame.Handl = 3;
						else
							throw new HTTPException( "Отсутствует символ [LF]", HTTPCode._400_ );
						break;
					// проверяет правильность окончания заголовков
					case 5:
						if (@char == LF)
						{
							_Frame.Handl = 0;
							header.SetReq ();
							_Frame.GetHead = true;
						}
						else
							throw new HTTPException( "Отсутствует символ [LF]", HTTPCode._400_ );
						
						// длинна тела запроса
						if (header.ContentLength > 0)
						{
							_Frame.Handl = 1;
							_Frame.bleng = header.ContentLength;
							header.Body = new byte[header.ContentLength];
							
						}
						// данные будут приходить по кускам
						if ( !string.IsNullOrEmpty(header.TransferEncoding) )
						{
							_Frame.Handl = 2;
							if (_Frame.bleng  >  0)
								throw new HTTPException( "Длинна тела задана не явно", HTTPCode._400_ );
							
						}
						break;
				}
					PointR++;
					_Frame.hleng++;
					_Frame.alleng++;
				
				if (_Frame.GetHead && header.IsReq )
					return read;
			}
            read = -1;
			return read;
		}
		public static void ParsePath(string strdata, IHeader header)
		{
			int index = strdata.LastIndexOf('.');
			if (index == -1)
				header.File = "html";
			else
				header.File = strdata.Substring(index + 1);
		}
	}
}
