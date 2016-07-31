using System;
using System.Net.Sockets;
using MyWebSocket.Tcp.Protocol.WS;

namespace MyWebSocket.Tcp.Protocol
{
	public interface IProtocol : IAgregator
	{
		/// <summary>
		/// tcp/ip
		/// </summary>
		Socket Tcp
        {
            get;
        }
		/// <summary>
		/// Текущий статус
		/// </summary>
		States State
		{
			get;
		}
		/// <summary>
		/// Входящие заголвоки
		/// </summary>
		Header Request
		{
			get;
		}
		/// <summary>
		/// Исходящие заголвоки
		/// </summary>
		Header Response
		{
			get;
		}
		/// <summary>
		/// Поток чтения данных
		/// </summary>
		TcpStream TcpStream
		{
			get;
		}

		/// <summary>
		/// Закрыть текущий протокол
		/// </summary>
		void Close();
		/// <summary>
		/// Обработать ошибку протокола
		/// </summary>
		/// <param name="error">ошибка протокола</param>
		void Error(Exception error);
    }
}