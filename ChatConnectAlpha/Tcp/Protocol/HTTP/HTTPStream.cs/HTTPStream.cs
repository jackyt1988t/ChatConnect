using System;
using System.IO;
using System.IO.Compression;
			using System.Text;

namespace ChatConnect.Tcp.Protocol.HTTP
{
	class HTTPStream
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

		public int Length;
        public int Position;
        public byte[] __Buffer;

		static HTTPStream()
		{
			ENDCHUNCK =
				new byte[] { 0x0D, 0x0A };
			EOFCHUNCK = 
				new byte[] { 0x30, 0x0D, 0x0A, 0x0D, 0x0A };
		}
		public HTTPStream(byte[] buffer)
		{
			Position = 0;
			__Buffer = buffer;
				Length = buffer.Length;
		}
		public HTTPStream(byte[] buffer, int pos)
		{
			Position = pos;
			__Buffer = buffer;
				Length = buffer.Length;
		}
		public HTTPStream(byte[] buffer, int pos, int length)
		{
			Position = pos;
			__Buffer = buffer;
				Length = length;
		}
		public byte[] ToArray()
		{
			byte[] arr = new byte[Length - Position];
			Array.Copy(__Buffer, Position, arr, 0, arr.Length);

			return arr;
		}
		public   int   ReadByte()
		{
            if (__Buffer == null)
                throw new ArgumentNullException( "DataBuffer" );
            if (Position >= __Buffer.Length)
                return -1;

            return __Buffer[  Position++  ];
        }
		public   int   ReadBody(ref HTTPFrame frame, IHeader header)
		{
			int _char = 0;
			int _reads = 0;
			if (frame.Handl == 0)
			{
				frame.GetBody = true;
				return _reads;
			}
			while ((_char = ReadByte()) > -1)
			{
				switch (frame.Handl)
				{
					case 1:
						if (frame.DataBody == null)
							frame.DataBody = new byte [frame.bleng];

						frame.DataBody[frame.bpart] = ( byte )_char;
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

						goto case 4;
					case 4:
						if (_char != CR)
						{
							frame.Pcod = HTTPFrame.DATA;
							goto case 2;
						}
						else
						{
							frame.Pcod = HTTPFrame.CHUNK;
							frame.Handl = 5;
						}
						break;
					case 5:
						if (_char == LF)
							throw new HTTPException("отсутсвует символ[LF]");
						break;

				}
				_reads++;
				frame.bpart++;

				if (frame.bpart == frame.bleng)
				{
					if (!string.IsNullOrEmpty(frame.Param))
						frame.Handl = 3;
					else
						frame.GetBody = true;

					// Добавлем в тело данных запроса
					header.SegmentsBuffer.Enqueue(frame.DataBody);

					return _reads;
				}
			}
			_reads = -1;
			return _reads;
		} 
		public   int   ReadHead(ref HTTPFrame frame, IHeader header)
		{
			int _char = 0;
			int _reads = 0;
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
							return _reads;
						}
						else
							throw new HTTPException( "Отсутствует символ [LF]" );
				}
				frame.hleng++;
			}
            _reads = -1;
			return _reads;
		}
		public static  void  ParsePath(string strdata, IHeader header)
		{
			int index = strdata.LastIndexOf('.');
			if (index == -1)
				header.File = "html";
			else
				header.File = strdata.Substring(index + 1);
		}
		public static  void  WriteBody(byte[] chunkdata, IHeader header)
		{
			using (MemoryStream memory = new MemoryStream())
			{
				using (GZipStream gzipstream = new GZipStream(memory, CompressionMode.Compress))
				{
					gzipstream.Write(chunkdata, 0, chunkdata.Length);
				}
				chunkdata = memory.ToArray();
			}
			string hex = chunkdata.Length.ToString("X");
			byte[] _hexsdata = Encoding.UTF8.GetBytes(hex);

			lock (header.SegmentsBuffer)
			{
				header.SegmentsBuffer.Enqueue( _hexsdata );
				header.SegmentsBuffer.Enqueue( ENDCHUNCK );
				header.SegmentsBuffer.Enqueue( chunkdata );
				header.SegmentsBuffer.Enqueue( ENDCHUNCK );
			}
		}
	}
}