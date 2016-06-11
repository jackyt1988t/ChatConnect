namespace MyWebSocket.Tcp.Protocol.HTTP
{
#region 1xx Инфо

	internal struct code100 : codexxx
	{
		/// <summary>
		/// продолжай
		/// </summary>
		public int value
		{
			get
			{
				return 100;
			}
		}
		public override string ToString()
		{
			return "Continue";
		}

	}
	internal struct code101 : codexxx
	{
		/// <summary>
		/// переключение протоколов
		/// </summary>
		public int value
		{
			get
			{
				return 101;
			}
		}
		public override string ToString()
		{
			return "Switching Protocols";
		}

	}
	internal struct code102 : codexxx
	{
		/// <summary>
		/// идёт обработка протокола
		/// </summary>
		public int value
		{
			get
			{
				return 102;
			}
		}
		public override string ToString()
		{
			return "Processing";
		}

	}

#endregion
	
#region 2xx Успех

	internal struct code200 : codexxx
	{
		/// <summary>
		/// ОК хорошо
		/// </summary>
		public int value
		{
			get
			{
				return 200;
			}
		}
		public override string ToString()
		{
			return "Success";
		}

	}
	internal struct code201 : codexxx
	{
		/// <summary>
		/// ОК создано
		/// </summary>
		public int value
		{
			get
			{
				return 201;
			}
		}
		public override string ToString()
		{
			return "Created";
		}

	}
	internal struct code204 : codexxx
	{
		/// <summary>
		/// нет содержимого
		/// </summary>
		public int value
		{
			get
			{
				return 204;
			}
		}
		public override string ToString()
		{
			return "NO CONTENT";
		}

	}
	internal struct code206 : codexxx
	{
		/// <summary>
		/// частичное содержимое
		/// </summary>
		public int value
		{
			get
			{
				return 204;
			}
		}
		public override string ToString()
		{
			return "Partial Content";
		}

	}

#endregion

#region 3xx Перенаправление

	internal struct code300 : codexxx
	{
		/// <summary>
		/// перенаправление
		/// </summary>
		public int value
		{
			get
			{
				return 300;
			}
		}
		public override string ToString()
		{
			return "Redirection";
		}

	}

	#endregion

#region 4xx Ошибка клиента.

	internal struct code400 : codexxx
	{
		/// <summary>
		/// плохой, неверный запрос
		/// </summary>
		public int value
		{
			get
			{
				return 400;
			}
		}
		public override string ToString()
		{
			return "Bad Request";
		}

	}
	internal struct code403 : codexxx
	{
		/// <summary>
		/// запрещено, неверный запрос
		/// </summary>
		public int value
		{
			get
			{
				return 403;
			}
		}
		public override string ToString()
		{
			return "Forbidden";
		}

	}
	internal struct code404 : codexxx
	{
		/// <summary>
		/// не найдено, неверный запрос
		/// </summary>
		public int value
		{
			get
			{
				return 404;
			}
		}
		public override string ToString()
		{
			return "Not Found";
		}

	}

#endregion

#region 5xx Ошибка сервера.

	internal struct code500 : codexxx
	{
		/// <summary>
		/// ошибка сервера
		/// </summary>
		public int value
		{
			get
			{
				return 500;
			}
		}
		public override string ToString()
		{
			return "Server Error";
		}

	}
	internal struct code501 : codexxx
	{
		/// <summary>
		/// не реализовано
		/// </summary>
		public int value
		{
			get
			{
				return 501;
			}
		}
		public override string ToString()
		{
			return "Not Implemented";
		}

	}
	internal struct code503 : codexxx
	{
		/// <summary>
		/// сервис недоступен
		/// </summary>
		public int value
		{
			get
			{
				return 503;
			}
		}
		public override string ToString()
		{
			return "Unavailable";
		}

	}

#endregion 
}
