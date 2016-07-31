using System;
using System.IO;

namespace MyWebSocket.Tcp.Protocol
{

    /// <summary>
    /// Кольцевой поток данных
    /// </summary>
    public class MyStream : Stream
    {
#region	public property
		/// <summary>
		/// Минимальный размер потока
		/// </summary>
		public static int MINRESIZE = 10000;
		/// <summary>
		/// Максимальный размер потока
		/// </summary>
		public static int MAXRESIZE = 100000000;

		/// <summary>
		/// Длинна потока
		/// </summary>
		public long Count
        {
            get
            {
                return count;
            }
        }
        /// <summary>
        /// true если есть не прочитанные данные
        /// </summary>
        public bool Empty
        {
            get
            {
                if (_loop)
                    return false;
                else
                    return (__p_r == __p_w);
            }
        }
        /// <summary>
        /// true если в потоке есть место для записи
        /// </summary>
        public long Clear
        {
            get
            {
                if (_loop)
                    return (__p_r - __p_w);
                else
                    return (count - __p_w) + __p_r;
            }
        }
        public object obSync
        {
            get;
            private set;
        }
        /// <summary>
        /// Длинна не прочитанных байт. Чтобы узнать длинну потока
        /// необходимо воспользоваться свойством Count
        /// </summary>
        public override long Length
        {
            get
            {
                lock (obSync)
                {
                    if (_loop)
                        return (count - __p_r) + __p_w;
                    else
                        return (__p_w - __p_r);
                }
            }
        }
        /// <summary>
        /// Показывет возможночть чтения
        /// </summary>
        public override bool CanRead
        {
            get
            {
                return true;
            }
        }
        /// <summary>
        /// Показывает возможность записи
        /// </summary>
        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }
        /// <summary>
        /// Показывает возможность поиска
        /// </summary>
        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }
        /// <summary>
        /// Увеличивает текущую позицию только внутри записанных байт
        /// если значение выше Length возникнет исключение.
        /// Всегда возвращает 0
        /// </summary>
        public override long Position
        {
            get
            {
                return 0;
            }

            set
            {
                lock (obSync)
                {
                    if (value < 0)
                        throw new ArgumentOutOfRangeException("value");
                    if (value > Length)
                        throw new IOException("Превышена допустимая длинна");
                    if ((value + __p_r) < Count)
                        __p_r = value + __p_r;
                    else
                    {
                        _loop = false;
                        __p_r = value - ( Count - __p_r );
                    }
			if (Count > MINRESIZE)
			{
				int resize = 
				    (int)Count / 4;
				if (resize > Length)
					Resize(resize);
			}
		}
            }
        }

#endregion
#region internal property 
        long __p_r;
        /// <summary>
        /// Указатель на текущую позицию записи данных
        /// </summary>
        internal long PointW
        {
            get
            {
                return __p_w;
            }
            set
            {
                lock (obSync)
                {
                    if (value < 0 || value > Count)
                        throw new ArgumentOutOfRangeException("value");
                    if (value == Count)
                    {
                        __p_w = 0;
                        _loop = true;
                    }
                    else
                        __p_w = value;
                }
            }
        }
        long __p_w;
        /// <summary>
        /// Указатель на текущую позицию чтения данных
        /// </summary>
        internal long PointR
        {
            get
            {
                return __p_r;
            }
            set
            {
                lock (obSync)
                {
                    if (value < 0 || value > Count)
                        throw new ArgumentOutOfRangeException("value");
                    if (value == Count)
                    {
                        __p_r = 0;
                        _loop = false;
                    }
                    else
                        __p_r = value;
                }
            }
        }
        /// <summary>
        /// Хранилище
        /// </summary>
        internal byte[] Buffer
        {
            get
            {
                return _buffer;
            }
        }

#endregion
#region protected property

		/// <summary>
		/// По куругу
		/// </summary>
		protected bool _loop;
        /// <summary>
        /// длинна потока
        /// </summary>
        protected long count;
        /// <summary>
        /// хранилише данных
        /// </summary>
        protected byte[] _buffer;

#endregion

#region constructor

        /// <summary>
        /// Создает новый кольцевой буффер
        /// </summary>
        /// <param name="length">длинна кольцевого буффера</param>
        public MyStream(int length) : 
            base()
        {
            count = length;
            obSync = new object();
            _buffer = new byte[length];
        }

