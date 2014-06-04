<form name="template" action="download.php" id="openTemplate" method="get">
	<label>
		<span>Filename</span>
<?php 
	if(!empty($_GET["f"]))
	{
		$filename = trim($_GET["f"]);
		echo '<input type="text" name="dlFilename" value="'.htmlentities($filename).'">';
	}
	else
		echo '<input type="text" name="dlFilename" value="">';
?>
	</label>
	<label>
		<span>Password</span>
		<input type="password" name="dlPassword">
	</label>
	<input type="submit" name="tempSubmit">
</form>

