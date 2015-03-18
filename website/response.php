<?php
	include_once("config.php");
	
	if (!$conn) {
		echo "not connected";
	}
	
	echo "connected successfully\r\n";
	
	$results = $conn->query("SELECT title, time FROM blogs");

	while($row = $results->fetch_assoc()) {
		echo $row["title"] . "\r\n";
		echo $row["time"];
	}
	
	$conn->close();

?>