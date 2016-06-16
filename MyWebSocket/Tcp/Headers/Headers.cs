using System;
using System.Text;
using System.Collections.Generic;

namespace MyWebSocket.Tcp
{
	/// <summary>
	/// Содержит информацию о заголвоках
	/// </summary>
    class Header : IHeader
    {
#region Распространенные Заголовки http

		int contentlength;
		public int ContentLength
		{
			get
			{
				return contentlength;
			}
			set
			{
				AddHeader("Content-Length", value.ToString());
			}
		}
		string upgrade;
		public string Upgrade
		{
			get
			{
				return upgrade;
			}
			set
			{
				AddHeader("Upgrade", value.ToString());
			}
		}
		string connection;
		public string Connection
		{
			get
			{
				return connection;
			}
			set
			{
				AddHeader("Upgrade", value.ToString());
			}
		}
		string contentencoding;
		public string ContentEncoding
		{
			get
			{
				return contentencoding;
			}
			set
			{
				AddHeader("Content-Encoding", value.ToString());
			}
		}
		string transferencoding;
		public string TransferEncoding
		{
			get
			{
				return transferencoding;
			}
			set
			{
				AddHeader("Transfer-Encoding", value.ToString());
			}
		}

		List<string> acceptencoding;
		public List<string> AcceptEncoding
		{
			get
			{
				return acceptencoding;
			}
			set
			{
				AddHeader("Accept-Encoding", value.ToString());
			}
		}

		#endregion

		/// <summary>
		/// Показывает отправлены, получены данные
		/// </summary>
		public bool IsEnd
		{
			get;
			private set;
		}
		/// <summary>
		/// Указывает были ли получены все заголовки
		/// </summary>
		public bool IsReq
		{
			get;
			private set;
		}
		/// <summary>
		/// Указывает были ли отправлены все заголвоки
		/// </summary>
		public bool IsRes
		{
			get;
			private set;
		}
		/// <summary>
		/// Показывает Необходимость закрытия соединение
		/// </summary>
		public bool Close
		{
			get;
			set;
		}
		/// <summary>
		/// тело сообщения
		/// </summary>
		public byte[] _Body
        {
            get;
            set;
        }
		/// <summary>
		/// Объект синхронизации
		/// </summary>
		public object Sync
		{
			get;
		}
		public string File
		{
			get;
			set;
		}
		/// <summary>
		/// путь полученного http запроса
		/// </summary>
		public string Path
        {
            get;
            set;
        }
		/// <summary>
		/// версия текущего http протокола
		/// </summary>
        public string Http
        {
            get;
            set;
        }
        public string Method
        {
            get;
            set;
        }
        public string StartString
        {
            get;
            set;
        }
		public DateTime TimeConnection
		{
			get;
			private set;
		}
		public Queue<byte[]> SegmentsBuffer
		{
			get;
			private set;
		}
		private Dictionary<string, string> ContainerHeaders;
		public Header()
        {
			Sync = new object();
			TimeConnection = DateTime.Now;
			SegmentsBuffer = new Queue<byte[]>(50);
			ContainerHeaders = new Dictionary<string, string>();

		}
		public void Clear()
		{
			ContainerHeaders.Clear();
		}
		public bool SetReq()
		{
			if (!IsReq)
			{
				IsReq = true;
				return false;
			}
			return true;
		}
		public bool SetRes()
		{
			if (!IsRes)
			{
				IsRes = true;
				return false;
			}
			return true;
		}
		public bool SetEnd()
		{
			if (!IsEnd)
			{
				IsEnd = true;
				return false;
			}
			return true;
		}
		/// <summary>
		/// Добавляет значение и параметр заголовка
		/// если заголовок уже добавлен заменяет его
		/// </summary>
		/// <param name="key">параметр заголовка</param>
		/// <param name="value">значение заголовка</param>
		public void AddHeader(string key, string value)
		{
			key = key.Trim(new char[] { ' ' });
			value = value.TrimStart(new char[] { ' ' });
			
			Analizating(key, value);
			ReplaceHeader(key, value);
		}
		/// <summary>
		/// Удаляет заголовок если он был добавлен
		/// </summary>
		/// <param name="key">параметр заголвока</param>
		/// <param name="@case">регстронезависимый поиск параметра</param>
		/// <returns>true если заголвок был найден иначе false</returns>
		public bool RemoveHeader(string key, bool @case = true)
		{
			foreach (KeyValuePair<string, string> header in ContainerHeaders)
			{
				string Key;
				if (!@case)
					Key = header.Key;
				else
					Key = header.Key.ToLower();
				if (Key == key)
				{
					Analizating(Key, string.Empty);
					ContainerHeaders.Remove(header.Key);
					return true;
				}
			}
			return false;
		}
		/// <summary>
		/// Проверяет добвлен указанный заголвок или нет
		/// </summary>
		/// <param name="key">параметр заголвока</param>
		/// <param name="@case">регстронезависимый поиск параметра</param>
		/// <returns>true если заголвок был найден иначе false</returns>
		public bool ContainsKeys(string key, bool @case = true)
		{
			foreach (KeyValuePair<string, string> header in ContainerHeaders)
			{
				string Key;
				if (!@case)
					Key = header.Key;
				else
					Key = header.Key.ToLower();
				if (Key == key)
					return true;
			}
			return false;
		}
		/// <summary>
		/// Проверяет добвлен указанный заголвок или нет,
		/// если добавлен записывает значение парметра в переданную переменную
		/// </summary>
		/// <param name="key">значение парметра</param>
		/// <param name="value">будет записано значение заголвока</param>
		/// <param name="@case">регстронезависимый поиск параметра<</param>
		/// <returns>true если заголвок был найден инче false</returns>
		public bool ContainsKeys(string key, out string value, bool @case = true)
		{
			
			foreach (KeyValuePair<string, string> header in ContainerHeaders)
			{
				if (header.Key.ToLower() == key)
				{
					value = header.Value;
					return true;
				}
			}
			value = string.Empty;
			return false;
		}

