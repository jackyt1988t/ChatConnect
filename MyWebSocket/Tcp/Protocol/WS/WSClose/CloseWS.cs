using System;
using System.Text;
using System.Collections.Generic;

namespace MyWebSocket.Tcp.Protocol.WS
{
	public class CloseWS
	{
		public bool Req
		{
			get;
			private set;
		}
		public bool Res
		{
			get;
			private set;
		}

		/// <summary>
		/// Инициатор закрытия
		/// </summary>
		public string Host
		{
			get;
			private set;
		}
        public string Initiator
        {
            get;
            private set;
        }
		public string ServerHost
		{
			get;
			private set;
		}
		public string ClientHost
		{
			get;
			private set;
		}

		/// <summary>
		/// Информация о закрытии
		/// </summary>
		public string ServerData
		{
			get;
			private set;
		}
		/// <summary>
		/// Информация о закрытии
		/// </summary>
		public string ClientData
		{
			get;
			private set;
		}

		/// <summary>
		/// Код закрытия WebSocket
		/// </summary>
		public WSClose _InitCode
		{
			get;
			private set;
		}
		/// <summary>
		/// Код закрытия WebSocket
		/// </summary>
		public WSClose ServerCode
		{
			get;
			private set;
		}
		/// <summary>
		/// Код закрытия WebSocket
		/// </summary>
		public WSClose ClientCode
		{
			get;
			private set;
		}
		/// <summary>
		/// Вермя закрытия соединения
		/// </summary>	 
		public DateTime CloseTime
		{
			get;
			private set;
		}
		/// <summary>
		/// Вермя прошедшее после закрытия
		/// </summary>
		public TimeSpan AwaitTime
		{
			get
			{
				return DateTime.Now - CloseTime;
			}
		}
		/// <summary>
		/// Содержит описание завершения подключения
		/// </summary>
		public static Dictionary<WSClose, string> Message;

		public CloseWS()
		{
            CloseTime = DateTime.Now;
		}
		static CloseWS()
		{
			Message = new Dictionary<WSClose, string>();

			Message.Add(WSClose.Normal, "Соединение было закрыто чисто");
			Message.Add(WSClose.GoingAway, "Был выполнен переход");
			Message.Add(WSClose.ProtocolError, "Произошла ошибка протокола");
			Message.Add(WSClose.UnsupportedData, "Данные не поддерживаются текущей версией");
			Message.Add(WSClose.Reserved, "Первышено время ожидания");
			Message.Add(WSClose.NoStatusRcvd, "Код статуса не получен");
			Message.Add(WSClose.Abnormal, "Соединение было закрыто неправильно");
			Message.Add(WSClose.InvalidFrame, "Неверный формат данных");
			Message.Add(WSClose.PolicyViolation, "нарушена политика безопасности");
			Message.Add(WSClose.BigMessage, "Слишком большое сообщение");
			Message.Add(WSClose.Mandatory, "Не возвращен список поддерживаемых расширений");
			Message.Add(WSClose.ServerError, "Произошла ошибка сервера");
			Message.Add(WSClose.TLSHandshake, "не удалось совершить рукопожатие");
		}
        internal void Parse(byte[] data)
        {
            string message = string.Empty;
            if (data.Length > 2)
                message = 
                    Encoding.UTF8.GetString(
                            data, 2, data.Length - 2);
            
            if (data.Length < 2)
                Client(  WSClose.Abnormal, message  );
            else
            {
                int number = data[1] 
                    | (data[0] << 8);

                if (number >= 1000 && number <= 1012)
                    Client( (WSClose)number, message );
                else
                    Client(  WSClose.Abnormal, message  );
            }
        }
        internal void Server(WSClose code, string data)
		{
            if (Res)
				return;
			Res = true;
			ServerCode = code;
			ServerData = data;
			if (!Req)
			{
                _InitCode = code;
                Initiator = "Server";
				CloseTime = DateTime.Now;
			}
		}
        internal void Client(WSClose code, string data)
		{
			if (Req)
				return;
			Req = true;
			ClientCode = code;
			ClientData = data;
			if (!Res)
			{
				_InitCode = code;
                Initiator = "Client";
				CloseTime = DateTime.Now;
			}
		}
		/// <summary>
		/// Возвращает информацию о том каким образом было закрыто соединение
		/// </summary>
		/// <returns>строка с информацие о закрытом подключении</returns>
		public override string ToString()
		{
			return "Инициатор "  +  Host + ". " + _InitCode.ToString() + ": " + Message[_InitCode];
		}
	}
}
