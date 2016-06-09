using System;
using System.Collections.Generic;

namespace MyWebSocket.Tcp
{
    interface IHeader
    {
		int ContentLength
		{
			get;
		}
		string TransferEncoding
		{
			get;
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
			set;
		}
		byte[] Body
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

		bool SetReq();
		bool SetRes();
		bool SetEnd();
		byte[] ToByte();
		void AddHeader(string key, string value);
		bool SearchHeader(string key, string value);
		bool ContainsKeys(string key, bool @case = true);
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
