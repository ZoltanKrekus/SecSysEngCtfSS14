#!/usr/bin/env python
import sys
import socket
import sys

CONST_GAME_SERVER_IP = "10.10.40.200"
CONST_GAME_SERVER_PORT = 80
CONST_GAME_SERVER_SUBMIT_URL = "/SubmitFlagServlet?teamInput=122&flagInput="
DEBUG_MODE = False

def sreadline(sock):
        line = sock.makefile().readline()
        logInfo("recv: " + line)
        return line

def swriteline(sock, line):
        logInfo("send: " + line)
        sock.send(line + "\n")

def logDebug(msg):
	if(DEBUG_MODE):	 
		print("%s\n" % msg)

def logInfo(msg):
        print("%s\n" % msg)

if __name__ == '__main__':
	
	logDebug("GameServer at: " + CONST_GAME_SERVER_IP + ":" + str(CONST_GAME_SERVER_PORT))

	sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
	sock.connect((CONST_GAME_SERVER_IP, CONST_GAME_SERVER_PORT))

	#flag = "130620131510524BZ4YY0S23WO6P1ZFE"
	flag = sys.argv[1]

	logDebug("submitting:" + sys.argv[1])
	
	submitUrl = "GET " + CONST_GAME_SERVER_SUBMIT_URL + flag + " HTTP/1.1\r\nHost: " + CONST_GAME_SERVER_IP + "\n"
	logDebug("write to:" + submitUrl)
        swriteline(sock, submitUrl)
	sock.close()
