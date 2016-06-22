function init() {
	var body = document.body;
	while (body.childNodes.length > 0) {
		body.removeChild(body.childNodes[0]);
	}
	var win = {
		width: document.documentElement.clientWidth - 32 
	}
	var info = document.getElementById("info");
	var elem = document.getElementById("elem");
	var text = document.getElementById("text");
	var send = document.getElementById("send");
	if (info === null) {
		info  =  document.createElement('div');
		info.id = 'info';
		body.appendChild(info);
	}
	info.style.width = win.width + 'px';
	info.style.margin = 'auto';
	info.style.height = '20px';
	
	if (elem === null) {
		elem  =  document.createElement('div');
		elem.id = 'elem';
		body.appendChild(elem);
	}
	elem.style.width = win.width + 'px';	
	elem.style.margin = 'auto';
	elem.style.height = '600px';
	if (text === null) {
		text  =  document.createElement('textarea');
		text.id = 'text';
		body.appendChild(text);
	}
	text.style.width = win.width + 'px';
	text.style.margin = 'auto';
	text.style.height = '60px';
	text.style.display = 'block';
	if (send === null) {
		send  =  document.createElement('button');
		send.id = 'send';
		body.appendChild(send);
	}
	send.style.width = win.width + 'px';
	send.style.margin = 'auto';
	send.style.height = '20px';
	send.style.display = 'block';
}
function initws()
{
	init();
	// адрес сервера
	var str = 'ws://12.0.0.1:8081';
	var info = document.getElementById("info");
    var elem = document.getElementById("elem");
	var _text = document.getElementById("text");
	var _send = document.getElementById("send");
    var wsock = new WebSocket(str);
        wsock.onopen = function () {
            info.innerText = "Соединен";
        }
        wsock.onclose = function (event) {
            info.innerText = "Отключен";
            setTimeout(function () {
                wsock = new WebSocket(str);
            }, 1000);
        }
	    wsock.onerror = function (error) { 
		    info.innerText = "Ошибка" + error.message;
	    }
        wsock.onmessage = function (event) {
		    if (typeof(event.data) === 'object') {
		    //Binnary
		    }
		    else {
			    var div  =  document.createElement('div');
			    div.style.backgroundColor = 'yellow';
			    div.innerText = event.data;
			    elem.appendChild(div);
		    }
	    };
	    _send.onclick = function () {
		    wsock.send(_text.value, user);
			           _text.value = '';
	}
}