			/// <summary>
			/// Возвращает составленный в виде ответа заголовки
			/// </summary>
			/// <returns>массив байт</returns>
			public virtual byte[] ToByte()
			{	
				return Encoding.UTF8.GetBytes(ToString());
			}
			/// <summary>
			/// Возвращает составленный в виде ответа заголовки
			/// </summary>
			/// <returns>строка заголовков</returns>
			public override string ToString()
			{				
				string request = StartString + "\r\n";
				foreach (KeyValuePair<string, string> keyvalue in ContainerHeaders)
				{
					request += keyvalue.Key + ": " + keyvalue.Value + "\r\n";
				}
				request += "\r\n";
				return request;
			}
		
		private void Analizating(string key, string value)
		{
			switch (key.ToLower())
			{
				case "upgrade":
					upgrade = value.ToLower();
					break;
				case "connection":
					connection = value.ToLower();
					break;
				case "content-length":
					contentlength = int.Parse(value);
					break;
				case "accept-encoding":
					acceptencoding = 
							ReplaseValues(value, ",");
					break;
				case "content-encoding":
					contentencoding = value.ToLower();
					break;
				case "transfer-encoding":
					transferencoding = value.ToLower();
					break;
			}
		}
		private void ReplaceHeader(string key, string value)
		{
			if (IsReq)
				throw new HeadersException("заголовки были получены");
			if (IsRes)
				throw new HeadersException("заголовки были отправлены");
			foreach (KeyValuePair<string, string> header in ContainerHeaders)
			{
				if (header.Key.ToLower() == key.ToLower())
				{
					ContainerHeaders[header.Key] = value;
					return;
				}
			}
			ContainerHeaders.Add(key, value);
		}
		private List<string> ReplaseValues(string value, string separat)
		{
			List<string> values = 
						new List<string>(value.Split(separat.ToCharArray()));
			for (int i = 0; i < values.Count; i++)
			{
				values[i] = values[i].TrimStart(new char[] { ' ' });
			}
			return values;
		}
	}
}
