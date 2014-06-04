#!/bin/bash

#cd /home/ksap
mkdir -p bin
mkdir -p log
cp resources/logback.xml bin/
javac -cp "src/:lib/*" -d bin/ src/at/ac/tuwien/inso/ss2014/ctf/MainServer.java
