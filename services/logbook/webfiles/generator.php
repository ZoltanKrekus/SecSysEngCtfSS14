<?php

$filename = uniqid("file-", true);

if(isset($_GET["f"]))
{
	$filename = trim($_GET["f"]);

	if(isset($_GET["pw"]))
	{
		$pw = trim($_GET["pw"]);
		$auth = 0;
		$passwordh = base64_encode($pw);
		$fileContent = file_get_contents("../sources/list");
		$lines = explode("\n", $fileContent);
		foreach ($lines as $line)
		{
			$elem = explode(",", $line);
			if(trim($elem[0]) == $filename && trim($elem[1]) == $passwordh)
			{
				$auth = 1;
				break;	
			}
		}
		fclose($fp);
		
		if ($auth <= 0) {
			die("<p>Invalid password. Aborting.</p>\n");
		}
	} else {
		echo "<a href=\"main.php?i=ov\">Overview</a><br>\n";
	}
	$latex = file_get_contents("../sources/$filename.tex");
} else {
	$filename = "FILE0".substr(md5(uniqid("filename", true)), 0, 10);
	$auth = 1;
}

if ($auth > 0) {
?>
<div id="generator">
	<form id="form_gen_latex" action="phplatex.php" method="get">
		<textarea id="editor" name="editor">
<?php
if (isset($latex)) {
echo $latex;
} else { echo '\documentclass{article}
\begin{document}

Fill me with content.

\end{document}
';
}
?>
</textarea>
		<fieldset>
			<label>
				<span>Filename</span>
				<input type="text" name="filename" id="filename" value="<?php echo $filename ?>">
			</label>
			<label>
				<span>Password</span>
				<input type="password" name="password" id="password" value="">
			</label>
			<label>
				<input type="submit" name="gen_latex" id="gen_latex" value="Generate">
			</label>					
		</fieldset>
	</form>
</div>
<?php
}
?>
