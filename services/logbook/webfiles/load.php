<form name="template" action="main.php?i=gen" id="openTemplate" method="get">
	<input type="hidden" name="i" value="gen">
	<label>
		<span>Filename</span>
<?php 
	if(!empty($_GET["f"]))
	{
		$filename = trim($_GET["f"]);
		echo '<input type="text" name="f" value="'.htmlentities($filename).'">';
	}
	else
		echo '<input type="text" name="f" value="">';
?>
	</label>
	<label>
		<span>Password</span> 
		<input type="password" name="pw">
	</label>
	<input type="submit" name="tempSubmit">
</form>

