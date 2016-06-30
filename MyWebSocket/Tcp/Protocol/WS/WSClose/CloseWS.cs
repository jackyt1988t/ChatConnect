using System;
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
		public void Server(WSClose code, string data, string host)
		{
			if (Res)
				return;
			Res = true;
			ServerCode = code;
			ServerData = data;
			ServerHost = host;
			if (!Req)
			{
				
				Host = host;
				_InitCode = code;
				CloseTime = DateTime.Now;
			}
		}
		public void Client(WSClose code, string data, string host)
		{
			if (Req)
				return;
			Req = true;
			ClientCode = code;
			ClientData = data;
			ClientHost = host;
			if (!Res)
			{
				
				Host = host;
				_InitCode = code;
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
