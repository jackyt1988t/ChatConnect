using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatConnect.WebModul.Chat.JsonObject
{
    class JsInfo
    {
        public int Room { get; set; }
        public DateTime Date { get; set; }

        public JsInfo()
        {
            Room = 0;
            Date = DateTime.Now;
        }
    }
}
