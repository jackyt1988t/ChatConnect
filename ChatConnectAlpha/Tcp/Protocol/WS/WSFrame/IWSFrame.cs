namespace ChatConnect.Tcp.Protocol.WS
{
    interface IWSFrame
    {
        int BitFind
        {
            get;
            set;
        }
        int BitRsv1
        {
            get;
            set;
        }
        int BitRsv2
        {
            get;
            set;
        }
        int BitRsv3
        {
            get;
            set;
        }
        int BitPcod
        {
            get;
            set;
        }
        int BitLeng
        {
            get;
            set;
        }
        bool GetsHead
        {
            get;
            set;
        }
        bool GetsBody
        {
            get;
            set;
        }
        long PartBody
        {
            get;
            set;
        }
        long PartHead
        {
            get;
            set;
        }
        long LengHead
        {
            get;
            set;
        }
        long LengBody
        {
            get;
            set;
        }
        byte[] DataHead
        {
            get;
            set;
        }
        byte[] DataBody
        {
            get;
            set;
        }

        void Clear();
		byte[] GetDataFrame();
    }
}