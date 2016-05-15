using System;
using ChatConnect.WebModul.Chat.JsonObject;

namespace ChatConnect.WebModul.Chat
{
    class HandlerModuleH : HandlerModuleU
    {
        /*private void BannC(JsBannC bannc)
        {

        }
        private void UnbannC(JsUnbannC bannc)
        {

        }
        public override bool HandlerJson(WebSocketModule module, WebMooduleJson js)
        {
            if (base.HandlerJson(module, js))
                return true;

            switch (js.JsEvent)
            {
                case "bannc":
                    BannC(JsCheckObject.JsCheckDesirializer<JsBannC>(js.JsReader));
                    return true;
                case "unbannc":
                    UnbannC(JsCheckObject.JsCheckDesirializer<JsUnbannC>(js.JsReader));
                    return true;
                default:
                    return false;
            }
        }*/
        public override bool HandlerExpr(IWebModule module, WebModuleCommand cmd)
        {
            if (base.HandlerExpr(module, cmd))
                return true;
            return false;
        }
    }
}
