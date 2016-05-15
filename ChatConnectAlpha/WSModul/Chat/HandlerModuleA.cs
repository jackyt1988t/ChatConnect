using System;
using Newtonsoft.Json;
using ChatConnect.Chats;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ChatConnect.WebModul.Chat
{
    class HandlerModuleA : HandlerModuleM
    {
        public HandlerModuleA() :
            base()
            {
                
            }
        public HandlerModuleA(IWebModule wm) :
            base(wm)
            {

        }
        public override bool HandlerExpr(IWebModule module, WebModuleCommand cmd)
        {
            if (base.HandlerExpr(module, cmd))
                return true;

            switch (cmd.Val)
            {
                case "/echo":
                    return true;
                default:
                    return false;
            }
        }
    }
}