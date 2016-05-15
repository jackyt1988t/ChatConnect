using System;
using System.IO;
using Newtonsoft.Json;

namespace ChatConnect.WebModul.Chat
{
    [JsonObject(MemberSerialization.OptIn)]
    class Work
    {
        const long Interval = (long)10 * 1000 * 1000 * 25;

        [JsonProperty]
        public long Min;
        [JsonProperty]
        public long Max;
        [JsonProperty]
        public long Ticks;
        [JsonProperty]
        public long Speed;
        [JsonProperty]
        public long Times;

        public Work()
        {
            Ticks = 0;
            Speed = 0;
            Times = DateTime.Now.Ticks;
        }
        public void Reset()
        {
            Min = 0;
            Max = 0;
            Ticks = 0;
            Speed = 0;
        }
        public bool TimeWait()
        {
            if (Times < DateTime.Now.Ticks)
            {
                Times = DateTime.Now.Ticks + Interval;
                return true;
            }
            return false;
        }
        public void AvgSpeed(long speed)
        {
            if (Min == 0)
                Min = speed;
            else if (Min > speed)
                Min = speed;
            if (Max == 0)
                Max = speed;
            else if (Max < speed)
                Max = speed;

            Ticks++;
            Speed = (Speed + speed) / 2;
        }
    }
}
