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
		/// Объект для проверки данных заголвока
		/// </summary>
		
		protected WSBinnary __Binnary;
		
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
		public override bool Ping(byte[] message)
		{
			int Opcod = WSFrameSample.PING;
			WSFrameSample frame = new WSFrameSample();
						  frame.BitFind = 0;
						  frame.BitPcod = Opcod;
						  frame.BitLeng = message.Length;
						  frame.DataBody = message;

			return Send(frame.GetDataFrame());
		}
		public override bool Pong(byte[] message)
		{
			int Opcod = WSFrameSample.PONG;
			WSFrameSample frame = new WSFrameSample();
						  frame.BitFind = 0;
						  frame.BitPcod = Opcod;
						  frame.BitLeng = message.Length;
						  frame.DataBody = message;

			return Send(frame.GetDataFrame());
		}
		public override bool Close(string message)
		{
			int Opcod = WSFrameSample.CLOSE;
			WSFrameSample frame = new WSFrameSample();
						  frame.BitFind = 0;
						  frame.BitPcod = Opcod;

			return Send(frame.GetDataFrame());
		}
		public override bool Message(byte[] message, WSOpcod opcod, WSFin fin)
		{
			int Fin = 0;
			int Opcod = 0;
			if (fin == WSFin.Last)
				Fin = 0;
			else if (fin == WSFin.Next)
				Fin = 1;
			if (opcod == WSOpcod.Text)
				Opcod = WSFrameSample.TEXT;
			else if (opcod == WSOpcod.Binnary)
				Opcod = WSFrameSample.BINNARY;
			WSFrameSample frame = new WSFrameSample();
						  frame.BitFind  = Fin;
						  frame.BitPcod  = Opcod;
						  frame.BitLeng  = message.Length;
						  frame.DataBody = message;
			
			return Send(frame.GetDataFrame());
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
		protected override void Send()
		{
						
		}
		protected override void Data()
		{
			if(!Reader.isRead)
				return;
		}
		protected override void Close(WSClose close)
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