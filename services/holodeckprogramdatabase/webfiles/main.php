<?php

	require_once "functions.inc.php";
	require_once "config.inc.php";

	require_once "header.tmpl.php";

	get_site_inner();

	require_once $props["page"];

	require_once "footer.tmpl.php";
?>
