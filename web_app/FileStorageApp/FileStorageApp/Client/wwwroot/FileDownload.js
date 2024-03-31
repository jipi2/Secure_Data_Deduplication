//window.downloadFile = function (base64Content, fileName, contentType) {
//    var byteCharacters = atob(base64Content);
//    var byteNumbers = new Array(byteCharacters.length);
//    for (var i = 0; i < byteCharacters.length; i++) {
//        byteNumbers[i] = byteCharacters.charCodeAt(i);
//    }
//    var byteArray = new Uint8Array(byteNumbers);
//    var blob = new Blob([byteArray], { type: contentType });

//    var link = document.createElement('a');
//    link.href = window.URL.createObjectURL(blob);
//    link.download = fileName;

//    document.body.appendChild(link);

//    link.click();

//    document.body.removeChild(link);
//}
