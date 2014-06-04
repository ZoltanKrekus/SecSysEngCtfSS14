<html>
<head>
	<link rel="stylesheet" type="text/css" href="style.css">
</head>	
<body>
<center>
	<img src="img/newplanet.jpg" id="newplanet" alt="" /></br></br>
<form action="output.php" method="get">

<p>Planetname:<br />
<input type="Text" name="planetname" ></p>

<input type="Submit" name="" value="Query">
</br></br>
<a href="index.php">Back to Start</a>
</center>
</form>

<?php
if ( $_GET['planetname'] <> "")
{
	echo '<center>';
	echo passthru("sudo -u starbase -- ./planetdb ".escapeshellarg($_GET['planetname']));
	echo '</br>';
	echo '</center>';
}
?>
</body>
</html>
