using System;
using System.Net.Sockets;
using System.Collections.Generic;

namespace MyWebSocket.Tcp.Protocol.WS
{
	public static class WSErrors
	{
		static Dictionary<WsError, string> __WSErrors;

		static WSErrors()
		{

			__WSErrors = new Dictionary<WsError, string>();
			__WSErrors.Add(WsError.PongBodyIncorrect, "Неверное тело ответа PONG");
			__WSErrors.Add(WsError.PingNotResponse, "Удаленная сторона не ответила на запрос PING");
			__WSErrors.Add(WsError.BodyWaitLimit, "Превышено время ожидания данных от удаленной стороны");
			__WSErrors.Add(WsError.PcodNotSuported, "Полученный опкод не поддерживается текущей реализацией");
			__WSErrors.Add(WsError.PcodNotRepeat, "Опкод полученный в фрейме не совпадает с предыдущем...");
			__WSErrors.Add(WsError.HeaderFrameError, "Ошибка при получении заголовков фрейма WS");
			__WSErrors.Add(WsError.BodyFrameError, "Ошибка при получении тела фрейма WS");
			__WSErrors.Add(WsError.HandshakeError, "Ошибка заголвоков протокола WS");
			__WSErrors.Add(WsError.BufferLimitLength, "Превышена допустимая длинна буфера данных");
			__WSErrors.Add(WsError.CriticalError, "Произошла критическая ошибка программы");
		}
		/// <summary>
		/// Информация об ошибке
		/// </summary>
		/// <param name="error">ошибка</param>
		/// <returns>информация об ошибке</returns>
		public static string WSErrorInfo(WsError error)
		{
			return __WSErrors[error];
		}
	}
}
