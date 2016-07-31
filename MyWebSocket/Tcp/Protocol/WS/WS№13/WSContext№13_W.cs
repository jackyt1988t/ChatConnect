using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using MyWebSocket.Tcp.Protocol.HTTP;

namespace MyWebSocket.Tcp.Protocol.WS.WS_13
{
    /// <summary>
    /// Объект WSContext_13_W
    /// </summary>
    public class WSContext_13_W  : IContext
    {
        internal bool _to_;
        internal bool _ow_;
        internal bool _next_;

        /// <summary>
        /// Протолкол HTTP
        /// </summary>
        internal HTTProtocol Protocol;
        /// <summary>
        /// Последняя ошибка
        /// </summary>
        internal WSException _1_Error;

        /// <summary>
        /// Закончена обр-ка
        /// </summary>
        public bool Cancel
        {
            get
            {
                return Response.isEnd;
            }
        }

        /// <summary>
        /// Синхронизация текущего объекта
        /// </summary>
        public object ObSync
        {
            get;
            private set;
        }
        /// <summary>
        /// Поток записи
        /// </summary>
        public WSWriterN13 __Writer
        {
            get;
        }
        /// <summary>
        /// Коллекция
        /// </summary>
        public WSFramesN13 Response
        {
            get
            {
                return __Writer.__Frame;
            }
        }     

        /// <summary>
        /// Создает контекст получения, отправки данных
        /// </summary>
        /// <param name="protocol">Протокол обработки данных</param>
        /// <param name="ow">
        /// true чтобы использовать основной поток
        /// иначе создать виртуальный поток для отправки
        /// </param>
        public WSContext_13_W(HTTProtocol protocol, bool ow)
        {
            _ow_ = ow;

            ObSync   =   new object();
            Protocol =       protocol;

            if (!_ow_)
                __Writer = new WSWriterN13(new MyStream(4096));
            else
                __Writer = new WSWriterN13(Protocol.GetStream);
        }
        #region async
        /// <summary>
        /// Асинхронной отправляет строковый фрейм
        /// или несколько форматированных фремов...
        /// </summary>
        /// <returns>true если сообщение было отправлено</returns>
        /// <param name="s_data">строка которую необходимо отправить</param>
        async
        public Task<bool> AsMssg(string s_data)
        {
            int length;
            int offset = 0;
            byte[] buffer = 
                Encoding.UTF8.GetBytes( s_data );
                   length     =     buffer.Length;

            return await AsyncMessage(buffer, 
                                          offset,
                                              length,
                                                  WSFin.Last,
                                                      WSOpcod.Text);
        }
        /// <summary>
        /// Асинхронной отправляет строковый фрейм
        /// или несколько форматированных фремов...
        /// </summary>
        /// <returns>true если сообщение было отправлено</returns>
        /// <param name="buffer">маасив байт который необходимо отправить</param>
        async
        public Task<bool> AsMssg(byte[] buffer)
        {
            int offset = 0;
            int length = buffer.Length;

            return await AsyncMessage(buffer, 
                                          offset,
                                              length,
                                                  WSFin.Last,
                                                      WSOpcod.Binnary);
        }

