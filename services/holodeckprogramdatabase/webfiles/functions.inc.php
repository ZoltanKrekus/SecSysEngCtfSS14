<?php

	$props = array("page" => "", "action" => "");
	$db = FALSE;

	function print_error($msg = "")
	{
		echo '<font color="red">huh, es trat ein Fehler auf!</font><br />';
		echo $msg;
	}

	function get_site_inner()
	{
		global $props;

		if(empty($_GET["i"]))
			$i = "front;none";
		else
			$i = $_GET["i"];

		if(!stristr($i, ";"))
			$i .= ";none";

		$tmp = explode(";", $i);
                $page = $tmp[0];
                if($page === "list" || $page === "query" || $page === "manage")
                {
			$props["page"] = $page.".include.php";
			$props["action"] = $tmp[1];
                }
		else
		{
			$props["page"] = "front.include.php";
			$props["action"] = "none";
		}
		
	}

?>
