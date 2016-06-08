using System;
using System.Text;
	using System.Net;
	using System.Net.Sockets;
using System.Security.Cryptography;
			using System.Threading;
		

using MyWebSocket.Tcp;
using MyWebSocket.Tcp.Protocol;
using MyWebSocket.Tcp.Protocol.WS;
using MyWebSocket.Tcp.Protocol.HTTP;



namespace MyWebSocket.Test
{
	class wsProtocolN13 : WSProtocolN13
	{
		static string CHECKKEY = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

		public IPEndPoint _Point
		{
			get;
		}		

		public wsProtocolN13(string adress, int port) :
			base()
		{
			Policy.SetPolicy(0, 1, 1, 1, 0, 32000);
													   
			_Point = new IPEndPoint(IPAddress.Parse(adress), port);
		}
		public void Connection()
		{
			bool open = true;
			Tcp = new Socket(_Point.AddressFamily, 
								SocketType.Stream, 
									ProtocolType.Tcp);
			try
			{
				Tcp.Connect(_Point);
				Session = new WSEssion(((IPEndPoint)Tcp.RemoteEndPoint).Address);

			}
			catch (SocketException err)
			{	
				open = false;
				Error(new WSException(
						"Ошибка при подключении", 
								err.SocketErrorCode, 
									WSClose.ServerError));
			}

			if (open)
			{
				HTTP http = new HTTPProtocol(Tcp);
				Request.StartString = 
				"GET /chat/websocket HTTP/1.1\r\n";
				Request.Add("Upgrade", "WebSocket");
				Request.Add("Connection", "Upgrade");
				Request.Add("Sec-WebSocket-Key", "");
				Request.Add("Sec-WebSocket-Protocol", "13");
				for (int i = 0; i < 24; i++)
				{
					Request["Sec-WebSocket-Key"] += 
						(char)new Random().Next(0x30, 0x79);
				}
				http.EventClose += (object sender, PEventArgs e) =>
				{
					open = false;
					Close( WSClose.TLSHandshake );
				};
				http.EventOnOpen += (object sender, PEventArgs e) =>
				{
					open = false;
					
					if (!http.Reader.Empty)
					{
						int recive = (int)http.Reader.PointR;
						int length = (int)http.Reader.Length;
						Reader.Write(
							http.Reader.Buffer, recive, length);
					}
					SHA1 sha = SHA1.Create();
					Response = http.Response;
					string checkkey = Convert.ToBase64String(
						sha.ComputeHash(
							Encoding.UTF8.GetBytes(
								Request["Sec-WebSocket-Key"] + CHECKKEY)));
					sha.Clear();
					if (!Response.ContainsKey("sec-websocket-accept")
						|| Response["sec-websocket-accept"]  !=  checkkey)
					{
						Close(WSClose.TLSHandshake);
					}
				};

				byte[] buffer = Request.ToByte();
				http.Message(buffer, 0, buffer.Length);

				while (true)
				{
					if (!open)
						break;
							http.TaskLoopHandlerProtocol();
							Thread.Sleep(5);
				}
			}

			while (true)
			{
				TaskResult TaskResult = TaskLoopHandlerProtocol();
					if (TaskResult.Option  ==  TaskOption.Delete)
						break;
				Thread.Sleep(5);
			}
		}
		public bool TestMessage(WSFrameN13 frame)
		{
			lock (Writer)
			{
				writer.Frame.Null();

				writer.Frame = frame;
				writer.Frame.InitializationHeader();
				if (Debug)
					WSDebug.DebugN13(writer.Frame);
				if (!Message(writer.Frame.DataHead, 0, (int)writer.Frame.LengHead))
					return false;
				else
					return Message(writer.Frame.DataBody, (int)writer.Frame.PartBody, (int)writer.Frame.LengBody);
			}
		}
		protected override void Connection(IHeader request, IHeader response)
		{
			OnEventConnect(request, response);
		}
	}
}
