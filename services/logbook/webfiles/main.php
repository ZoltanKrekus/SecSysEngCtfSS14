<!DOCTYPE html>
<html lang="en">
<head>
	<title>Logbook</title>
	<meta charset="utf-8">
	<meta name="viewport" content="width=device-width, initial-scale=1.0"> 
	
	<link rel="stylesheet" href="style.css">
	<link rel="stylesheet" href="logbook.css">  
</head>
<body>
	<header>
		<div class="row">
			<a class="header-logo" tabindex="-1" href="main.php">Logbook</a>
		</div>
	</header>
	<nav class="row">
		<ul>
			<li><a href="main.php?i=ov">List saved log-entries</a></li>
			<li><a href="main.php?i=gen">Add new log-entry</a></li>
			<li><a href="main.php?i=load">Load stored log-entry into editor</a></li>
			<li><a href="main.php?i=dl">Download log-entry in PDF format</a></li>
		</ul>
	</nav>
	<div id="content">
	<?php
	error_reporting(E_ALL);	

	if (isset($_GET["i"])) {
		switch ($_GET["i"]) {
		case "ov":
			echo "<h2>List saved log-entries</h2>";
			require("listing.php");
			break;
		case "gen":
			echo "<h2>Add new log-entry</h2>";
			require("generator.php");
			break;
		case "dl":
			echo "<h2>Download log-entry in PDF format</h2>";
			require("loadpdf.php");
			break;
		default:
			echo "<h2>Load stored log-entry into editor</h2>";
			require("load.php");
			break;
		}
	} else {
		echo "<h2>Functionality</h2>\n";
		echo "<p>This service allows you to generate and store log-entries.</p>\n";
		echo "<p>The core functionality allows you to store your own LaTeX file fully password protected. To view the stored files the correct password is required.</p>\n";
		echo "<p>Every successful compilation of the PDF document will store the LaTeX document for further reference.</p>\n";
		echo "<p>Like every space ship, storage space is limited, so be careful with your resources.</p>";
		echo "<p>Choose a function in the list above.</p>\n";
	}
	?>
	</div>
	<!--<footer>
		<div class="row">
			&copy; Captain Kirk
		</div>
	</footer>-->
</body>
