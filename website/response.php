<?php
	include_once("../../mDev/config.php");
	
	if (!$conn) {
		echo "not connected";
	}
	
	$results = $conn->query("SELECT title, time FROM blogs");
	
	echo $results;
	
	$conn->close();

?>