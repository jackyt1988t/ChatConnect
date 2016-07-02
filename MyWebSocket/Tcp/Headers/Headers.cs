using System;
using System.Text;
using System.Collections.Generic;

namespace MyWebSocket.Tcp
{
    /// <summary>
    /// Содержит информацию о заголвоках
    /// </summary>
    public class Header : IHeader
    {
#region Распространенные Заголовки http

        int contentlength;
        /// <summary>
        /// Длинна заголовков
        /// </summary>
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
        /// <summary>
        /// Заголовок Upgrade
        /// </summary>
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
        /// <summary>
        /// Заголовок Connection
        /// </summary>
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
        /// <summary>
        /// Заголовок Content-Encoding
        /// </summary>
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
        /// <summary>
        /// Заголовок TransferEncoding
        /// </summary>
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

        List<string> contenttype;
        /// <summary>
        /// Заголовок Content-Type
        /// Содержит список значений Content-Type
        /// </summary>
        public List<string> ContentType
        {
            get
            {
                return contenttype;
            }
            set
            {
                contenttype = value;
                AddHeader("Content-Type", SplitValues("; ", value));
            }
        }
        List<string> cashcontrol;
        /// <summary>
        /// Заголовок Cash-Control
        /// Содержит список значений Cash-Control
        /// </summary>
        public List<string> CacheControl
        {
            get
            {
                return cashcontrol;
            }
            set
            {
                cashcontrol = value;
                AddHeader("Cache-Control", SplitValues(", ", value));
            }
        }
        List<string> acceptencoding;
        /// <summary>
        /// Заголовок Accept-Encoding
        /// Содержит список значений Accept-Encoding
        /// </summary>
        public List<string> AcceptEncoding
        {
            get
            {
                return acceptencoding;
            }
            set
            {
                acceptencoding = value;
                AddHeader("Accept-Encoding", SplitValues(", ", value));
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
            private set;
        }
        /// <summary>
        /// тело сообщения
        /// </summary>
        public byte[] Body
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
        /// <summary>
        /// Расширение файла(html)
        /// </summary>
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
        public string StrStr
        {
            get;
            set;
        }
		/// <summary>
		/// Возвращает значение заголвока
		/// </summary>
		/// <param name="param">параметр заголвока</param>
		/// <returns>возвращает null если заголвок не найден</returns>
		public string this[string param]
		{
			set
			{
				AddHeader(param, value);
			}
			get
			{
				param = param.ToLower();
				foreach (KeyValuePair<string, string> header in ContainerHeaders)
				{
					if (header.Key.ToLower() == param)
					{
						return header.Value;
					}
				}
				return null;
			}
		}
        /// <summary>
        /// время создания объекта заголвоков
        /// </summary>
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
        /// <summary>
        /// Класс Header содержит информацию о заголвоках
        /// </summary>
        public Header()
        {
            Sync = new object();
            TimeConnection = DateTime.Now;
            SegmentsBuffer = new Queue<byte[]>(50);
            ContainerHeaders = new Dictionary<string, string>();

        }

#region function
        
        /// <summary>
        /// Добавляет значение и параметр заголовка
        /// если заголовок уже добавлен заменяет его
        /// </summary>
        /// <param name="key">параметр заголовка</param>
        /// <param name="value">значение заголовка</param>
        public void AddHeader(string key, string value)
        {
			if (key == null)
				key = string.Empty;
			else
				key = key.Trim(new char[] { ' ' });
			if (value == null)
				value = string.Empty;
			else
				value = value.Trim(new char[] { ' ' });
            
            Analizating(key, value);
            ReplaceHeader(key, value);
        }
        /// <summary>
        /// Очищает все заголвоки
        /// </summary>
        public void ClearHeaders()
        {
            StrStr           = null;
            upgrade          = null;
			connection       = null;
			contenttype      = null;
			cashcontrol      = null;
			contentlength    =    0;
			acceptencoding   = null;
			contentencoding  = null;
			transferencoding = null;
			ContainerHeaders.Clear();
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
                    Analizating(Key, null);
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
        /// <param name="case">регстронезависимый поиск параметра</param>
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
                string request = StrStr + "\r\n";
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
                case "content-type":
					contenttype = 
						ReplaseValues(value, ",");
                    break;
                case "cache-control":
					cashcontrol =
						ReplaseValues(value, ",");
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
        private string SplitValues(string separat, List<string> values)
        {
            string value = string.Empty;
            for (int i = 0; i < values.Count; i++)
            {
                if ( i == values.Count - 1 )
                    value += values[i];
                else
                    value += values[i] + separat;
            }
            return value;
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

#endregion
    
#region Internal function

        internal bool SetReq()
        {
            if (!IsReq)
            {
                IsReq = true;
                return false;
            }
            return true;
        }
        internal bool SetRes()
        {
            if (!IsRes)
            {
                IsRes = true;
                return false;
            }
            return true;
        }
        internal bool SetEnd()
        {
            if (!IsEnd)
            {
                IsEnd = true;
                return false;
            }
            return true;
        }
        internal bool SetClose()
        {
            if (!Close)
            {
                Close = true;
                return false;
            }
            return true;
        }

#endregion
    }
}
