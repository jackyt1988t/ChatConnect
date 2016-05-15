using System;
using System.IO;
using System.Threading;

using Newtonsoft.Json;
using ChatConnect.Log;
using ChatConnect.Chats;
using ChatConnect.WebModul.Chat.JsonObject;
using ChatConnect.Tcp.Protocol;

namespace ChatConnect.WebModul.Chat
{
    class HandlerModuleN : IHandlerModule
    {
    	private volatile int _use;
        protected IWebModule __WM;

        public HandlerModuleN()
        {

        }
        public HandlerModuleN(IWebModule wm)
        {
            __WM = wm;
            __WM.WS.EventWork += InitFunc;
        }
        public virtual void AddWM()
        {
            WsChats.SetUnregisterUser(__WM);
        }
        public virtual void DelWM()
        {
            WsChats.UnsetUnregisterUser(__WM);
        }
        public virtual void Clear()
        {
			
			if (Interlocked.CompareExchange(ref _use, 0, 1) == 0)
			{
				__WM.WS.EventWork -= InitFunc;
			}
			else
			{
				DelWM();
				__WM.WS.EventWork -= EventWork;
				__WM.WS.EventClose -= EventClose;
			}

        }

        public virtual void InitFunc(object sender, PEventArgs e)
        {			
			if (Interlocked.CompareExchange(ref _use, 1, 0) == 0)
			{
				AddWM();				
				__WM.WS.EventWork -= InitFunc;
				__WM.WS.EventWork += EventWork;
				__WM.WS.EventClose += EventClose;
			}
        }
		public virtual void EventWork(object sender, PEventArgs e)
		{

		}
		public virtual void EventClose(object sender, PEventArgs e)
        {
			Clear();
        }
        public virtual bool HandlerJson(IWebModule module, WebMooduleJson js)
        {
            return false;
        }
        /*Вызывается когда происходит пользовательская ошибка во вермя выполнения*/
        public virtual void HandlerError(IWebModule module, WebModuleException exc)
        {

        }
    }
}
