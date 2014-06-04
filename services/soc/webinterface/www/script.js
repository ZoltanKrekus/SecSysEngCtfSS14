function setIframes() {
	// alert("test");
	var iframes=document.getElementsByTagName('iframe');
	for (var i = 0; i < iframes.length;i++) {
		var port = iframes[i].getAttribute("data-port");
		var url = iframes[i].getAttribute("data-url");
		iframes[i].setAttribute("src","http://"+document.location.hostname+":"+port+"/"+url);
		iframes[i].setAttribute("style","width: 100%; height: 250;");
	}
}

window.onload = setIframes
