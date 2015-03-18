"use strict"

$(document).ready(function(){
	jQuery.ajax({
		type: "POST",
		url: "../php/response.php",
		success:function(response){
			$("#posts").append(response);
		},
		error:function(xhr, ajaxOptions, thrownError){
			alert(thrownError);
		}
	});

});