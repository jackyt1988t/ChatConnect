using System;
using System.Collections.Generic;

namespace ChatConnect.Tcp
{
    interface IHeader : IDictionary<string, string>
    {
		int State
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

		void Req();
		void Res();
		void End();
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
