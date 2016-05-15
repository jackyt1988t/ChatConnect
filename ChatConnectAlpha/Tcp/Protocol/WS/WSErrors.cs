using System;
using System.Net.Sockets;
using System.Collections.Generic;

namespace ChatConnect.Tcp.Protocol.WS
{
	static class WSErrorMsg
	{
		static Dictionary<WsError, string> __WSErrorMsg;
		static Dictionary<SocketError, string> __SocketErrorMsg;

		static WSErrorMsg()
		{

			__WSErrorMsg = new Dictionary<WsError, string>();
			__WSErrorMsg.Add(WsError.PongBodyIncorrect, "Неверное тело ответа PONG");
			__WSErrorMsg.Add(WsError.PingNotResponse, "Удаленная сторона не ответила на запрос PING");
			__WSErrorMsg.Add(WsError.BodyWaitLimit, "Превышено время ожидания данных от удаленной стороны");
			__WSErrorMsg.Add(WsError.PcodNotSuported, "Полученный опкод не поддерживается текущей реализацией");
			__WSErrorMsg.Add(WsError.PcodNotRepeat, "Опкод полученный в фрейме не совпадает с предыдущем...");
			__WSErrorMsg.Add(WsError.HeaderFrameError, "Ошибка при получении заголовков фрейма WS");
			__WSErrorMsg.Add(WsError.BodyFrameError, "Ошибка при получении тела фрейма WS");
			__WSErrorMsg.Add(WsError.HandshakeError, "Ошибка заголвоков протокола WS");
			__WSErrorMsg.Add(WsError.BufferLimitLength, "Превышена допустимая длинна буфера данных");

			__SocketErrorMsg = new Dictionary<SocketError, string>();
			__SocketErrorMsg.Add(SocketError.AccessDenied, "Произведена попытка доступа к объекту Socket способом, запрещенным его разрешениями доступа");
			__SocketErrorMsg.Add(SocketError.AddressAlreadyInUse, "Обычно разрешается использовать только адрес");
			__SocketErrorMsg.Add(SocketError.AddressFamilyNotSupported, "Указанное семейство адресов не поддерживается");
			__SocketErrorMsg.Add(SocketError.AddressNotAvailable, "Указанный IP - адрес в данном контексте является недопустимым");
			__SocketErrorMsg.Add(SocketError.AlreadyInProgress, "Операция незаблокированного объекта Socket уже выполняется");
			__SocketErrorMsg.Add(SocketError.ConnectionAborted, "Подключение разорвано платформой .NET Framework или поставщиком основного сокета");
			__SocketErrorMsg.Add(SocketError.ConnectionRefused, "Удаленный узел активно отказывает в подключении");
			__SocketErrorMsg.Add(SocketError.ConnectionReset, "Подключение сброшено удаленным компьютером");
			__SocketErrorMsg.Add(SocketError.DestinationAddressRequired, "Требуемый адрес был пропущен в операции на объекте Socket");
			__SocketErrorMsg.Add(SocketError.Disconnecting, "Выполняется правильная последовательность отключения");
			__SocketErrorMsg.Add(SocketError.Fault, "Поставщиком основного сокета обнаружен недопустимый указатель адреса");
			__SocketErrorMsg.Add(SocketError.HostDown, "Ошибка при выполнении операции, вызванная отключением удаленного узла");
			__SocketErrorMsg.Add(SocketError.HostNotFound, "Такой узел не существует.Данное имя не является ни официальным именем узла, ни псевдонимом");
			__SocketErrorMsg.Add(SocketError.HostUnreachable, "Отсутствует сетевой маршрут к указанному узлу");
			__SocketErrorMsg.Add(SocketError.InProgress, "Выполняется блокирующая операция");
			__SocketErrorMsg.Add(SocketError.Interrupted, "Блокирующее обращение к объекту Socket отменено");
			__SocketErrorMsg.Add(SocketError.InvalidArgument, "Предоставлен недопустимый аргумент для члена объекта Socket");
			__SocketErrorMsg.Add(SocketError.IOPending, "Приложение инициировало перекрывающуюся операцию, которая не может быть закончена немедленно");
			__SocketErrorMsg.Add(SocketError.IsConnected, "Объект Socket уже подключен");
			__SocketErrorMsg.Add(SocketError.MessageSize, "У датаграммы слишком большая длина");
			__SocketErrorMsg.Add(SocketError.NetworkDown, "Сеть недоступна");
			__SocketErrorMsg.Add(SocketError.NetworkReset, "Приложение пытается задать значение KeepAlive для подключения, которое уже отключено");
			__SocketErrorMsg.Add(SocketError.NetworkUnreachable, "Не существует маршрута к удаленному узлу");
			__SocketErrorMsg.Add(SocketError.NoBufferSpaceAvailable, "Отсутствует свободное буферное пространство для операции объекта Socket");
			__SocketErrorMsg.Add(SocketError.NoData, "Требуемое имя или IP - адрес не найдены на сервере имен");
			__SocketErrorMsg.Add(SocketError.NoRecovery, "Неустранимая ошибка, или не удается найти запрошенную базу данных");
			__SocketErrorMsg.Add(SocketError.NotConnected, "Приложение пытается отправить или получить данные, а объект Socket не подключен");
			__SocketErrorMsg.Add(SocketError.NotInitialized, "Основной поставщик сокета не инициализирован");
			__SocketErrorMsg.Add(SocketError.NotSocket, "Предпринята попытка выполнить операцию объекта Socket не на сокете");
			__SocketErrorMsg.Add(SocketError.OperationAborted, "Перекрывающаяся операция была прервана из-за закрытия объекта Socket");
			__SocketErrorMsg.Add(SocketError.OperationNotSupported, "Семейство адресов не поддерживается семейством протоколов");
			__SocketErrorMsg.Add(SocketError.ProcessLimit, "Слишком много процессов используется основным поставщиком сокета");
			__SocketErrorMsg.Add(SocketError.ProtocolFamilyNotSupported, "Семейство протоколов не реализовано или не настроено");
			__SocketErrorMsg.Add(SocketError.ProtocolNotSupported, "Протокол не реализован или не настроен");
			__SocketErrorMsg.Add(SocketError.ProtocolOption, "Для объекта Socket был использован неизвестный, недопустимый или неподдерживаемый параметр или уровень");
			__SocketErrorMsg.Add(SocketError.ProtocolType, "Неверный тип протокола для данного объекта Socket");
			__SocketErrorMsg.Add(SocketError.Shutdown, "Запрос на отправку или получение данных отклонен, так как объект Socket уже закрыт");
			__SocketErrorMsg.Add(SocketError.SocketError, "Произошла неопознанная ошибка объекта Socket");
			__SocketErrorMsg.Add(SocketError.SocketNotSupported, "Указанный тип сокета не поддерживается в данном семействе адресов");
			__SocketErrorMsg.Add(SocketError.Success, "Операция объекта Socket выполнена успешно");
			__SocketErrorMsg.Add(SocketError.SystemNotReady, "Подсистема сети недоступна");
			__SocketErrorMsg.Add(SocketError.TimedOut, "Окончилось время ожидания попытки подключения, или произошел сбой при отклике подключенного узла");
			__SocketErrorMsg.Add(SocketError.TooManyOpenSockets, "Слишком много открытых сокетов в основном поставщике сокета");
			__SocketErrorMsg.Add(SocketError.TryAgain, "Не удалось разрешить имя хоста.Повторите операцию позднее");
			__SocketErrorMsg.Add(SocketError.TypeNotFound, "Указанный класс не найден");
			__SocketErrorMsg.Add(SocketError.VersionNotSupported, "Версия основного поставщика сокета выходит за пределы допустимого диапазона");
			__SocketErrorMsg.Add(SocketError.WouldBlock, "Операция на незаблокированном сокете не может быть закончена немедленно");
		}
		public static string Error(WsError error)
		{
			return __WSErrorMsg[error];
		}
		public static string Error(SocketError error)
		{
			return __SocketErrorMsg[error];
		}
	}
}
