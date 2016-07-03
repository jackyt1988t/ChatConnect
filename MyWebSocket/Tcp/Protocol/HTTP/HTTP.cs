using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyWebSocket.Tcp.Protocol.HTTP
{
    public class HTTProtocol : BaseProtocol
    {
        public HTTProtocol(Socket tcp)
        {
			Tcp = tcp;
            State = 
				States.Connection;
					
			ObSync = new object();
            TaskResult = new TaskResult();
                Request = new Header();
                Response = new Header();
			Reader = new HTTPReader(32000);
			Writer = (
				ContextRq = ContextRs = 
					  new HTTPContext(this))
								     .__Writer;
			AllContext = new Queue<IContext>();
			
			OnEventConnect();
			Interlocked.CompareExchange(ref state, 0, 3);
		}
		public override TaskResult HandlerProtocol()
        {
            try
            {
				/*==================================================================
					Запускает функцию обработки пользоватлеьских данных,в случае
					если статус не был изменен выполняет переход к следующему
					обраотчику, обработчику чтения данных.						   
				===================================================================*/
				if (state == 0)
				{
					OnEventWork();
					/*==================================================================
						Проверяет сокет были получены данные или нет. Читаем данные 
						из сокета, если есть данные обрабатываем  их. Когда данные
						будут получены и обработаны переходим к следующему обработчику,
						обработчику отправки данных.
					==================================================================*/
					if (Interlocked.CompareExchange(ref state, 1, 0) != 0)
						return TaskResult;
						read();
					/*==================================================================
						Проверяет возможность отправки данных. Если данные можно 
						отправить запускает функцию для отправки данных, в случае 
						если статус не был изменен выполняет переход к следующему 
						обраотчику.
					==================================================================*/
					if (Interlocked.CompareExchange(ref state, 2, 1) != 1)
						return TaskResult;
						write();
					if (Interlocked.CompareExchange(ref state, 0, 2) == 2)
						return TaskResult;
				}
				/*============================================================
                                        Обработчик ошибок
                    Запускам функцию обработки ошибок. Если заголвоки были
                    отправлены закрываем соединение, если нет отправляем 
                    информацию о произошедшей ошибки. При ошибке клиента 
                    400 или ошибке сервера 500 указываем серверу после 
                    отправки данных закрыть моединение. 					   
                ==============================================================*/
								if (state == 4)
								{
									/////Ошибка/////
									OnEventError(Exception);
									Interlocked.Exchange(ref state, 0);
								}
                /*============================================================
                                        Закрываем соединеие						   
                ==============================================================*/
								if (state == 5)
								{
									state = 7;
								}
								if (state == 7)
								{
									OnEventClose();
									if (Tcp.Connected)
										Tcp.Close( 0 );
										TaskResult.Option  =  TaskOption.Delete;
								}
            }
            catch (HTTPException err)
            {
                Error(err);
            }
            catch (Exception err)
            {
                Error(new HTTPException("Критическая ошибка. " + err.Message, HTTPCode._500_, err));
                Log.Loging.AddMessage(err.Message + Log.Loging.NewLine + err.StackTrace, "log.log", Log.Log.Debug);
            }
            return TaskResult;
        }
        public override string ToString()
        {
            return "HTTP";
        }
        /// <summary>
        /// получает данные
        /// </summary>
        private void read()
        {
			/*
                Если функция Poll Вернет true проверяем наличие данных, если данных нет значит соединение
                было закрыто. Если есть данные читаем данные из сокета проверяем на наличие ошибок, если
                выполнение произошло с ошибкой, обрабатываем.
            */
			if (Tcp.Poll(0, SelectMode.SelectRead))
			{
				if (Tcp.Available == 0)
				{
					Close();
				}
				else
				{
					SocketError error;
					if ((error = Read()) != SocketError.Success)
					{
						// проверка является данная ошибка критической
						if (error == SocketError.WouldBlock
						 || error == SocketError.NoBufferSpaceAvailable)
						{
							if (!Reader.Empty)
								ContextRq.Handler();
						}
						else
						{
							/*         Текущее подключение было закрыто сброшено или разорвано          */
							if (error == SocketError.Disconnecting || error == SocketError.ConnectionReset
																   || error == SocketError.ConnectionAborted)
								Close();
							else
							{
								Error(new HTTPException("Ошибка чтения http данных: " + error.ToString(), HTTPCode._500_));
							}
						}
					}
				}
			}
						else
						{
							if (!Reader.Empty)
								ContextRq.Handler();
						}
        }
        /// <summary>
        /// Отправляет сообщение
        /// </summary>
        private void write()
        {
            /*
                Если функция Poll Вернет false или есть наличие данные, считываем данные из сокета, иначе закрываем
                соединение. Если проверка прошла успешно читаем данные из сокета
            */
            if (Tcp.Poll(0, SelectMode.SelectWrite))
            {
                SocketError error;
                if (!Writer.Empty)
                {
                    if ((error = Send()) != SocketError.Success)
                    {
                        // проверка является данная ошибка критической
                        if (error != SocketError.WouldBlock
							  && error != SocketError.NoBufferSpaceAvailable)
                        {
							/*         Текущее подключение было закрыто сброшено или разорвано          */
							if (error == SocketError.Disconnecting || error == SocketError.ConnectionReset
																   || error == SocketError.ConnectionAborted)
								Close();
							else
							{
								Error(new HTTPException("Ошибка чтения http данных: " + error.ToString(), HTTPCode._500_));
							}

						}
                    }
                }
            }
        }
    }
}
