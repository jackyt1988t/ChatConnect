using System;

namespace MyWebSocket.Tcp.Protocol
{
	public interface IContext
	{
		/// <summary>
		/// Закончена обработка
		/// </summary>
		bool Cancel
		{
			get;
		}
		/// <summary>
		/// Объект для синхронизации
		/// </summary>
		object ObSync
		{
			get;
		}

		IContext Refresh();
		/// <summary>
		/// Новый контекст
		/// </summary>
		/// <returns>новый контекст</returns>
		IContext Context();
		/// <summary>
		/// Получает данные
		/// </summary>
		void Handler();
		/// <summary>
		/// Добавить сообщение в поток
		/// </summary>
		/// <param name="buffer">строка данных</param>
		/// <returns>true если добавлено</returns>
		void Message(string	buffer);
		/// <summary>
		/// Добавить сообщение в поток
		/// </summary>
		/// <param name="buffer">буффер данных</param>
		/// <returns>true если добавлено</returns>
		void Message(byte[] buffer);
		/// <summary>
		/// Добавить сообщение в поток
		/// </summary>
		/// <param name="buffer">буффер данных</param>
		/// <param name="recive">начальная позиция</param>
		/// <param name="length">необходимо записать</param>
		/// <returns>true если добавлено</returns>
		void Message(byte[] buffer, int recive, int length);
		
	}
}