        #endregion
        /// <summary>
        /// Возвращает новый контекст
        /// </summary>
        /// <returns></returns>
        public IContext Context()
        {
            throw new NotImplementedException("Context");
        }
        /// <summary>
        /// Сбрасывает выходной поток, если не использует основной
        /// </summary>
        public IContext Refresh()
        {
            lock (ObSync)
            {
                if (!_ow_)
                {
                    _ow_ = true;

                    __Writer.Stream.CopyTo( Protocol.GetStream );
                    __Writer.Stream.Dispose();

                    if (!Cancel)
                        __Writer.Stream  =  Protocol.GetStream;
                }
            }
            return this;
        }
        /// <summary>
        /// Завершает использование контекста
        /// </summary>
        public void End()
        {
            throw new NotImplementedException("End");
        }
        /// <summary>
        /// 
        /// </summary>
        public void Handler()
        {
            throw new NotImplementedException("Handler");
        }
        /// <summary>
        /// Записывает в стандартный поток бинарный фрейм
        /// </summary>
        /// <param name="buffer"></param>
        public void Message(byte[] buffer)
        {
            int offset = 0;
            int length    =    buffer.Length;
            Message(buffer, 
                        offset, 
                            length, 
                                WSFin.Last, 
                                    WSOpcod.Text);
        }
        /// <summary>
        /// Записывает в стандартный поток текстовый фрейм
        /// </summary>
        /// <param name="s_data"></param>
        public void Message(string s_data)
        {
            int offset = 0;
            int length = 0;
            byte[] buffer = 
                Encoding.UTF8.GetBytes(s_data);
                   length    =    buffer.Length;
            
            Message(buffer, 
                        offset, 
                            length, 
                                WSFin.Last, 
                                    WSOpcod.Text);
        }
        /// <summary>
        /// Записывает в стандартный поток бинарный фрейм
        /// </summary>
        /// <param name="message">массив данных</param>
        /// <param name="offset">стартовая позиция</param>
        /// <param name="length">количество которое необходимо записать</param>
        public void Message(byte[] buffer, 
                                    int offset, 
                                        int length)
        {
            Message(buffer, 
                        offset, 
                            length, 
                                WSFin.Last, 
                                    WSOpcod.Binnary);
        }
        /// <summary>
        /// Записывает в стандартный поток указанный фрейм
        /// </summary>
        /// <param name="buffer">массив данных</param>
        /// <param name="offset">стартовая позиция</param>
        /// <param name="length">количество которое необходимо записать</param>
        /// <param name="wsfin">указывает последоватьлность фрейма</param>
        /// <param name="wsopcod">информация о текущем фрейме</param>
        public void Message(byte[] buffer, 
                                    int offset, 
                                        int length, 
                                            WSFin wsfin, 
                                                WSOpcod wsopcod)
        {
            int WSFIN = 0;
            switch (wsfin)
            {
                case WSFin.Next:
                    WSFIN = 0;
                    break;
                case WSFin.Last:
                    WSFIN = 1;
                    break;
                default:
                    throw new HTTPException("Неизвестный fin");
            }

            int WSOPCOD = 0;
            switch (wsopcod)
            {
                case WSOpcod.Text:
                    WSOPCOD = WSFrameN13.TEXT;
                    break;
                case WSOpcod.Ping:
                    WSOPCOD = WSFrameN13.PING;
                    break;
                case WSOpcod.Pong:
                    WSOPCOD = WSFrameN13.PONG;
                    break;
                case WSOpcod.Close:
                    WSOPCOD = WSFrameN13.CLOSE;
                    break;
                case WSOpcod.Binnary:
                    WSOPCOD = WSFrameN13.BINNARY;
                    break;
                case WSOpcod.Continue:
                    WSOPCOD = WSFrameN13.CONTINUE;
                    break;
                default:
                    throw new HTTPException("Неизвестный opcod");
            }
            lock (ObSync)
            {
                try
                {
                    __Writer.Write(new WSFrameN13(buffer, offset, length)
                                   {
                                       BitFin  = WSFIN,
                                       BitPcod = WSOPCOD,
                                   });
                }
                catch (Exception error)
                {
                    Protocol.Close();
                        Log.Loging.AddMessage(
                            "Ошибка при записи данных WS" +
                            error.Message + ".\r\n" + error.StackTrace, "log.log", Log.Log.Fatal);

                }
            }
        }
        /// <summary>
        /// Обрабатывает ошибки произошедшие в текущем
        /// контексте и записывает их в основной протокол
        /// </summary>
        /// <param name="_1_error">1 error.</param>
        protected void HandlerError(WSException _1_error)
        {
            Protocol.Close();
        }
        /// <summary>
        /// Aсинхронно отправляет данные разбивая их на фреймы
        /// </summary>
        /// <returns>true если отправка закончилпсь успехом</returns>
        async
        protected Task<bool> AsyncMessage(byte[] buffer, 
                                                int offset, 
                                                    int length, 
                                                        WSFin wsfin, 
                                                            WSOpcod wsopcod)
        {
            int i = 0;
            int _chunk = 1000 * 32;
            int _count = (int)((length - offset) / _chunk);
            int recive = (int)((length - offset) - _count * _chunk);

            lock (ObSync)
            {
                if (_to_)
                    throw new HTTPException("Выполняется асинхронная операция");
                else
                    _to_ = true;
            }

            return await Task.Run<bool>(() =>
            {
                    
                while (i++ < _count)
                {
                    if (Protocol.State == States.Close
                         || Protocol.State == States.Disconnect)
                        return false;

                    if (i == 0 
                         && (_count > 1 || recive > 0))
                        Message(buffer, 
                                    (i * _chunk + offset), 
                                                    _chunk, 
                                                        WSFin.Next, 
                                                            wsopcod);
                    else if (_count == i && recive == 0)
                        Message(buffer, 
                                    (i * _chunk + offset), 
                                                    _chunk, 
                                                        WSFin.Next, 
                                                            WSOpcod.Continue); 
                    else
                        Message(buffer, 
                                    (i * _chunk + offset), 
                                                    _chunk, 
                                                        wsfin, 
                                                            WSOpcod.Continue);  
                }
                if (recive > 0)
                {
                    if (Protocol.State == States.Close
                         || Protocol.State == States.Disconnect)
                        return false;
                        
                    if (_count == 0)
                        Message(buffer, 
                                    (i * _chunk + offset), 
                                                    recive, 
                                                        wsfin, 
                                                            wsopcod); 
                    else
                        Message(buffer, 
                                    (i * _chunk + offset), 
                                                    recive, 
                                                        wsfin, 
                                                            WSOpcod.Continue);
                }
                        Log.Loging.AddMessage(
                            "Асинхронная отправка WS данных." +
                            " Отправлено: " + length + " байт данных." , "log.log", Log.Log.Info);
                
                return true;
            });
        }
    }
}

