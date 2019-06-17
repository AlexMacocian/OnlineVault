function method_post(url, params, callback) {
	var xhttp = new XMLHttpRequest();
	xhttp.onreadystatechange = callback;
	xhttp.open("POST", url, true);
	xhttp.setRequestHeader('Content-type', 'application/x-www-form-urlencoded');
	xhttp.send(params);
}

function method_get(url, callback){
	var xhttp = new XMLHttpRequest();
	xhttp.onreadystatechange = callback;
	xhttp.open("GET", url, true);
	xhttp.send(null);
}

function logout(){
	method_get("/logout", function(){
		document.location.href = "index.html";
	});	
}

function goToFirstPage(){
	document.location.href = "index.html";
}