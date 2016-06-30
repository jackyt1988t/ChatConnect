using System;
using System.Text;

namespace MyWebSocket.Tcp.Protocol.WS
{
    public class WSData
    {
		/// <summary>
		/// Фрагментация
		/// </summary>
		public WSFin Fin
		{
			get;
		}
		/// <summary>
		/// Содержут строку если получен текстовый фрейм
		/// </summary>
		public string _Text
		{
			get;
		}
		/// <summary>
		/// Содержит полученный массив байт(сырые данные)
		/// </summary>
        public byte[] _Data
        {
			get;
        }
		/// <summary>
		/// Содержит информацию о том какие данные были получены
		/// </summary>
		public WSOpcod Opcod
        {
            get;
            private set;
        }
		/// <summary>
		/// Показывает время поучения данных от удаленной стороны
		/// </summary>
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
