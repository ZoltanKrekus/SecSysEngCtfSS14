# submitting flags
for i in $(cat flags) ;
do python submit_flag.py $i;
done

