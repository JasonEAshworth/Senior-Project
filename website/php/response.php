<?php
	include_once("config.php");
	
	if (!$conn) {
		die;
	}
	
	$results = $conn->query("SELECT title, time, pictureReference, description FROM blogs");

	while($row = $results->fetch_assoc()) {
		echo '<div class="row" id="blog">';
		echo '<div class="col-lg-12">';
		echo '<hr>';
		echo '<h2 class="text-left">'.$row["title"].'</h2>';
		echo '<p><span class="glyphicon glyphicon-time"></span>';
		echo 'Posted on '.$row["time"].'</p>';
		echo '<hr>';
		echo '<center><img class="img-responsive" src="'.$row["pictureReference"].'" alt/></center>';
		echo '<hr>';
		echo '<p>'.$row["description"].'</p>';
		echo '<hr>';
		echo '</div>';
		echo '</div>';
	}
	
	$conn->close();

?>