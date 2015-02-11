$(function(){
	var doc = document.querySelector('link[rel="import"]').import;
	var game = doc.querySelector('.content');
	document.querySelector('.includedContent').appendChild(game);
});