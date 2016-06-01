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
    static class Logout
    {
        /*               Записывает сообщение в лог файл               */
        private static void RecordMessage(string message, string logpath)
        {
			FileInfo fileinfo = 
				 new FileInfo(logpath);            
            /*   Поток для записи.   */
            StreamWriter writer = null;
			
            try
            {
                /* Проверка файла. */
                if (fileinfo.Exists)
                {
                    /*   Запись в конец файла.   */
                    writer = fileinfo.AppendText();
                }
                else
                {
                    /*   Создать файл и запись   */
                    writer = fileinfo.CreateText();
                }
						 /* записываем сообщение */
						 writer.WriteLine(message);
            }
            catch (Exception exc)
            {
				Console.WriteLine(  exc.Message  );     
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
        }
        /*               функция для записи сообщения в файл                 */
		public static void AddMessage(string message, string logpath, Log mode)
        {
			RecordMessage(mode.ToString() + ": " + message + ". " + DateTime.Now.ToString(), logpath);
        }
    }
}
