using System;
using System.IO;
using System.Threading.Tasks;

namespace MyWebSocket.Log
{
	enum Log : short
	{
		Info =1,
		Debug = 2,
		Fatail = 3
	}
    static class Loging
    {
		public static object Sync = new object();
		/// <summary>
		/// Перевод строки
		/// </summary>
		public static string NewLine   =   "\r\n";
		/// <summary>
		/// функция для записи сообщения в файл
		/// </summary>
		/// <param name="message">информация о ошибке</param>
		/// <param name="path">путь до файла</param>
		/// <param name="mode">информация о приоритете ошибки</param>
		public static void AddMessage(string message, string path, Log mode)
        {
			message = mode.ToString() + ": " +
							  message + ". " +
							  DateTime.Now.ToString();
			
			FileInfo fileinfo = 
				 new FileInfo(path);            
            StreamWriter writer = null;
            try
            {
                if (fileinfo.Exists)
                {
                    writer = fileinfo.AppendText();
                }
                else
                {
                    writer = fileinfo.CreateText();
                }
						lock (Sync)
							writer.WriteLine(message);
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
        }
    }
}
