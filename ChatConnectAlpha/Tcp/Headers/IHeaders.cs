using System;
using System.Collections.Generic;

namespace ChatConnect.Tcp
{
    interface IHeader : IDictionary<string, string>
    {
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
