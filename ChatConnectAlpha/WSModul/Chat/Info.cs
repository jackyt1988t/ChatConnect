using System;

namespace ChatConnect.WebModul.Chat
{
    class Info
    {
        public string Ip;
        public string Descriptor;

        public Info(string ip)
        {
            Ip = ip;
            Descriptor = GetDescriptor();
        }

        protected static string __UNIQ = string.Empty;
        protected static object __SYNC = new object();

        protected static string GetDescriptor()
        {
            string descriptor = string.Empty;
            lock (   __SYNC   )
            {
                long time = DateTime.Now.Ticks;
                descriptor = Convert.ToString(time);

                if (descriptor.Equals(__UNIQ))
                    descriptor = Convert.ToString(time += 1);
                                         __UNIQ = descriptor;
            }
            return descriptor;
        }
    }
}
