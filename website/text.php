<?php
	$servername = "localhost";
	$username = "root";
	$password = "4801etgg";

	$conn = new mysqli($servername, $username, $password);

	if (!$conn) {
		die("connection failed: " . mysql_connect_error());
	}

	echo "connected successfully";

?>