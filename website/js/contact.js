$(document).ready(function(){
	$('#contactInfo').submit(function() {
	    // get all the inputs into an array.
	    var $inputs = $('#contactInfo :input');

	    // not sure if you wanted this, but I thought I'd add it.
	    // get an associative array of just the values.
	    var values = {};
	    $inputs.each(function() {
	        values[this.id] = $(this).val();
	    });

	});
});

