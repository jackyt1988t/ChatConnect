namespace ChatConnect.Tcp.Protocol
{
    interface IBuffer
    {
        int Count
        {
            get;
        }
        int Length
        {
            get;
        }
        object SyncRoot
        {
            get;
        }

        byte[] Pull();
         void  Push(byte[] data);
         void  Push(byte[] data, int length);
    }
}
