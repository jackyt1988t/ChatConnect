using System;

namespace ChatConnect.Tcp.Protocol.HTTP
{
	class HTTPStream : StreamS
	{
		const int LF = 0x0A;
		const int CR = 0x0D;
		const int CN = 0x3A;
		const int SPACE = 0x20; 
		const int STSTR = 1024;
		const int PARAM = 1024;
		const int VALUE = 1024;

		public static readonly byte[] ENDCHUNCK;
		public static readonly byte[] EOFCHUNCK;

		public IHeader header;
		public HTTPFrame frame;

		static HTTPStream()
		{
			ENDCHUNCK =
				new byte[] { 0x0D, 0x0A };
			EOFCHUNCK = 
				new byte[] { 0x30, 0x0D, 0x0A, 0x0D, 0x0A };
		}
		public HTTPStream(int length)
		{
			frame = new HTTPFrame();
			
			_len = length;
			_buffer = new byte[length];
		}
		public override int ReadBody()
		{
			int read = 0;
			int _char = 0;
			
			if (frame.Handl == 0)
			{
				frame.GetBody = true;
				return read;
			}
			while ((_char = ReadByte()) > -1)
			{
				switch (frame.Handl)
				{
					case 1:
						frame.DataBody[frame.bpart] = (byte)_char;
						break;
					case 2:
						if (_char == CR)
						{
							frame.Handl = 3;
							break;
						}
						else
							frame.Param += char.ToLower((char)_char);
						break;
					case 3:
						if (_char != LF)
							throw new HTTPException("отсутсвует символ[LF]");
						if (!int.TryParse(frame.Param, out frame.bleng))
							throw new HTTPException("Неверная длинна тела.");
						
						frame.Handl = 1;
						frame.DataBody = new byte[frame.bleng];
						break;
					case 4:
						if (_char == CR)
							frame.Handl = 5;
						else
							throw new HTTPException("отсутсвует символ[CR]");
						break;
					case 5:
						if (_char == LF)
						{
							frame.Pcod = 1;
							frame.handl = 6;
						}
						else
							throw new HTTPException("отсутсвует символ[LF]");
						break;
					case 6:
						if (_char == CR)
							frame.handl = 7;
						else
						{
							frame.Param += char.ToLower((char)_char);
							return read;
						}
						break;
					case 7:
						if (_char == LF)
						{
							frame.Pcod = 0;
							return read;
						}
						else
							throw new HTTPException("отсутсвует символ[LF]");
						break;

				}
				read++;
				frame.bpart++;

				if (frame.bpart == frame.bleng)
				{
					header.SegmentsBuffer.Enqueue(
							       frame.DataBody);
					if (!string.IsNullOrEmpty(frame.Param))
						frame.Handl = 4;
					else
					{
						frame.GetBody = true;
						return read;
					}
				}
			}
			read = -1;
			return read;
		} 
		public override int ReadHead()
		{
			int read = 0;
			int _char = 0;
			
			while ( (_char = ReadByte() ) > -1)
			{
				if ( frame.hleng > 36000 )
					throw new HTTPException("Превышена длинна заголовков");
					
				switch (frame.Handl)
				{
					case 0:
						if (frame.ststr > STSTR)
							throw new HTTPException( "Длинна стартовой строки" );
						if (_char == CR)
						{
							frame.Handl = 4;
							header.StartString = frame.StStr;
								   frame.StStr = string.Empty;
						}
						else
						{
							frame.ststr++;
							frame.StStr += char.ToLower((char)_char);
							if (_char == SPACE)
							{
								frame.Hand++;
								if (frame.Hand == 2)
									ParsePath( header.Path, header );
								if (frame.Hand  > 2)
									throw new HTTPException("Некорректная стартовая строка");
							}
							else
							{
								switch (frame.Hand)
								{
									case 0:
										header.Method +=
										   char.ToLower((char)_char);
										break;
									case 1:
										header.Path +=
										   char.ToLower((char)_char);
										break;
									case 2:
										header.Http +=
										   char.ToLower((char)_char);
										break;
								}
							}
						}
						break;
					case 1:
						if (frame.param > PARAM)
							throw new HTTPException("Длинна параметра заголовка");
						if (_char == CN)
                            frame.Handl = 2;
						else
						{
							frame.param++;
							frame.Param += char.ToLower((char)_char);
						}
						break;
					case 2:
						if (frame.value > VALUE)
							throw new HTTPException("Длинна значения заголовка");
						if (_char == CR)
						{
							frame.param = 0;
							frame.value = 0;
							frame.Handl = 4;
							string param = 
								frame.Param.Trim(new char[] { ' ' });
							string value =
								frame.Value.Trim(new char[] { ' ' });
							if (header.ContainsKey(param))
								header[param] = header[param]  +  ";"  +  value;
							else
								header.Add(param, value);

							frame.Param  =  string.Empty;
							frame.Value  =  string.Empty;
						}
						else
						{
							frame.value++;
							frame.Value += ( char )_char;
						}
						break;
					case 3:
						if (_char == CR)
						{
							frame.Handl = 5;
							break;
						}
						else
						{
							frame.Handl = 1;
							goto case 1;
						}
					case 4:
						if (_char == LF)
						{
							frame.Handl = 3;
							break;
						}
						else
							throw new HTTPException( "Отсутствует символ [LF]" );
					case 5:
						if (_char == LF)
						{
							frame.GetHead = true;
							return read;
						}
						else
							throw new HTTPException( "Отсутствует символ [LF]" );
				}
				frame.hleng++;
			}
            read = -1;
			return read;
		}
		public static  void  ParsePath(string strdata, IHeader header)
		{
			int index = strdata.LastIndexOf('.');
			if (index == -1)
				header.File = "html";
			else
				header.File = strdata.Substring(index + 1);
		}
	}
}
