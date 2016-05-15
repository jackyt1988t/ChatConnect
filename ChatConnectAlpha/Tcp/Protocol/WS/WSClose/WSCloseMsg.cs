using System.Collections.Generic;

namespace ChatConnect.Tcp.Protocol.WS
{
	static class WSCloseMsg 
	{
		static Dictionary<WSCloseNum, string> CloseMsg;

		static WSCloseMsg()
		{
			CloseMsg = new Dictionary<WSCloseNum, string>();

			CloseMsg.Add(WSCloseNum.Normal, "Соединение было закрыто чисто");
			CloseMsg.Add(WSCloseNum.GoingAway, "Был выполнен переход");
			CloseMsg.Add(WSCloseNum.ProtocolError, "Произошла ошибка протокола");
			CloseMsg.Add(WSCloseNum.UnsupportedData, "Данные не поддерживаются текущей версией");
			CloseMsg.Add(WSCloseNum.Reserved, "");
			CloseMsg.Add(WSCloseNum.NoStatusRcvd, "Код статуса не получен");
			CloseMsg.Add(WSCloseNum.Abnormal, "Соединение было закрыто неправильно");
			CloseMsg.Add(WSCloseNum.InvalidFrame, "Неверный формат данных");
			CloseMsg.Add(WSCloseNum.PolicyViolation, "нарушена политика безопасности");
			CloseMsg.Add(WSCloseNum.BigMessage, "Слишком большое сообщение");
			CloseMsg.Add(WSCloseNum.Mandatory, "Не возвращен список поддерживаемых расширений");
			CloseMsg.Add(WSCloseNum.ServerError, "Произошла ошибка сервера");
			CloseMsg.Add(WSCloseNum.TLSHandshake, "не удалось совершить рукопожатие");
		}
		public static string Message(WSCloseNum close)
		{
			return CloseMsg[close];
		}
	}
}
