# WORKSTATION_IP=192.168.40.117
WORKSTATION_IP=10.0.0.4
ssh root@WORKSTATION_IP tcpdump -U -s0 -w -n not dst port 22 and not src port 22 | wireshark -k -i - 
