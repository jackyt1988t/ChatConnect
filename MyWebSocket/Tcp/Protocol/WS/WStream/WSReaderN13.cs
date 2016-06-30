using System;

namespace MyWebSocket.Tcp.Protocol.WS
{
    class WSReaderN13 : MyStream
    {
		byte _ngHead;
		public WSN13 _Frame;

		public WSReaderN13() : 
			base(1024)
		{
			_Frame = new WSN13();
		}
		public WSReaderN13(int length) :
			base(length)
        {
			_Frame  =  new WSN13();
        }

		public override void Reset()
		{
			base.Reset();
			_Frame.Null();
		}
		unsafe public bool ReadBody()
        {
			if (  _Frame.BitLeng == 0  )
				return true;
            if (  _Frame.BitMask == 0  )
            {
				fixed (byte* sourse = _buffer, target = _Frame.DataBody)
				{
					byte* ps = sourse + PointR;
					byte* pt = target + _Frame.PartBody;

					while (!Empty)
					{
						*pt = *ps;
						ps++;
						pt++;						
						PointR++;

						if (++_Frame.PartBody == _Frame.LengBody)
						{
							return (_Frame.GetsBody = true);
						}
					}
				}
			}
            else
            {
				fixed (byte * sourse = _buffer, target = _Frame.DataBody)
				{
					byte* ps = sourse + PointR;
					byte* pt = target + _Frame.PartBody;

					while (!Empty)
					{
						*pt = (byte)(*ps ^ _Frame.DataMask[_Frame.PartBody % 4]);
						ps++;
						pt++;
						PointR++;

						if (++_Frame.PartBody == _Frame.LengBody)
						{
							return (_Frame.GetsBody = true);
						}
					}
				}
            }
            return false;
        }
        public bool ReadHead()
        {
            while (!Empty)
            {
                switch (_Frame.Handler)
                {
                    case 0:

						_Frame.BitFin  = (Buffer[PointR] & 0x80) >> 7;
                        _Frame.BitRsv1 = (Buffer[PointR] & 0x40) >> 6;
						_Frame.BitRsv2 = (Buffer[PointR] & 0x20) >> 5;
						_Frame.BitRsv3 = (Buffer[PointR] & 0x10) >> 4;
                        _Frame.BitPcod = (Buffer[PointR] & 0x0F);

                        /* Общая длинна  */
                        _Frame.LengHead = 2;
                        /* Длинна ответа */
                        _Frame.PartHead = 0;
                        /* Длинна ответа */
                        _Frame.PartBody = 0;
                        /*  Обработчик.  */
                        _Frame.Handler += 1;

						_ngHead = Buffer[PointR];
						break;
                    case 1:
						_Frame.BitMask = (Buffer[PointR] & 0x80) >> 7;
                        _Frame.BitLeng = (Buffer[PointR] & 0x7F);
			
						if (_Frame.BitMask == 1)
							_Frame.LengHead += 4;
                        if (_Frame.BitLeng == 127)
                        {
                            _Frame.Handler += 1;
                            _Frame.LengHead += 8;
                        }
                        else if (_Frame.BitLeng == 126)
                        {
                            _Frame.Handler += 7;
                            _Frame.LengHead += 2;
                        }
                        else if (_Frame.BitLeng <= 125)
                        {
                            _Frame.Handler += 9;
                            _Frame.LengBody = _Frame.BitLeng;
                        }
						
						_Frame.DataHead = 
							new byte[_Frame.LengHead];
						_Frame.DataHead[0] = _ngHead;
						_Frame.DataHead[_Frame.PartHead] = Buffer[PointR];
						
						break;
                    case 2:
                        /*  Обработчик.  */
                        _Frame.Handler += 1;
						_Frame.DataHead[_Frame.PartHead] = Buffer[PointR];
                                         _Frame.LengBody = (long)(Buffer[PointR] << 56);
                        break;
                    case 3:
                        /*  Обработчик.  */
                        _Frame.Handler += 1;
						_Frame.DataHead[_Frame.PartHead] = Buffer[PointR];
						_Frame.LengBody = _Frame.LengBody | (long)(Buffer[PointR] << 48);
                        break;
                    case 4:
                        /*  Обработчик.  */
                        _Frame.Handler += 1;
						_Frame.DataHead[_Frame.PartHead] = Buffer[PointR];
						_Frame.LengBody = _Frame.LengBody | (long)(Buffer[PointR] << 40);
                        break;
                    case 5:
                        /*  Обработчик.  */
                        _Frame.Handler += 1;
						_Frame.DataHead[_Frame.PartHead] = Buffer[PointR];
						_Frame.LengBody = _Frame.LengBody | (long)(Buffer[PointR] << 32);
                        break;
                    case 6:
                        /*  Обработчик.  */
                        _Frame.Handler += 1;
						_Frame.DataHead[_Frame.PartHead] = Buffer[PointR];
						_Frame.LengBody = _Frame.LengBody | (long)(Buffer[PointR] << 24);
                        break;
                    case 7:
                        /*  Обработчик.  */
                        _Frame.Handler += 1;
						_Frame.DataHead[_Frame.PartHead] = Buffer[PointR];
						_Frame.LengBody = _Frame.LengBody | (long)(Buffer[PointR] << 16);
                        break;
                    case 8:
                        /*  Обработчик.  */
                        _Frame.Handler += 1;
						_Frame.DataHead[_Frame.PartHead] = Buffer[PointR];
						_Frame.LengBody = _Frame.LengBody | (long)(Buffer[PointR] << 08);
                        break;
                    case 9:
                        /*  Обработчик.  */
                        _Frame.Handler += 1;
						_Frame.DataHead[_Frame.PartHead] = Buffer[PointR];
						_Frame.LengBody = _Frame.LengBody | (long)(Buffer[PointR] << 00);
                        break;
                    case 10:
                        /*  Обработчик.  */
                        _Frame.Handler += 1;
						_Frame.DataMask = new byte[4];
						_Frame.DataMask[0] = Buffer[PointR];
						_Frame.DataHead[_Frame.PartHead] = Buffer[PointR];
						
                                          		_Frame.MaskVal = (Buffer[PointR] << 24);
                        break;
                    case 11:
                        /*  Обработчик.  */
                        _Frame.Handler += 1;
						_Frame.DataMask[1] = Buffer[PointR];
						_Frame.DataHead[_Frame.PartHead] = Buffer[PointR];
						
                        		_Frame.MaskVal = _Frame.MaskVal | (Buffer[PointR] << 16);
                        break;
                    case 12:
                        /*  Обработчик.  */
                        _Frame.Handler += 1;
						_Frame.DataMask[2] = Buffer[PointR];
						_Frame.DataHead[_Frame.PartHead] = Buffer[PointR];
						
								_Frame.MaskVal = _Frame.MaskVal | (Buffer[PointR] << 08);
                        break;
                    case 13:
						_Frame.DataMask[3] = Buffer[PointR];
						_Frame.DataHead[_Frame.PartHead] = Buffer[PointR];
								_Frame.MaskVal = _Frame.MaskVal | (Buffer[PointR] << 00);
                        break;
                }

				PointR++;
                _Frame.PartHead++;

                if (_Frame.PartHead == _Frame.LengHead)
                {
					if (_Frame.LengBody > -1)
						_Frame.DataBody = new byte[_Frame.LengBody];
					return (_Frame.GetsHead = true);
                }
            }
            return false;
        }
    }
	
}
