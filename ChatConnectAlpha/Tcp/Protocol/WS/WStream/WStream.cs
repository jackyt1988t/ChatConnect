using System;
using System.IO;

namespace ChatConnect.Tcp.Protocol.WS
{
	class WStream : Stream
	{
		public bool isRead
		{
			get
			{
				return (_p_r != _p_w);
			}
		}
		public bool isWrite
		{
			get
			{
				return (_p_r != _p_w);
			}
		}
		
		public override long Length
		{
			get
			{
				if (_p_w < _p_r)
					return (_len - _p_r) + _p_w;
				else
					return (_len - _p_w) - _p_r;
			}
		}
		public override long Position
		{
			get
			{
				return _p_r;
			}

			set
			{
				_p_r = value;
			}
		}
		public override bool CanRead
		{
			get
			{
				return true;
			}
		}
		public override bool CanSeek
		{
			get
			{
				return true;
			}
		}
		public override bool CanWrite
		{
			get
			{
				return true;
			}
		}

		private long _p_r;
		private long _p_w;
		protected long _len;
		protected byte[] _buffer;

		public override void Flush()
		{
			throw new NotImplementedException();
		}
		public virtual int ReadHead()
		{
			throw new NotImplementedException();
		}
		public virtual int ReadBody()
		{
			throw new NotImplementedException();
		}
		public override int ReadByte()
		{
			if (_p_r == _len)
				_p_r = 0;
			if (_p_r == _p_w)
				return -1;
			return _buffer[_p_r++];
		}				
		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}
		public override long Seek(long offset, SeekOrigin origin)
		{
			switch (origin)
			{
				case SeekOrigin.End:
					_p_w = _len;
					return _len;
				case SeekOrigin.Begin:
					_p_r = _len;
					return _len;
				case SeekOrigin.Current:
					_p_r = offset;
					return offset;
				default:
					return 0;
			}
		}
		unsafe public override int Read(byte[] buffer, int pos, int len)
		{
			int i;
			if (!isRead)
				return -1;
			fixed(byte* source = buffer, target = _buffer)
			{
				
				byte* ps = source + pos;
				for (  i = 0; i < len; i++  )
				{
					byte* pt = target + _p_r;
						  ps = pt;
				
					ps++;
					_p_w++;
					if (_p_r == _len)
						_p_r = 0;
					if (_p_r == _p_w)
						return i;
				}
			}
			return i;
		}
		unsafe public override void Write(byte[] buffer, int pos, int len)
		{
			int i;
			fixed(byte* source = _buffer, target = buffer)
			{
				byte* pt = target + pos;
				for (  i = 0; i < len; i++  )
				{
					byte* ps = source + _p_w;
						  ps = pt;
					
					pt++;
					_p_w++;
					if (_p_w == _len)
						_p_w = 0;
					if (_p_w == _p_r)
						throw new IOException("Переаолнение буффера");
				}
			}
		}
	}
}
