<?php
echo "<h2>retrieve holodeck-access token</h2>";

if(empty($_POST["title"]) || empty($_POST["bemerkung"]) ) {
        output_html_form();
} else {
        $etitel = SQLite3::escapeString($_POST['title']);
        $ebemerkung = SQLite3::escapeString($_POST['bemerkung']);

        $sec = haskell_get($etitel,$ebemerkung);
        if($sec != null) {
                echo "the holodeck access token : $sec";
		echo '<br /><a href="main.php" target="_self">back to main site</a>';
        } else {
                echo "wrong key";
                echo "given deck $etitel  ; given secret $ebemerkung";
		echo '<br /><a href="main.php" target="_self">back to main site</a>';
        }
}

function output_html_form() {
        echo '
<form method=POST action="?i=query">
<fieldset>
 <legend>data</legend>
 <label for="textinput1">holodeck Name:</label><br>
 <input type="text" name="title" ><br>

 <label>access token</label><br>
 <input type="text" name="bemerkung"></label><br>

 <button type="submit" name="query">query</button>
</fieldset>
</form>
		<a href="main.php" target="_self">back to main site</a>
';


}

function haskell_get($titel,$bemerkung) {
        $fp = fopen(LOCKFILE,'w');

        $shell_titel = escapeshellcmd($titel);
        $shell_bemerkung = escapeshellcmd($bemerkung);

        flock($fp,LOCK_EX);
        $output = shell_exec("./scripths.sh retrieve \"$shell_titel\" \"$shell_bemerkung\" ");
        flock($fp,LOCK_UN);

        return $output;
}

?>

