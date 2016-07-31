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
		/// ������� ������
		/// </summary>
		States State
		{
			get;
		}
		/// <summary>
		/// �������� ���������
		/// </summary>
		Header Request
		{
			get;
		}
		/// <summary>
		/// ��������� ���������
		/// </summary>
		Header Response
		{
			get;
		}
		/// <summary>
		/// ����� ������ ������
		/// </summary>
		TcpStream TcpStream
		{
			get;
		}

		/// <summary>
		/// ������� ������� ��������
		/// </summary>
		void Close();
		/// <summary>
		/// ���������� ������ ���������
		/// </summary>
		/// <param name="error">������ ���������</param>
		void Error(Exception error);
    }
}