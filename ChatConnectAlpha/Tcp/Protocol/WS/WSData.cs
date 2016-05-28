using System;
using System.Text;

namespace ChatConnect.Tcp.Protocol.WS
{
    class WSData
    {
		public WSFin Fin
		{
			get;
		}
		public string _Text
		{
			get;
		}
        public byte[] _Data
        {
			get;
        }
		public WSOpcod Opcod
        {
            get;
            private set;
        }
        public DateTime Create
        {
            get;
            private set;
        }
		public WSData(byte[] data)
        {
			_Data = data;            
		}
		public WSData(byte[] data, WSOpcod opcod, WSFin fin) :
			this(data)
		{
			Fin = fin;
			Opcod = opcod;
			Create = DateTime.Now;
			if (opcod == WSOpcod.Text)
				_Text = Encoding.UTF8.GetString(data);		
		}
		/// <summary>
		/// Возвращает сырые данные
		/// </summary>
		/// <returns></returns>
		public byte[] ToByte()
		{
			return _Data;
		}
		/// <summary>
		/// Возвращает текстовые данные если Opcod = Text
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return _Text;
		}
	}
}
