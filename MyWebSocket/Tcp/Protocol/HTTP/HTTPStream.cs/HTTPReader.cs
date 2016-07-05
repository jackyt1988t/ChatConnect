using System;
using System.IO;
using System.Text;

namespace MyWebSocket.Tcp.Protocol.HTTP
{
	public class HTTPReader
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
		/// Базовый поток
		/// </summary>
		public Stream Stream;
		/// <summary>
		/// Храналищи данных, данные входящего запроса 
		/// </summary>
		public Stream Archiv;
		/// <summary>
		/// Информация о текщем состоянии чтения запроса
		/// </summary>
		public HTTPFrame __Frame;
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
		/// Создает поток
		/// </summary>
		/// <param name="stream">кольцевой потока</param>
		public HTTPReader(Stream stream)
		{
			Stream = stream;
			Archiv = new MyStream(0)
			__Frame = new HTTPFrame();
		}
		/// <summary>
		/// считывает из потока тело http запроса и записывает их в header.Body
		/// </summary>
		/// <returns>-1 если тело не былио получено</returns>
		/// <exception cref="HTTPException">Ошибка http протокола</exception>
		public bool ReadBody()
		{
			// нет тела запроса
			if (__Frame.Handl == 0)
			{
				Header.SetEnd();
				return (__Frame.GetBody = true);
			}
			int @char = 0;
			while ((@char = Stream.ReadByte()) > -1)
			{
				switch ( __Frame.Handl )
				{
					// Записывает тело запроса
					case 1:
								if (Header.Body == null)
									Header.Body  =  new byte[ __Frame.bleng ];
						
												 Header.Body[ __Frame.bpart++ ]=(byte)@char;
						int __read = Stream.Read(Header.Body, __Frame.bpart,
													          __Frame.bleng - __Frame.bpart);

						__Frame.bpart  += __read;
						__Frame.alleng += __read;
						if (__Frame.bpart != __Frame.bleng)
							return __Frame.GetBody;
						else
						{
							Archiv.Write(Header.Body, 0, __Frame.bleng);
							if (!string.IsNullOrEmpty( 
											 __Frame.Param.ToString()))
								__Frame.Handl = 4;
							else
							{
								Header.SetEnd();
								return (__Frame.GetBody = true);
							}
						}
						break;
					// Записывает окончание запроса
					case 2:
						if (@char == CR)
						{
							__Frame.Handl = 3;
							break;
						}
						else
							__Frame.Param.Append((char)@char);
						break;
					// Проверяет правильность окончания длинны
					case 3:
						if (@char != LF)
							throw new HTTPException("отсутсвует символ[LF]", HTTPCode._400_);
						if (!int.TryParse(__Frame.Param.ToString(), out __Frame.bleng))
							throw new HTTPException("Неверная длинна тела.", HTTPCode._400_);
						
						__Frame.Handl = 1;
						__Frame.Param.Clear();
						if (__Frame.bleng > LENCHUNK)
							throw new HTTPException("Превышена длинна тела", HTTPCode._400_);
						break;
					case 4:
						if (@char == CR)
							__Frame.Handl = 5;
						else
							throw new HTTPException("отсутсвует символ[CR]", HTTPCode._400_);
						break;
					case 5:
						if (@char == LF)
						{
							__Frame.Pcod = 1;
							__Frame.Handl = 6;
						}
						else
							throw new HTTPException("отсутсвует символ[LF]", HTTPCode._400_);
						break;
					case 6:
						if (@char == CR)
							__Frame.Handl = 7;
						else
						{
							__Frame.Param.Append(@char);
						}
						break;
					case 7:
						if (@char == LF)
						{
							__Frame.Pcod = 0;
							return (__Frame.GetBody = true);
						}
						else
							throw new HTTPException("отсутсвует символ[LF]", HTTPCode._400_);

				}
							__Frame.bpart++;
							__Frame.alleng++;
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
			if (__Frame.StStr == null)
				__Frame.StStr = new StringBuilder(STSTR);
			if (__Frame.Param == null)
				__Frame.Param = new StringBuilder(PARAM);
			if (__Frame.Value == null)
				__Frame.Value = new StringBuilder(VALUE);
				
			while ((@char = Stream.ReadByte()) > -1)
			{
				if (__Frame.hleng > LENHEAD)
					throw new HTTPException( "Превышена длинна заголовков", HTTPCode._400_ );

				switch ( __Frame.Handl )
				{
					// получает стартовую строку
					case 0:
						ReadStStr(@char);
						break;
					// получает параметр заголовка
					case 1:
						ReadParam(@char);
						break;
					// получает значение заголовка
					case 2:
						ReadValue(@char);
						break;
					// проверяет получены или нет заголвоки
					case 3:
						Header.StrStr = __Frame.StStr.ToString();
						//////Окончание//////
						if (@char != CR)
						{
							ReadParam(@char);
						}
						else
							__Frame.Handl = 5;
						//////Окончание//////
						break;
					// проверяет перенос заголвока, 
					// проверяет получены или нет заголвоки
					case 7:
						if (@char == SP)
						{
							__Frame.Handl = 2;
						}
						else
						{
						// добавляем заголвоки и очищаем зависимости
						__Frame.param = 0;
						__Frame.value = 0;
						Header.AddHeader(__Frame.Param.ToString(),
										 __Frame.Value.ToString());
							    			 __Frame.Param.Clear();
											 __Frame.Value.Clear();
						//////Окончание//////
						if (@char != CR)
						{
							ReadParam(@char);
						}
						else
							__Frame.Handl = 5;
						//////Окончание//////
						}
						break;
					// проверяет правильность окончания заголовка 
					case 6:
						if (@char == LF)
							__Frame.Handl = 7;
						else
							throw new HTTPException( "Отсутствует символ [LF] в окончании значения заголвока", HTTPCode._400_ );
						break;
					// проверяет правильность окончания заголовка
					case 4:
						if (@char == LF)
							__Frame.Handl = 3;
						else
							throw new HTTPException( "Отсутствует символ [LF]", HTTPCode._400_ );
						break;
					// проверяет правильность окончания заголовков
					case 5:
						if (@char == LF)
						{
							Header.SetReq();
							__Frame.Handl = 0;
						}
						else
							throw new HTTPException( "Отсутствует символ [LF]", HTTPCode._400_ );
						
						// длинна тела запроса
						if (Header.ContentLength > 0)
						{
							__Frame.Handl = 1;
							__Frame.bleng = Header.ContentLength;	
						}
						// данные будут приходить по кускам
						if ( !string.IsNullOrEmpty(Header.TransferEncoding) )
						{
							__Frame.Handl = 2;
							if (__Frame.bleng > 0)
								throw new HTTPException( "Длинна тела задана не явно", HTTPCode._400_ );
							
						}
						break;
				}
				__Frame.hleng++;
				__Frame.alleng++;

				if (Header.IsReq)
					return (__Frame.GetHead = true);
			}

			return false;
		}
		private void ReadParam(int @char)
		{
			__Frame.Handl = 1;
			while (true)
			{
				if (__Frame.param > PARAM)
					throw new HTTPException( "Длинна параметра заголовка", HTTPCode._400_ );
				if (@char < 33
				 || @char > 125)
					throw new HTTPException( "Символ параметра заголовка", HTTPCode._400_ );
				if (@char == CN)
				{
                    			__Frame.Handl = 2;
					break;
				}
				else
				{
					__Frame.param++;
					__Frame.Param.Append((char)@char);
				}
				
				if ((@char = Stream.ReadByte()) == -1)
					break;
					
					__Frame.hleng++;
					__Frame.alleng++;

			}
		}
		private void ReadValue(int @char)
		{
			__Frame.Handl = 2;
			while (true)
			{
				if (__Frame.value > VALUE)
					throw new HTTPException( "Длинна значения заголовка", HTTPCode._400_ );
				if (@char == CR)
				{
					__Frame.Handl = 6;
					break;
				}
				else
				{
					__Frame.value++;
					__Frame.Value.Append((char)@char);
				}
				
				if ((@char = Stream.ReadByte()) == -1)
					break;
					
					__Frame.hleng++;
					__Frame.alleng++;

			}
		}
		private void ReadStStr(int @char)
		{
			__Frame.Handl = 0;
			while (true)
			{
				if (__Frame.ststr > STSTR)
					throw new HTTPException( "Длинна стартовой строки", HTTPCode._400_ );
				if (@char == CR)
				{
					__Frame.Handl = 4;
					break;
				}
				else
				{
					__Frame.ststr++;
					__Frame.StStr.Append((char)@char);
					if (@char == SP)
					{
						
						__Frame.Hand++;
						if (__Frame.Hand >= 3)
							Header.Http += char.ToLower((char)@char);
					}
					else
					{
						switch (__Frame.Hand)
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

				if ((@char = Stream.ReadByte()) == -1)
					break;
					
					__Frame.hleng++;
					__Frame.alleng++;

			}
		}
	}
}
