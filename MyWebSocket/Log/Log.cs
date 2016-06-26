using System;
using System.IO;
using System.Threading.Tasks;

namespace MyWebSocket.Log
{
	enum Log : short
	{
		Info =1,
		Debug = 2,
		Fatal = 3
	}
    static class Loging
    {
		/// <summary>
		/// Мод записи
		/// </summary>
		public static Log Mode = Log.Info;
		/// <summary>
		/// Перевод строки
		/// </summary>
		public static string NewLine = "\r\n";
		
		private static object ObjSync = new object();
		/// <summary>
		/// функция для записи сообщения в файл
		/// </summary>
		/// <param name="message">информация о ошибке</param>
		/// <param name="path">путь до файла</param>
		/// <param name="mode">информация о приоритете ошибки</param>
		public static void AddMessage(string message, string path, Log mode)
        {
			if (Mode < mode)
				return;
			
			message = mode.ToString() + ": " +
							  message + ". " +
							  DateTime.Now.ToString();
						            
            StreamWriter writer = null;
            try
            {
				FileInfo fileinfo = new FileInfo(path);
				lock (ObjSync)
				{
					if (fileinfo.Exists)
					{
						writer = fileinfo.AppendText();
					}
					else
					{
						writer = fileinfo.CreateText();
					}
						writer.WriteLine(   message   );
				}
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
        }
    }
}
