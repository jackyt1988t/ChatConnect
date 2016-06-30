using System;

namespace MyWebSocket.Tcp.Protocol.HTTP
{
	public class HTTPReader : MyStream
	{
		const int LF = 0x0A;
		const int CR = 0x0D;
		const int CN = 0x3A;
		const int SP = 0x20; 
		/// <summary>
		/// Допустимая длинна с. с.
		/// </summary>
		public static int STSTR;
		/// <summary>
		/// Допустимая длинна парметра
		/// </summary>
		public static int PARAM;
		/// <summary>
		/// Допустимая длинна значения
		/// </summary>
		public static int VALUE;
		/// <summary>
		/// Допустимая длинна заголовков
		/// </summary>
		public static int LENHEAD;
		/// <summary>
		/// Допустимая длинна блока Transfer-Encoding
		/// </summary>
		public static int LENCHUNK;

		/// <summary>
		/// Инфыормация о заголвоках входящего запроса
		/// </summary>
		public Header Header;
		/// <summary>
		/// Информация о текщем состоянии чтения запроса
		/// </summary>
		public HTTPFrame _Frame;
		/// <summary>
		/// Статический конструктор
		/// </summary>
		static HTTPReader()
		{
			STSTR = 1024;
			PARAM = 1024;
			VALUE = 1024;
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
		public bool ReadBody()
		{
			// нет тела запроса
			if (_Frame.Handl == 0)
			{
				Header.SetEnd();
				return (_Frame.GetBody = true);
			}

			int @char = 0;
			while (!Empty)
			{
				@char = Buffer[PointR];
				switch ( _Frame.Handl )
				{
					// Записывает тело запроса
					case 1:
						if (Header.Body == null)
							Header.Body = new byte[_Frame.bleng];
						
						int __read = Read(Header.Body, _Frame.bpart,
													   _Frame.bleng - _Frame.bpart);

						_Frame.bpart  += __read;
						_Frame.alleng += __read;
						if (_Frame.bpart != _Frame.bleng)
							return _Frame.GetBody;
						else
						{
							Header.SegmentsBuffer.Enqueue(Header.Body);
							if (!string.IsNullOrEmpty( _Frame.Param ))
								_Frame.Handl = 4;
							else
							{
								Header.SetEnd();
								return (_Frame.GetBody = true);
							}
						}
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
							Header.Body = new byte[_Frame.bleng];
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
						}
						break;
					case 7:
						if (@char == LF)
						{
							_Frame.Pcod = 0;
							return (_Frame.GetBody = true);
						}
						else
							throw new HTTPException("отсутсвует символ[LF]", HTTPCode._400_);

				}
				PointR++;
				_Frame.bpart++;
				_Frame.alleng++;
			}
			return false;
		}
		/// <summary>
		/// считывает из потока заголовки http запроса и записывает их в header
		/// </summary>
		/// <returns>-1 если заголвки не были получены</returns>
		/// <exception cref="HTTPException">Ошибка http протокола</exception>
		public bool ReadHead()
		{
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
						if (!ReadStStr())
							return false;
						break;
					// получает параметр заголовка
					case 1:
						if (!ReadParam())
							return false;
						break;
					// получает значение заголовка
					case 2:
						if (!ReadValue())
							return false;
						break;
					// проверяет получены или нет заголвоки
					case 3:
						Header.StrStr = _Frame.StStr;
										_Frame.StStr = string.Empty;
						//////Окончание//////
						if (@char != CR)
						{
							if (!ReadParam())
								return false;
						}
						else
							_Frame.Handl = 5;
						//////Окончание//////
						break;
					// проверяет перенос заголвока, 
					// проверяет получены или нет заголвоки
					case 7:
						if (@char == SP)
						{
							_Frame.Handl = 2;
						}
						else
						{
						// добавляем заголвоки и очищаем зависимости
						_Frame.param = 0;
						_Frame.value = 0;
						Header.AddHeader(_Frame.Param, _Frame.Value);
										 _Frame.Param = string.Empty;
										 _Frame.Value = string.Empty;
						//////Окончание//////
						if (@char != CR)
						{
							if (!ReadParam())
								return false;
						}
						else
							_Frame.Handl = 5;
						//////Окончание//////
						}
						break;
					// проверяет правильность окончания заголовка 
					case 6:
						if (@char == LF)
							_Frame.Handl = 7;
						else
							throw new HTTPException( "Отсутствует символ [LF] в окончании значения заголвока", HTTPCode._400_ );
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
							Header.SetReq();

							_Frame.Handl = 0;
						}
						else
							throw new HTTPException( "Отсутствует символ [LF]", HTTPCode._400_ );
						
						// длинна тела запроса
						if (Header.ContentLength > 0)
						{
							_Frame.Handl = 1;
							_Frame.bleng = Header.ContentLength;	
						}
						// данные будут приходить по кускам
						if ( !string.IsNullOrEmpty(Header.TransferEncoding) )
						{
							_Frame.Handl = 2;
							if (_Frame.bleng > 0)
								throw new HTTPException( "Длинна тела задана не явно", HTTPCode._400_ );
							
						}
						break;
				}
				PointR++;
				_Frame.hleng++;
				_Frame.alleng++;

				if (Header.IsReq)
					return (_Frame.GetHead = true);
			}

			return false;
		}
		private bool ReadParam()
		{
			_Frame.Handl = 1;

			int @char = 0;
			while (!Empty)
			{
				@char = Buffer[PointR];
				if (_Frame.param > PARAM)
					throw new HTTPException( "Длинна параметра заголовка", HTTPCode._400_ );
				if (@char < 33
				 || @char > 125)
					throw new HTTPException( "Символ параметра заголовка", HTTPCode._400_ );
				if (@char == CN)
				{
                    _Frame.Handl = 2;
					return true;
				}
				else
				{
					_Frame.param++;
					_Frame.Param += char.ToLower((char)@char);
				}
				PointR++;
				_Frame.hleng++;
				_Frame.alleng++;
			}
			return false;
		}
		private bool ReadValue()
		{
			_Frame.Handl = 2;

			int @char = 0;
			while (!Empty)
			{
				@char = Buffer[PointR];
				if (_Frame.value > VALUE)
					throw new HTTPException( "Длинна значения заголовка", HTTPCode._400_ );
				if (@char == CR)
				{
					_Frame.Handl = 6;
					return true;
				}
				else
				{
					_Frame.value++;
					_Frame.Value += ( char )@char;
				}
				PointR++;
				_Frame.hleng++;
				_Frame.alleng++;
			}
			return false;
		}
		private bool ReadStStr()
		{
			_Frame.Handl = 0;

			int @char = 0;
			while (!Empty)
			{
				@char = Buffer[PointR];
				if (_Frame.ststr > STSTR)
					throw new HTTPException( "Длинна стартовой строки", HTTPCode._400_ );
				if (@char == CR)
				{
					_Frame.Handl = 4;
					return true;
				}
				else
				{
					_Frame.ststr++;
					_Frame.StStr += char.ToLower((char)@char);
					if (@char == SP)
					{
						_Frame.Hand++;
						if (_Frame.Hand >= 3)
							Header.Http += char.ToLower((char)@char);
					}
					else
					{
						switch (_Frame.Hand)
						{
							case 1:
								Header.Path += char.ToLower((char)@char);
									break;
							case 2:
								Header.Http += char.ToLower((char)@char);
								break;
							case 0:
								Header.Method += char.ToUpper((char)@char);
								break;
						}
					}
				}
				PointR++;
				_Frame.hleng++;
				_Frame.alleng++;
			}
			return false;
		}
	}
}
