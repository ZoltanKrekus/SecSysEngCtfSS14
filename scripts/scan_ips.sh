for num in {110..140} 
do 
	ip='10.10.40.'$num
	echo "${ip}" 
	ping -c 1 -t 1 $ip > /dev/null && echo "${ip} is up";
done
