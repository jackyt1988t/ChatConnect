using System;
using System.Collections.Generic;

namespace MyWebSocket.Tcp.Protocol.WS
{
	class CloseWS
	{
		bool req;
		public bool Req
		{
			get
			{
				return req;
			}
			set
			{
				req = value;
				if (!res)
					CloseTime = DateTime.Now;
			}
		}
		bool res;
		public bool Res
		{
			get
			{
				return res;
			}
			set
			{
				res = value;
				if (!req)
					CloseTime = DateTime.Now;
			}
		}
		/// <summary>
		/// Инициатор закрытия
		/// </summary>
		public string Other_Host
		{
			get;
			private set;
		}
		string serverhost;
		public string ServerHost
		{
			get
			{
				return serverhost;
			}
			set
			{
				if (string.IsNullOrEmpty(Other_Host))
					Other_Host = serverhost = value;
			}
		}
		string clienthost;
		public string ClientHost
		{
			get
			{
				return clienthost;
			}
			set
			{
				if (string.IsNullOrEmpty(Other_Host))
					Other_Host = clienthost = value;
			}
		}
		/// <summary>
		/// Информация о закрытии
		/// </summary>
		public string ServerData
		{
			get;
			set;
		}
		/// <summary>
		/// Информация о закрытии
		/// </summary>
		public string ClientData
		{
			get;
			set;
		}
		/// <summary>
		/// Код закрытия WebSocket
		/// </summary>
		public WSClose Other_Code
		{
			get
			{
				if (Other_Host == "Server")
					return ServerCode;
				else
					return ClientCode;
			}
		}
		/// <summary>
		/// Код закрытия WebSocket
		/// </summary>
		public WSClose ServerCode
		{
			get;
			set;
		}
		/// <summary>
		/// Код закрытия WebSocket
		/// </summary>
		public WSClose ClientCode
		{
			get;
			set;
		}
		/// <summary>
		/// Вермя закрытия соединения
		/// </summary>	 
		public DateTime CloseTime
		{
			get;
			set;
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
			
		}
		public CloseWS(string host, WSClose code)
		{
			ServerHost = host;
			ServerData = 
			     Message[code];
			ServerCode = code;
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
		/// <summary>
		/// Возвращает информацию о том каким образом было закрыто соединение
		/// </summary>
		/// <returns>строка с информацие о закрытом подключении</returns>
		public override string ToString()
		{
			return "Инициатор "  +  Other_Host + ". " + Other_Code.ToString() + ": " + Message[Other_Code];
		}
	}
}
