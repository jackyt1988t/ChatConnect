using System;
using System.Collections.Generic;

namespace ChatConnect.Tcp.Protocol.WS
{
	class Close
	{
		public string Host
		{
			get;
			private set;
		}
		public string CloseMsg
		{
			get;
			private set;
		}
		public DateTime CloseTime
		{
			get;
			private set;
		}
		public WSClose CloseCode
		{
			get;
			private set;
		}
		public static Dictionary<WSClose, string> Message;
		
		public Close(string host, WSClose code)
		{
			Host	  = host;
			CloseMsg  = Message[code];
			CloseCode = code;
		}
		static Close()
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
		public override string ToString()
		{
			return "Инициатор "  +  Host + ". "  +  CloseCode.ToString()  +  ": " + CloseMsg;
		}
	}
}
