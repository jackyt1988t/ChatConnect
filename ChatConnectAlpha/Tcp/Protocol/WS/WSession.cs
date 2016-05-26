using System;
using System.Net;

namespace ChatConnect.Tcp.Protocol.WS
{
	class WSession
	{
		/// <summary>
		/// Продолжительность текущей сессии
		/// </summary>
		public TimeSpan Time
		{
			get
			{
				return DateTime.Now - Start;
			}
		}
		/// <summary>
		/// Начало текущей сессии
		/// </summary>
		public DateTime Start
		{
			get;
		}
		/// <summary>
		/// Адресс подключения
		/// </summary>
		public IPAddress Address
		{
			get;
		}

		public WSession(IPAddress address)
		{
			Start =
				DateTime.Now;
			Address = address;
		}
	}
}
