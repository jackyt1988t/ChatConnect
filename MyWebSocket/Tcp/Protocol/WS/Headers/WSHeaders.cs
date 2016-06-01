using System;

using System.Text;
using System.Text.RegularExpressions;

	using System.Security.Cryptography;

namespace MyWebSocket.Tcp.Protocol.WS
{
	static class WSHeader
	{
		const string KEY = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
		
		public static void Register(IHeader req, IHeader res)
		{
			if (!req.ContainsKey("websocket-protocol"))
			{
				if (!req.ContainsKey("sec-websocket-key"))
					throw new WSException("sec-websocket-key", WsError.HandshakeError,
															   WSClose.TLSHandshake);
				Versions(req, res);
			}
			else
			{
				if (!req.ContainsKey("sec-websocket-key1"))
					throw new WSException("sec-websocket-key", WsError.HandshakeError,
															   WSClose.TLSHandshake);
				if (!req.ContainsKey("sec-websocket-key2"))
					throw new WSException("sec-websocket-key", WsError.HandshakeError,
															   WSClose.TLSHandshake);
				VersionSample(req, res);
			}
		}
		public static void Versions(IHeader req, IHeader res)
		{
			SHA1 sha1 = SHA1.Create();

			string key = req["sec-websocket-key"] + KEY;
			byte[] val = sha1.ComputeHash(Encoding.UTF8.GetBytes(key));
									 key = Convert.ToBase64String(val);

			res.StartString = "HTTP/1.1 101 Switching Protocols";
			res.Add("Upgrade", "WebSocket");
			res.Add("Connection", "Upgrade");
			res.Add("Sec-WebSocket-Accept", key);

			sha1.Clear();
		}
		public static void VersionSample(IHeader req, IHeader res)
		{
			MD5 md5 = MD5.Create();

			string key1 = req["sec-websocket-key1"];
			long space_1 = Regex.Matches(key1, @" ").Count;
			string key2 = req["sec-websocket-key2"];
			long space_2 = Regex.Matches(key2, @" ").Count;

			Regex regex = new Regex(@"\D");
			key1 = regex.Replace(key1, "");
			long key1_64 = Convert.ToInt64(key1);
			key2 = regex.Replace(key2, "");
			long key2_64 = Convert.ToInt64(key2);

			int key1_32 = (int)(key1_64 / space_1);
			int key2_32 = (int)(key2_64 / space_2);

			byte[] keyb_byte = req.SegmentsBuffer.Dequeue();
			byte[] key1_byte = BitConverter.GetBytes(key1_32);
			byte[] key2_byte = BitConverter.GetBytes(key2_32);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(key1_byte);
				Array.Reverse(key2_byte);
			}

			byte[] key_string = new byte[16];
			Array.Copy(key1_byte, 0, key_string, 0, 4);
			Array.Copy(key2_byte, 0, key_string, 4, 4);
			Array.Copy(keyb_byte, 0, key_string, 8, 8);

			res.StartString = "HTTP/1.1 101 Web Socket Protocol Handshake";

			res.Add("Upgrade", "WebSocket");
			res.Add("Connection", "Upgrade");
			res.Body = md5.ComputeHash(key_string);

			md5.Clear();
		}
	}
}
