<?php

$dir = "../sources/";
$list = "../sources/list";
$files = array();

/* Check for directory */
if(!file_exists($dir))
	mkdir($dir, 750);	

/* Check for list */
if(!file_exists($list))
	die("Can't find file list. Aborting.");

$fp = fopen($list, "r");
if ($fp) {
	while (($buf = fgets($fp)) !== false) {
		$tmp = explode(",", $buf, -1);
		$files = array_merge($files, $tmp);
	}
} else {
	die("Can't read file. Aborting.");
}
fclose($fp);

echo "<table>\n";
echo "<thead>\n";
echo "<tr>\n";
echo "<th>Filename</th>\n";
echo "<th>PDF</th>\n";
echo "</tr>\n";
echo "</thead>\n";

echo "<tbody>\n";
foreach($files as $value) {
	echo "<tr>\n";
	echo "<td><a href=\"main.php?i=load&f=$value\">$value</a></td>\n";
	echo "<td><a href=\"main.php?i=dl&f=$value\">PDF</a></td>\n";
	echo "</tr>\n";
}
echo "</tbody>\n";
echo "</table>\n";

echo "<p>Click filename link to load source into editor.</p>\n";
echo "<p>Click PDF link to download PDF file.</p>\n";

?>

<!--form name="openTemplate" id="openTemplate" method="get">
	<input type="text" name="templateFilename">
	<input type="password" name="templatePassword">
	<input type="submit" name="templateSubmit">
</form-->
