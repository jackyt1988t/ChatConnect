namespace ChatConnect.WebModul.Chat
{
    interface IHandlerModule
    {
        void Clear();
        bool HandlerJson(IWebModule module, WebMooduleJson js);
        void HandlerError(IWebModule module, WebModuleException exc);
    }
}
