<?php

if (!isset($_GET["dlPassword"]) || empty($_GET["dlPassword"])) {
	die("Password required.");
}

if (!isset($_GET["dlFilename"]) || empty($_GET["dlFilename"])) {
	die("Filename required.");
}

$pw = trim($_GET["dlPassword"]);
$passwordh = base64_encode($pw);
$filename = trim($_GET["dlFilename"]);

$found = 0;

$fileContent = file_get_contents("../sources/list");
$lines = explode("\n", $fileContent);
foreach ($lines as $line)
{
	$elem = explode(",", $line);
	if(trim($elem[0]) == $filename && trim($elem[1]) == $passwordh)
	{
		$found = 1;
		break;	
	}
}

/*
$fp = fopen("../sources/list", "r");
while (!feof($fp)) {
	$line = fgets($fp);
	$elem = explode(",", $line);
	if(trim($elem[0]) == $filename && trim($elem[1]) == $passwordh)
	{
		$found = 1;
		break;	
	}
}
fclose($fp);
*/
if ($found <= 0) {
	die("<p>Invalid password. Aborting.</p>\n");
}

$file = dirname(__FILE__) . "/../log/" . $_GET["dlFilename"] . ".pdf";

header("Pragma: public");
header('Content-disposition: attachment; filename='.$filename);
header("Content-type: ".mime_content_type($file));
header('Content-Transfer-Encoding: binary');
ob_clean();
flush();
readfile($file);
