using System;
using System.IO;
using System.Collections.Generic;

namespace MyWebSocket.Tcp.Protocol.WS.WS_13
{
    /// <summary>
    /// Объект WSFramesN13
    /// </summary>
    public class WSFramesN13
    {
        public bool isEnd
        {
            get;
            set;
        }
        /// <summary>
        /// Опкод
        /// </summary>
        /// <value>The opcod.</value>
        public WSOpcod Opcod
        {
            get;
            set;
        }
        /// <summary>
        /// Информация об отправленных фреймах
        /// </summary>
        public List<WSFrameN13> __Frames
        {
            get;
        }
        /// <summary>
        /// Создает экземпляр объекта WSFrames
        /// </summary>
        public WSFramesN13()
        {
            __Frames = new List<WSFrameN13>();
        }
    }
}

