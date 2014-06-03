#!/bin/bash

echo "Pfad:"
pwd
echo ""
echo "I am"
whoami
echo ""
echo "User on the system:"
w
echo ""
echo "/etc/passwd"
cat /etc/passwd
echo ""
echo "uname -a"
uname -a
echo ""
echo "Ports running"
netstat -lnp
echo ""
echo "ps"
ps ax
echo ""
echo "lsof - which process is listening to port"
echo "do: lsof -i :80"
