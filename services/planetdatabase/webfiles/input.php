<html>
<head>
	<link rel="stylesheet" type="text/css" href="style.css">
</head>	
<body>
<center>
<img src="img/askplanet.jpg" id="askplanet" alt="" /></br></br>
<form action="input.php" method="get">

<p>Planetname:<br />
<input type="Text" name="planetname" ></p>

<p>Astronomical Unit:<br />
<input type="Text" name="astronomical_unit" ></p>

<input type="Submit" name="" value="Save">
</br></br>
<a href="index.php">Back to Start</a>

</center>
</form>

<?php
if ( $_GET['planetname'] <> "" && $_GET['astronomical_unit'])
{
    $handle = fopen ( "./planets/".str_replace('..','',$_GET['planetname']), "a" );

	    fwrite ( $handle, str_replace('..','',$_GET['planetname']) );
	    fwrite ( $handle, "|" );
	    fwrite ( $handle, str_replace('..','',$_GET['astronomical_unit']) );
	    fclose ( $handle );
		echo "<center>";
    echo "Thanks .....for about 99.99999999999% the data was saved correctly! :-) </br>";
		echo "</center>";
    exit;
}
?>

</body>
</html>
