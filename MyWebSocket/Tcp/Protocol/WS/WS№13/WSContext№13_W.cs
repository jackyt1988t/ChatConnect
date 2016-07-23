using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using MyWebSocket.Tcp.Protocol.HTTP;
using System.Threading.Tasks;
using System.Collections.Generic;

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
            get;
            private set;
        }       
        /// <summary>
        /// Синхронизация текущего объекта
        /// </summary>
        public object __ObSync
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
        /// Информация об отправленных фреймах
        /// </summary>
        public List<WSFrameN13> Response
        {
            get;
            private set;
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

            __ObSync =     new object();
            Protocol =         protocol;
            Response = 
                 new List<WSFrameN13>();

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
            byte[] buffer = 
                Encoding.UTF8.GetBytes( s_data );
            return await AsyncMessage(buffer, 0,
                                        buffer.Length, 
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
            return await AsyncMessage(buffer, 0, 
                                        buffer.Length, 
                                            WSOpcod.Binnary);
        }

        #endregion
        /// <summary>
        /// Возвращает новый контекст
        /// </summary>
        /// <returns></returns>
        public IContext Context()
        {
            throw new NotFiniteNumberException("Context");
        }
        /// <summary>
        /// Сбрасывает выходной поток, если не использует основной
        /// </summary>
        public IContext Refresh()
        {
            lock (__ObSync)
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
            Cancel = true;
        }
        /// <summary>
        /// 
        /// </summary>
        public void Handler()
        {
            throw new NotFiniteNumberException("Handler");
        }
        /// <summary>
        /// Записывает в стандартный поток бинарный фрейм
        /// </summary>
        /// <param name="buffer"></param>
        public void Message(byte[] buffer)
        {
            Message(buffer, 0, 
                        buffer.Length);
        }
        /// <summary>
        /// Записывает в стандартный поток текстовый фрейм
        /// </summary>
        /// <param name="s_data"></param>
        public void Message(string s_data)
        {
            byte[] buffer = 
                Encoding.UTF8.GetBytes(s_data);
            
            Message(buffer, 0, 
                        buffer.Length, 
                            WSFin.Last, 
                                WSOpcod.Text);
        }
        /// <summary>
        /// Записывает в стандартный поток бинарный фрейм
        /// </summary>
        /// <param name="message">массив данных</param>
        /// <param name="offset">стартовая позиция</param>
        /// <param name="length">количество которое необходимо записать</param>
        public void Message(byte[] message, 
                                    int offset, 
                                        int length)
        {
            Message(message, offset, 
                        message.Length, 
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
        /// <param name="opcod">информация о текущем фрейме</param>
        public void Message(byte[] buffer, 
                                    int offset, 
                                        int length, 
                                            WSFin wsfin, 
                                                WSOpcod opcod)
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

            int OPCOD = 0;
            switch (opcod)
            {
                case WSOpcod.Text:
                    OPCOD = WSFrameN13.TEXT;
                    break;
                case WSOpcod.Ping:
                    OPCOD = WSFrameN13.PING;
                    break;
                case WSOpcod.Pong:
                    OPCOD = WSFrameN13.PONG;
                    break;
                case WSOpcod.Close:
                    OPCOD = WSFrameN13.CLOSE;
                    break;
                case WSOpcod.Binnary:
                    OPCOD = WSFrameN13.BINNARY;
                    break;
                case WSOpcod.Continue:
                    OPCOD = WSFrameN13.CONTINUE;
                    break;
                default:
                    throw new HTTPException("Неизвестный opcod");
            }

            if (WSFIN == 0)
                _next_ = true;
            
            lock (__ObSync)
            {
                
                if (Cancel)
                    throw new HTTPException("Отправка данных закончена");

                if (_next_ && OPCOD != WSFrameN13.CONTINUE)
                    throw new HTTPException("Отправка данных закончена");
                try
                {
                    WSFrameN13 frame = new WSFrameN13();
                               frame.BitFin   = WSFIN;
                               frame.BitPcod  = OPCOD;
                               frame.BitMask  = 0;
                               frame.PartBody = offset;
                               frame.LengBody = length;
                               frame.DataBody = buffer;


                    if (WSFIN == 1)
                        End();
                    else
                        _next_ = true;
                    
                    Response.Add(frame);
                    __Writer.Write(frame);

                    if (Log.Loging.Mode  >  Log.Log.Info)
                        Log.Loging.AddMessage(
                            "WS данные успешно добавлены", "log.log", Log.Log.Info);
                    else
                        Log.Loging.AddMessage(
                            "WS данные успешно добавлены" +
                            "\r\n" + WSDebug.DebugN13(frame), "log.log", Log.Log.Info);
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
            Protocol.Close(true);
        }
        /// <summary>
        /// Aсинхронно отправляет данные разбивая их на фреймы
        /// </summary>
        /// <returns>true если отправка закончилпсь успехом</returns>
        /// <param name="buffer">массив данных</param>
        /// <param name="offset">стартовая позиция</param>
        /// <param name="length">количество которое необходимо записать</param>
        /// <param name="opcod">Опкод фрейма который необходимо отправить</param>
        async
        protected Task<bool> AsyncMessage(byte[] buffer, 
                                                int offset, 
                                                    int length, 
                                                        WSOpcod opcod)
        {
            int i = 0;
            int _chunk = 1000 * 32;
            int _count = (int)((length - offset) / _chunk);
            int recive = (int)((length - offset) - _count * _chunk);

            lock (__ObSync)
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
                        Message(buffer, i * _chunk + offset, 
                                                        _chunk, 
                                                            WSFin.Next, 
                                                                    opcod);
                    else if (_count == i && recive == 0)
                        Message(buffer, i * _chunk + offset, 
                                                        _chunk, 
                                                            WSFin.Next, 
                                                                WSOpcod.Continue); 
                    else
                        Message(buffer, i * _chunk + offset, 
                                                        _chunk, 
                                                            WSFin.Last, 
                                                                WSOpcod.Continue);  
                }
                if (recive > 0)
                {
                    if (Protocol.State == States.Close
                         || Protocol.State == States.Disconnect)
                        return false;
                        
                    if (_count == 0)
                        Message(buffer, i * _chunk + offset, 
                                                        recive, 
                                                            WSFin.Last, 
                                                                    opcod); 
                    else
                        Message(buffer, i * _chunk + offset, 
                                                        recive, 
                                                            WSFin.Last, 
                                                                WSOpcod.Continue);
                }
                return true;
            });
        }
    }
}

