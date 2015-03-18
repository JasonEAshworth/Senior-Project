<?php
	include_once("config.php");
	
	if (!$conn) {
		echo "not connected";
	}
	
	echo "connected successfully";
	
	$results = $conn->query("SELECT title, time FROM blogs");

	while($row = $results->fetch_assoc()) {
		echo $row["title"];

	}
	
	$conn->close();

?>