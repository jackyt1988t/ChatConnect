using System;

namespace ChatConnect.WebModul.Chat
{
    class HandlerModuleM : HandlerModuleU
    {
        public HandlerModuleM() :
            base()
            {

            }
        public HandlerModuleM(IWebModule wm) :
            base(wm)
            {

            }
        private void BannU()
        {
            
        }
        private void UnbannU()
        {

        }

        public override bool HandlerJson(IWebModule module, WebMooduleJson js)
        {
            if (base.HandlerJson(module, js))
                return true;
            
            switch (js.JsEvent)
            {
                case "bannu":
                    BannU();
                    return true;
                case "unbannu":
                    UnbannU();
                    return true;
                default:
                    return false;
            }
        }
        public override bool HandlerExpr(IWebModule module,  WebModuleCommand cmd)
        {
            if (base.HandlerExpr(module, cmd))
                return true;

            switch (cmd.Val)
            {
                case "/console":
                    Console.WriteLine("Console+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
                    return true;
                default:
                    return false;
            }
        }
    }
}
