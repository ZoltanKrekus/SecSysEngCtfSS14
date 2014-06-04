<?php
echo "<h2>add a holodeck access token</h2>";

if(empty($_POST['title'])) {
        output_html_form();
} else {
        $etitle =SQLite3::escapeString($_POST['title']);
        $ebemerkung = SQLite3::escapeString($_POST['bemerkung']);
        $esecret = SQLite3::escapeString($_POST['secret']);
        haskell_add($etitle,$ebemerkung,$esecret); 
        echo "stored";
	echo '<br /><a href="main.php" target="_self">back to main site</a>';
}
function haskell_add($titel,$bemerkung,$secret) {
        $fp = fopen(LOCKFILE,'w');

        $shell_titel = escapeshellcmd($titel);
        $shell_bemerkung = escapeshellcmd($bemerkung);
        $shell_secret = escapeshellcmd($secret);

        flock($fp,LOCK_EX);
        $output = shell_exec("./scripths.sh store \"$shell_titel\" \"$shell_bemerkung\"  \"$shell_secret\"");
        flock($fp,LOCK_UN);
}

function output_html_form() {
        echo '
<form method=POST action="?i=manage">
<fieldset>
 <legend>data</legend>
 <label>
 	holodeck_name:
 	<br>
 	<input type="text" name="title" >
 </label>
 <br>
 <label>
	 Key: Attention! "Key" is needed to retrieve the access token, store it in a safe place
	 <br>
	 <input type="text" name="bemerkung" id="bemerkung">
 </label>
 <br>
 <label>
	 access token
	 <br>
	 <input type="text" name="secret">
 </label>
 </label>
 <br>
 <button type="submit">add</button>
</fieldset>
</form>
<a href="main.php" target="_self">back to main site</a>';
}

?>
