using System;
using System.IO;

namespace ChatConnect.Tcp.Protocol.WS
{
	class WStreamSample : WStream
	{
		public WSFrameSample Frame = 
				  new WSFrameSample();
		public WStreamSample(int length)
		{
			_len = length;
			_buffer  =  new byte[length];
		}
		public override int ReadBody()
		{
			return -1;
		}
		public override int ReadHead()
		{
			int _read = 0;
			int _byte = -1;

			while ((_byte = ReadByte()) > -1)
			{
				switch (Frame.Handler)
				{
					case 0:
						/*       FIN - доставка сообщения     */
						Frame.BitFin = (int)((uint)_byte >> 7);
						/*      RCV1 - устанавливается сервером.     */
						Frame.BitRsv1 = (int)((uint)_byte << 25 >> 31);
						/*      RCV2 - устанавливается сервером.     */
						Frame.BitRsv2 = (int)((uint)_byte << 26 >> 31);
						/*      RCV3 - устанавливается сервером.     */
						Frame.BitRsv3 = (int)((uint)_byte << 27 >> 31);
						/*      Опкод-хранит информацию о данных     */
						Frame.BitPcod = (int)((uint)_byte << 28 >> 28);

						/* Общая длинна  */
						Frame.LengHead = 2;
						/* Длинна ответа */
						Frame.PartHead = 0;
						/* Длинна ответа */
						Frame.PartBody = 0;
						/*  Обработчик.  */
						Frame.Handler += 1;

						break;
					case 1:
						/*      Бит маски тела сообщения      */
						Frame.BitRsv4 = (int)((uint)_byte >> 7);
						/*     Длинна полученного тела сообщения     */
						Frame.BitLeng = (int)((uint)_byte << 25 >> 25);

						if (Frame.BitLeng == 127)
						{
							Frame.Handler += 1;
							Frame.LengHead += 8;
						}
						else if (Frame.BitLeng == 126)
						{
							Frame.Handler += 7;
							Frame.LengHead += 2;
						}
						else if (Frame.BitLeng <= 125)
						{
							Frame.Handler += 9;
							Frame.LengBody = Frame.BitLeng;
						}

						break;
					case 2:
						/*  Обработчик.  */
						Frame.Handler += 1;
						Frame.LengBody = (int)((uint)_byte << 56);
						break;
					case 3:
						/*  Обработчик.  */
						Frame.Handler += 1;
						Frame.LengBody = (int)((uint)Frame.LengBody | (uint)_byte << 48);
						break;
					case 4:
						/*  Обработчик.  */
						Frame.Handler += 1;
						Frame.LengBody = (int)((uint)Frame.LengBody | (uint)_byte << 40);
						break;
					case 5:
						/*  Обработчик.  */
						Frame.Handler += 1;
						Frame.LengBody = (int)((uint)Frame.LengBody | (uint)_byte << 32);
						break;
					case 6:
						/*  Обработчик.  */
						Frame.Handler += 1;
						Frame.LengBody = (int)((uint)Frame.LengBody | (uint)_byte << 24);
						break;
					case 7:
						/*  Обработчик.  */
						Frame.Handler += 1;
						Frame.LengBody = (int)((uint)Frame.LengBody | (uint)_byte << 16);
						break;
					case 8:
						/*  Обработчик.  */
						Frame.Handler += 1;
						Frame.LengBody = (int)((uint)Frame.LengBody | (uint)_byte << 08);
						break;
					case 9:
						/*  Обработчик.  */
						Frame.Handler += 1;
						Frame.LengBody = (int)((uint)Frame.LengBody | (uint)_byte << 00);
						break;
					case 10:
						/*  Обработчик.  */
						Frame.Handler += 1;
						Frame.LengExtn = (int)((uint)Frame.LengExtn | (uint)_byte << 08);
						break;
					case 11:
						Frame.LengExtn = (int)((uint)Frame.LengExtn | (uint)_byte << 00);
						break;
				}

				_read++;
				Frame.PartHead++;

				if (Frame.PartHead == Frame.LengHead)
				{
					Frame.GetsHead = true;
					return _read;
				}
			}

			_read = -1;
			return _read;
		}
	}
}
