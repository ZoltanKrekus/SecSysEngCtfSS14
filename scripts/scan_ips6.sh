for num in {110..111} 
do 
	ip='fe80::5054:ff:'$(printf "%x" $num) 
	echo "${ip}" 
	ping6 -c 1 -t 1 $ip > /dev/null && echo "${ip} is up";
done