#endregion
#region virtual	finction
        /// <summary>
        /// сбрасывает поток в начальное положение
        /// </summary>
        public virtual void Reset()
        {
            __p_r = 0;
            __p_w = 0;
            _loop = false;
        }
		/// <summary>
		/// изменяет емкость(длинну) кольцевого потока
		/// </summary>
		/// <param name="resize">емкость потока</param>
		public virtual void Resize(int resize)
        {
            if (resize < 0)
                throw new ArgumentOutOfRangeException("length");
            lock (obSync)
            {
                if (resize < Length)
                    throw new IOException("Не хватает места для перезаписи");
				if (resize > MAXRESIZE)
					resize = MAXRESIZE;
				if (resize < MINRESIZE)
					resize = MINRESIZE;
				
				int length = (int)Length;

					byte[] buffer = new byte[  resize  ];

				if (!_loop)
					Array.Copy(_buffer, __p_r, buffer, 0, length);
				else
				{
					int offset = (int)(  Count - __p_r  );
					Array.Copy(_buffer, __p_r, buffer, 0, offset);
					Array.Copy(_buffer, 0, buffer, offset, __p_w);
				}

					_loop   = false;
					__p_r   = 0;
					__p_w   = length;
					count   = resize;
					_buffer = buffer;
            }
        }
#endregion
#region ovveride finction
        public override void Flush()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// увеличивает текущую длинну кольцевого потока, 
        /// не может выходить за рамки длинны буффера потока
        /// </summary>
        /// <param name="value">количетво на которое необходимо увеличить длинну потока</param>
                        
        public override void SetLength(long value)
        {
            lock (obSync)
            {
                if (value > Clear)
                    throw new IOException("Недостаточно свободного места");
                if (__p_w + value < count)
                    __p_w = value + __p_w;
                else
                {
                    _loop = true;
                    __p_w = value - (count - __p_w);
                }
            }
        }
        #endregion
#region read write finction 
        /// <summary>
		/// Читает один байт
		/// </summary>
		/// <returns>-1 если достигнут конец потока</returns>
        public override int ReadByte()
        {
            lock (obSync)
            {
                if (Empty)
                    return -1;
                return _buffer[PointR++];
            }
        }
        /// <summary>
        /// Записывает данные в поток
        /// </summary>
        /// <param name="buffer">буффер данных для записи</param>
        /// <param name="offset">начальная позиция</param>
        /// <param name="length">количество которое необходимо записать</param>
        /// <returns></returns>
        unsafe public override int Read(byte[] buffer, int offset, int length)
        {
            int i = 0;
            
			if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset");
            if (length < 0)
                throw new ArgumentOutOfRangeException("length");
            if ((offset + length) > buffer.Length)
                throw new ArgumentOutOfRangeException("length + offset");
            lock (obSync)
            {
				if (length  > Length)
                    length = (int)Length;
                fixed (byte* source = buffer, target = _buffer)
                {

                    byte* ps = source + offset;
                    for (i = 0; i < length; i++)
                    {
                        byte* pt = target + PointR;

                        *ps = *pt;
                        ps++;
                        PointR++;
                        
                    }
                }
				if (Count > MINRESIZE)
				{
					int resize = 
						(int)Count / 4;
					if (resize > Length)
						Resize(  resize  );
				}
			}
            return i;
        }
        /// <summary>
        /// Не поодерживается данной реализацией
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        unsafe public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Читает данные из потока
        /// </summary>
        /// <param name="buffer">буффер в который будут записаны данные</param>
        /// <param name="offset">начальная позиция</param>
        /// <param name="length">количество которое необходимо прочитать</param>
        unsafe public override void Write(byte[] buffer, int offset, int length)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset");
            if (length < 0)
                throw new ArgumentOutOfRangeException("length");
            if ((offset + length) > buffer.Length)
                throw new ArgumentOutOfRangeException("length + offset");
            lock (obSync)
            {
                int i;
				if (length > Clear)
				{
                    int resize = 2 * (int)Count;

                    if (resize < length)
                        resize = 
                            length + (int)Count;
					Resize(  resize  );
				}
			if (length > Clear)
				throw new IOException("Недостаточно места, MAXRESIZE");
                fixed (byte* source = Buffer, target = buffer)
                {
                    byte* pt = target + offset;
                    for (i = 0; i < length; i++)
                    {
                        byte* ps = source + PointW;

                        *ps = *pt;
                        pt++;
                        PointW++;
                    }
                }
            }
        }
#endregion
    }
}
