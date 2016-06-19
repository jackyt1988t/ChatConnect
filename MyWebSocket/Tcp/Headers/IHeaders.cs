using System;
using System.Collections.Generic;

namespace MyWebSocket.Tcp
{
    public interface IHeader
    {
		int ContentLength
		{
			get;
			set;
		}

		string Upgrade
		{
			get;
			set;
		}

		string Connection
		{
			get;
			set;
		}
		string ContentEncoding
		{
			get;
			set;
		}
		string TransferEncoding
		{
			get;
			set;
		}
		List<string> CashControl
		{
			get;
			set;
		}
		List<string> ContentType
		{
			get;
			set;
		}
		List<string> AcceptEncoding
		{
			get;
			set;
		}

		bool IsEnd
		{
			get;
		}
		bool IsReq
		{
			get;
		}
		bool IsRes
		{
			get;
		}
		bool Close
		{
			get;
		}
		byte[] _Body
        {
            get;
            set;
        }
		string File
		{
			get;
			set;
		}
		string Path
        {
            get;
            set;
        }
        string Http
        {
            get;
            set;
        }
        string Method
        {
            get;
            set;
        }
        string StartString
        {
            get;
            set;
        }
		DateTime TimeConnection
		{
			get;
		}
		Queue<byte[]> SegmentsBuffer
		{
			get;
		}
		byte[] ToByte();
		void AddHeader(string key, string value);
		void ClearHeaders();
		bool ContainsKeys(string key, bool @case = true);
		bool ContainsKeys(string key, out string value, bool @case = true);
	}
    interface IHeaders
    {
        IHeader ResHeaders
        {
            get;
        }
        IHeader ReqHeaders
        {
            get;
        }
    }
}
