## Запуск тестового сервера
Включаем отдачу статических файлов по протоколу http.
```C#
using ChatConnect.Tcp.Protocol.HTTP;

// Обрабатываем новый http запрос
HTTP.EventConnect += (object obj, PEventArgs a) =>
{
	// Объект Http
	HTTP Http = obj as HTTP;
	
	switch (Http.Request.Path)
	{
		case "/":
		  // отправляем index.html
			Http.File("Html/index.html", "html");
			break;
		default:
			Http.File("Html" + Http.Request.Path, Http.Request.File);
			break;
	}
	// Вывод информации о полученных заголовках
	Console.WriteLine(Http.Request.ToString());
};
```
Html путь к папке сфайлами
Данный тест это cтраничка в браузере которая подключается к нашему серверу по адресу 127.0.0.1:8081
