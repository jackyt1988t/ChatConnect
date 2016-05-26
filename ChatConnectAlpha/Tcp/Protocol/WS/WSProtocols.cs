using System;
using System.Net;
using System.Net.Sockets;

namespace ChatConnect.Tcp.Protocol.WS
{
    class WSProtocol : WS
    {
		public WSChecks WSChecks
		{
			get;
			set;
		}
		public override WStream Reader
		{
			get;
		}
		public override WStream Writer
		{
			get;
		}
		
		/// <summary>
		/// Ининцилазириует класс протокола WS без подключения
		/// </summary>
		public WSProtocol()
		{
			Sync       = new object();
			State      = 
					States.Connection;
			Reader	   = new WStreamSample(1204 * 512);
			Writer	   = new WStreamSample(1204 * 512);
			Response   = new Header();
			WSChecks   = new WSChecks();
			TaskResult = new TaskResult();
			TaskResult.Protocol   =   TaskProtocol.WSRFC76;
		}
		/// <summary>
		/// Инициализрует класс протокола WS с указанным обработчиком
		/// </summary>
		/// <param name="http">протокол  http</param>
		/// <param name="connect">обрабтчик собятия подключения</param>
		public WSProtocol(IProtocol http, PHandlerEvent connect)
        {
			Tcp        = http.Tcp;
			Sync       = new object();
			State      = 
					States.Connection;
			Reader     = new WStreamSample(1204 * 512);
			Writer     = new WStreamSample(1204 * 512);
			Request    = http.Request;
			Response   = new Header();
			WSChecks   = new WSChecks();
			TaskResult = new TaskResult();
			TaskResult.Protocol   =   TaskProtocol.WSRFC76;
		}
		public override bool Message(byte[] message, int recive, int length, WSOpcod opcod, WSFin fin)
		{
			int Fin = 0;
			int Opcod = 0;
			if (fin == WSFin.Last)
				Fin = 0;
			else if (fin == WSFin.Next)
				Fin = 1;
			switch (opcod)
			{
				case WSOpcod.Text:
					Opcod = WSFrameN13.TEXT;
					break;
				case WSOpcod.Ping:
					Opcod = WSFrameN13.PING;
					break;
				case WSOpcod.Pong:
					Opcod = WSFrameN13.PONG;
					break;
				case WSOpcod.Close:
					Opcod = WSFrameN13.CLOSE;
					break;
				case WSOpcod.Binnary:
					Opcod = WSFrameN13.BINNARY;
					break;
				case WSOpcod.Continue:
					Opcod = WSFrameN13.CONTINUE;
					break;
			}
			WSFrameSample frame = new WSFrameSample()
			{
				BitFin   = Fin,
				BitPcod  = Opcod,
				PartBody = recive,
				LengBody = length,
				DataBody = message
			};
			lock (Writer)
			{
				if (Writer.Clear > (frame.DataHead.Length
								   + frame.DataBody.Length))
				{
					Writer.Write(frame.DataHead, 0, (int)frame.LengHead);
					Writer.Write(frame.DataBody, (int)frame.PartBody,
													(int)frame.LengBody);
					return true;
				}
			}
			return false;
		}
		private void Ping()
		{

		}
		private void Pong()
		{
			
		}

		protected override void Work()
		{
			OnEventWork();
		}
		
		protected override void Data()
		{
			if(!Reader.Empty)
				return;
		}
		protected override void Close(Close close)
		{
            OnEventClose(close);
		}
		protected override void Error(WSException error)
		{
			OnEventError(error);
		}
		
		protected override void Connection(IHeader request, IHeader response)
		{
			OnEventConnect(request, response);
		}		
    }
	
}