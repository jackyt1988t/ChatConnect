namespace MyWebSocket.Tcp.Protocol.HTTP
{
	/// <summary>
	/// Коды состояния http
	/// </summary>
	static class HTTPCode
	{
#region 1xx Инфо
		/// <summary>
		/// 100 продолжай
		/// </summary>
		public static codexxx _100_ = new code100();
		/// <summary>
		/// 101 переключение протоколов
		/// </summary>
		public static codexxx _101_ = new code101();
		/// <summary>
		/// 102 идёт обработка протокола
		/// </summary>
		public static codexxx _102_ = new code102();

#endregion

#region 2xx Успех

		/// <summary>
		/// 200 ОК хорошо
		/// </summary>
		public static codexxx _200_ = new code200();
		/// <summary>
		/// 201 ОК создано
		/// </summary>
		public static codexxx _201_ = new code201();
		/// <summary>
		/// 204	нет содержимого
		/// </summary>
		public static codexxx _204_ = new code204();
		/// <summary>
		/// 206	частичное содержимое
		/// </summary>
		public static codexxx _206_ = new code206();

#endregion

#region 3xx Перенаправление

		/// <summary>
		/// 300	перенаправление
		/// </summary>
		public static codexxx _300_ = new code300();

#endregion

#region 4xx Ошибка клиента.

		/// <summary>
		/// 400 плохой, неверный запрос
		/// </summary>
		public static codexxx _400_ = new code400();
		/// <summary>
		/// 403	запрещено, неверный запрос
		/// </summary>
		public static codexxx _403_ = new code403();
		/// <summary>
		/// 404	не найдено, неверный запрос
		/// </summary>
		public static codexxx _404_ = new code404();

#endregion

#region 5xx Ошибка сервера.

		/// <summary>
		/// 500	ошибка сервера
		/// </summary>
		public static codexxx _500_ = new code500();
		/// <summary>
		/// 501 не реализовано
		/// </summary>
		public static codexxx _501_ = new code501();
		/// <summary>
		/// 503	сервис недоступен
		/// </summary>
		public static codexxx _503_ = new code503();

#endregion
	}
}