using System; 
using System.Net.Sockets;
using System.Collections.Generic;

namespace MyWebSocket.Tcp.Protocol
{
	static class SocketErrors
	{
		static Dictionary<SocketError, string> __SocketErrors;

		static SocketErrors()
		{
			__SocketErrors = new Dictionary<SocketError, string>();
			__SocketErrors.Add(SocketError.AccessDenied, "Произведена попытка доступа к объекту Socket способом, запрещенным его разрешениями доступа");
			__SocketErrors.Add(SocketError.AddressAlreadyInUse, "Обычно разрешается использовать только адрес");
			__SocketErrors.Add(SocketError.AddressFamilyNotSupported, "Указанное семейство адресов не поддерживается");
			__SocketErrors.Add(SocketError.AddressNotAvailable, "Указанный IP - адрес в данном контексте является недопустимым");
			__SocketErrors.Add(SocketError.AlreadyInProgress, "Операция незаблокированного объекта Socket уже выполняется");
			__SocketErrors.Add(SocketError.ConnectionAborted, "Подключение разорвано платформой .NET Framework или поставщиком основного сокета");
			__SocketErrors.Add(SocketError.ConnectionRefused, "Удаленный узел активно отказывает в подключении");
			__SocketErrors.Add(SocketError.ConnectionReset, "Подключение сброшено удаленным компьютером");
			__SocketErrors.Add(SocketError.DestinationAddressRequired, "Требуемый адрес был пропущен в операции на объекте Socket");
			__SocketErrors.Add(SocketError.Disconnecting, "Выполняется правильная последовательность отключения");
			__SocketErrors.Add(SocketError.Fault, "Поставщиком основного сокета обнаружен недопустимый указатель адреса");
			__SocketErrors.Add(SocketError.HostDown, "Ошибка при выполнении операции, вызванная отключением удаленного узла");
			__SocketErrors.Add(SocketError.HostNotFound, "Такой узел не существует.Данное имя не является ни официальным именем узла, ни псевдонимом");
			__SocketErrors.Add(SocketError.HostUnreachable, "Отсутствует сетевой маршрут к указанному узлу");
			__SocketErrors.Add(SocketError.InProgress, "Выполняется блокирующая операция");
			__SocketErrors.Add(SocketError.Interrupted, "Блокирующее обращение к объекту Socket отменено");
			__SocketErrors.Add(SocketError.InvalidArgument, "Предоставлен недопустимый аргумент для члена объекта Socket");
			__SocketErrors.Add(SocketError.IOPending, "Приложение инициировало перекрывающуюся операцию, которая не может быть закончена немедленно");
			__SocketErrors.Add(SocketError.IsConnected, "Объект Socket уже подключен");
			__SocketErrors.Add(SocketError.MessageSize, "У датаграммы слишком большая длина");
			__SocketErrors.Add(SocketError.NetworkDown, "Сеть недоступна");
			__SocketErrors.Add(SocketError.NetworkReset, "Приложение пытается задать значение KeepAlive для подключения, которое уже отключено");
			__SocketErrors.Add(SocketError.NetworkUnreachable, "Не существует маршрута к удаленному узлу");
			__SocketErrors.Add(SocketError.NoBufferSpaceAvailable, "Отсутствует свободное буферное пространство для операции объекта Socket");
			__SocketErrors.Add(SocketError.NoData, "Требуемое имя или IP - адрес не найдены на сервере имен");
			__SocketErrors.Add(SocketError.NoRecovery, "Неустранимая ошибка, или не удается найти запрошенную базу данных");
			__SocketErrors.Add(SocketError.NotConnected, "Приложение пытается отправить или получить данные, а объект Socket не подключен");
			__SocketErrors.Add(SocketError.NotInitialized, "Основной поставщик сокета не инициализирован");
			__SocketErrors.Add(SocketError.NotSocket, "Предпринята попытка выполнить операцию объекта Socket не на сокете");
			__SocketErrors.Add(SocketError.OperationAborted, "Перекрывающаяся операция была прервана из-за закрытия объекта Socket");
			__SocketErrors.Add(SocketError.OperationNotSupported, "Семейство адресов не поддерживается семейством протоколов");
			__SocketErrors.Add(SocketError.ProcessLimit, "Слишком много процессов используется основным поставщиком сокета");
			__SocketErrors.Add(SocketError.ProtocolFamilyNotSupported, "Семейство протоколов не реализовано или не настроено");
			__SocketErrors.Add(SocketError.ProtocolNotSupported, "Протокол не реализован или не настроен");
			__SocketErrors.Add(SocketError.ProtocolOption, "Для объекта Socket был использован неизвестный, недопустимый или неподдерживаемый параметр или уровень");
			__SocketErrors.Add(SocketError.ProtocolType, "Неверный тип протокола для данного объекта Socket");
			__SocketErrors.Add(SocketError.Shutdown, "Запрос на отправку или получение данных отклонен, так как объект Socket уже закрыт");
			__SocketErrors.Add(SocketError.SocketError, "Произошла неопознанная ошибка объекта Socket");
			__SocketErrors.Add(SocketError.SocketNotSupported, "Указанный тип сокета не поддерживается в данном семействе адресов");
			__SocketErrors.Add(SocketError.Success, "Операция объекта Socket выполнена успешно");
			__SocketErrors.Add(SocketError.SystemNotReady, "Подсистема сети недоступна");
			__SocketErrors.Add(SocketError.TimedOut, "Окончилось время ожидания попытки подключения, или произошел сбой при отклике подключенного узла");
			__SocketErrors.Add(SocketError.TooManyOpenSockets, "Слишком много открытых сокетов в основном поставщике сокета");
			__SocketErrors.Add(SocketError.TryAgain, "Не удалось разрешить имя хоста.Повторите операцию позднее");
			__SocketErrors.Add(SocketError.TypeNotFound, "Указанный класс не найден");
			__SocketErrors.Add(SocketError.VersionNotSupported, "Версия основного поставщика сокета выходит за пределы допустимого диапазона");
			__SocketErrors.Add(SocketError.WouldBlock, "Операция на незаблокированном сокете не может быть закончена немедленно");
		}
		/// <summary>
		/// Информация об ошибке
		/// </summary>
		/// <param name="error">ошибка</param>
		/// <returns>информация об ошибке</returns>
		static public string SocketErrorInfo(SocketError error)
		{
			return __SocketErrors[error];
		}
	}
}
