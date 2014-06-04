<?php

$errors = array();
$dir = "../log/";
$list = "../sources/list";

if (isset($_GET["filename"]) && !empty($_GET["filename"])) {
	$filename = trim($_GET["filename"]);
} else {
	die("Invalid or empty filename.");
}

if(!preg_match("/^[a-zA-z0-9]{3,50}$/", $filename))
{
	die("<p>Invalid filename (only letters and numbers allowed, min. 3 chars, max. 50 chars). Aborting.</p>\n");
}

if (isset($_GET["password"]) && !empty($_GET["password"])) {
	$password = trim($_GET["password"]);
	$passwordh = base64_encode($password);
} else {
	die("Invalid or empty password.");
}

if (isset($_GET["editor"]) && !empty($_GET["editor"])) {
	$latex = trim($_GET["editor"]);
} else {
	die("Invalid or empty LaTeX.");
}

// check if filename is already in password file
$already_stored = FALSE;
if(file_exists($list))
{
	$fileContent = file_get_contents($list);
	$lines = explode("\n", $fileContent);
	foreach ($lines as $line)
	{
	$elem = explode(",", $line);
		if(trim($elem[0]) == $filename)
		{
			if(trim($elem[1]) != $passwordh)
			{
				die("Filename already used and password does not match.");
			}
			$already_stored = TRUE;
		}
	}
	
	/*$fp = fopen($list, "r");
	if ($fp === false)
		die("File cannot be opened.");
	while (!feof($fp))
	{
		$line = fgets($fp);
		$elem = explode(",", $line);
		if(trim($elem[0]) == $filename)
		{
			if(trim($elem[1]) != $passwordh)
			{
				die("Filename already used and password does not match.");
			}
			$already_stored = TRUE;
		}
	}
	fclose($fp);*/
}

$pdflatex = "/usr/bin/pdflatex";
$pdflatex_flags = "-interaction=nonstopmode -output-directory=$dir -jobname=" . escapeshellcmd($filename);


$output = array();
$return_var = -1;

/* Create the output directory */
if (!file_exists($dir)) {
	if (!mkdir($dir, 750))	{
		echo "Cant mkdir\n";
		die();
	}
}

/* Write to tempfile for pdflatex */
/*
$tmp = tempnam(sys_get_temp_dir(), "phptmp-");
$handle = fopen($tmp, "w");
//echo "TMP: $tmp";
fwrite($handle, $latex);
fclose($handle);
*/
$filepath = dirname(__FILE__) . "/../sources/$filename.tex";
$fp = fopen($filepath, "w");
fwrite($fp, $latex);
fclose($fp);

echo "Processing LaTeX...<br>\n";
flush();

/* Executing pdflatex */
$shell = "$pdflatex $pdflatex_flags ".escapeshellarg($filepath);
//echo "Executing: " . $shell . "<br><br>\n\n";
exec($shell, $output, $return_var);

if ($return_var == 0)
{
	if(!$already_stored)
	{
		$fp = fopen("../sources/list", "a");
		fwrite($fp, "$filename,$passwordh\n");
		fclose($fp);
	}

	echo "Done.<br><br>\n";
//	echo "Done! Redirecting...<br>\n";
}
else
{
	echo "An error has occurred.<br><br>\n";
	echo "<div style=\"display: none;\">\n";
	print_r($output);
	echo "</div>\n";
	unlink($filepath);
}

/* Cleanup */
unset($output);
unset($errors);
//unlink($tmp);

/* Done; return to index.html and view file */

if ($return_var == 0) {
	header("Location: main.php?i=ov");
	echo "You can now use the funtion \"Download log-entry in PDF format\" to get your PDF!<br><br>";
	echo "<a href=\"main.php?i=ov\">go back to overview-page</a><br>\n";
} else {
	echo "<a href=\"main.php?i=ov\">go back to overview-page</a><br>\n";
//	echo "<a href=\"main.php?i=gen&f=$filename&pw=$password\">BACK</a><br>\n";
}
die();
