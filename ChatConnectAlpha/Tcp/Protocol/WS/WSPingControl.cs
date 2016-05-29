using System;

namespace ChatConnect.Tcp.Protocol.WS
{
	class WSPingControl
	{
		/// <summary>
		/// пинг отправлен
		/// </summary>
		public bool IsPing;
		/// <summary>
		/// понг был получен
		/// </summary>
		public bool IsPong;
		
		/// <summary>
		/// Веремя ожидания-получения пинг
		/// </summary>
		public TimeSpan GetPong
		{
			get
			{
				return _getpong;
			}
			set
			{
				IsPong   = true;
				IsPing   = false;
				_getpong = value;
			}
		}
		/// <summary>
		/// Время отправки пинг сообщения
		/// </summary>
		public TimeSpan SetPing
		{
			get
			{
				return _setping;
			}
			set
			{
				IsPing   = true;
				IsPong   = false;
				_setping = value;
				_getpong = 
					new TimeSpan(DateTime.Now.Ticks);
			}
		}

		private TimeSpan _setping;
		private TimeSpan _getpong;

		public WSPingControl()
		{
			_setping = new TimeSpan(DateTime.Now.Ticks + TimeSpan.TicksPerSecond * 5);
		}
	}
}
