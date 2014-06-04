#!/usr/bin/python
# -*- coding: utf-8 -*-
#
#   This client was coded by some insane space code monkeys
#   Feel free to fix some bugs, but there are no critical ones, better focus on the server code
#


import curses
import curses.textpad
import threading 
import socket
import sys
import os
import binascii
import message
import Queue
import signal
import argparse

ST = None
RT = None
DQUEUE = Queue.Queue()
INPUT = None
CLIENTID = binascii.b2a_hex(os.urandom(8))


class ReceiverThread(threading.Thread): 
    def __init__(self, socket, win):
        threading.Thread.__init__(self)
        self.sock = socket
        self.win = win
        self.stop_event = threading.Event()
 
    def run(self): 
        global CLIENTID
        cw.addnotification("starting rx components".upper())  
        while not self.stop_event.isSet():    
            try: 
                data = self.sock.recv(message.protocolsize())
                if not data:
                    continue
                m = message.Message(data)
                m.content = self.sock.recv(m.contentlength)

                if m.mtype == message.MSG_TEXT:
                    self.win.addmessage(m.content)
                elif m.mtype == message.MSG_SERVERDOWN:
                    self.win.addnotification(m.content)
                    self.stop()
                elif m.mtype == message.MSG_SERVERERROR:
                    self.win.adderror(m.content)
                    CLIENTID = md5.new(os.urandom(32)).digest()
                else:
                    self.win.adderror(m.content)
                self.win.win.refresh()
            except:
                self.win.adderror("error getting data")
                self.stop()

    def stop(self): 
        self.stop_event.set()


class SenderThread(threading.Thread): 
    def __init__(self, socket, queue):
        threading.Thread.__init__(self) 
        self.sock = socket
        self.stop_event = threading.Event()
        self.queue = queue
 
    def run(self):
        global CLIENTID
        cw.addnotification("starting tx components".upper())        
        while not self.stop_event.isSet():
            try:
                m = message.Message()
                qcont = self.queue.get().lstrip()
                if repr(qcont) == "end":
                    self.stop()
                    continue
                m.content = qcont
                m.sid = CLIENTID
            except:
                cw.adderror("error creating message")
                self.stop()
                break

            try:
                self.sock.sendall(m.pack())
            except:
                cw.adderror("error sending message")
                self.stop()

    def stop(self): 
        self.stop_event.set()


class CursesInputDialog(object):
    def __init__(self, screen, prompt):
        self.screen = screen
        self.prompt = prompt

    def show(self):
        (height, width) = self.screen.getmaxyx()

        dialogWidth = len(self.prompt) + 24
        inputWindow = self.screen.subwin(3, 40, height/2 - 1, width/2 - dialogWidth/2)
        inputWindow.border(0)
        inputWindow.addstr(1, 1, self.prompt)
        inputWindow.refresh()
        curses.echo()
        curses.nocbreak()
        input = inputWindow.getstr(1, len(self.prompt) + 1)
        curses.cbreak()
        curses.noecho()
        inputWindow.clear()
        inputWindow.refresh()

        return input


class CursesMenu(object):
    def __init__(self, screen):
        self.screen = screen

    def show(self):
        (height, width) = self.screen.getmaxyx()
        win = self.screen.subwin(3, 80,0, 0)
        win.clear()
        win.bkgd(curses.color_pair(2))
        win.box()
        win.addstr(1, 2, "F2:", curses.A_UNDERLINE)
        win.addstr(1, 6, "Info")
        win.addstr(1, 18, "F3:", curses.A_UNDERLINE)
        win.addstr(1, 22, "Save ComKey")
        win.addstr(1, 34, "F4:", curses.A_UNDERLINE)
        win.addstr(1, 38, "Retr ComKey")
        win.addstr(1, 68, "F8:", curses.A_UNDERLINE)
        win.addstr(1, 72, "Exit")


class CursesStatus(object):
    def __init__(self, screen):
        self.screen = screen

    def show(self):
        (height, width) = self.screen.getmaxyx()
        win = self.screen.subwin(3, 80,height-3, 0)
        win.clear()
        stdscr.bkgd(curses.color_pair(2))
        win.box()
        win.addstr(1, 2, " Server: {}:{}".format(HOST, PORT))
        win.addstr(1, 30, " User: {}".format(NICK))
        win.addstr(1, 52, " Status: {}".format("offline"), curses.color_pair(3))
        self.win = win


class CursesChat(object):
    def __init__(self, screen):
        self.screen = screen
        self.line = 0

    def show(self):
        (height, width) = self.screen.getmaxyx()
        self.win = self.screen.subwin(height-9, 80, 3, 0)
        self.win.scrollok(True)

    def addnotification(self, text):
        (height, width) = self.win.getmaxyx()
        self.win.scroll(1)
        self.win.addstr(height-1, 1, text, curses.color_pair(1))
        self.line +=1

    def addline(self, text):
        (height, width) = self.win.getmaxyx()
        self.win.scroll(1)
        self.win.addstr(height-1, 1, text, curses.color_pair(2))
        self.line +=1
        
    def adderror(self, text):
        (height, width) = self.win.getmaxyx()
        self.win.scroll(1)
        self.win.addstr(height-1, 1, text, curses.color_pair(3))
        self.line +=1
        
    def addmessage(self, text):
        (height, width) = self.win.getmaxyx()
        self.win.scroll(1)
        self.win.addstr(height-1, 1, text, curses.color_pair(4))
        self.line += 1


class CursesInput(object):
    def __init__(self, screen,prompt):
        self.screen = screen
        self.prompt = prompt
        (height, width) = self.screen.getmaxyx()
        self.win = self.screen.subwin(1, 80,height-5, 0)
        stdscr.bkgd(curses.color_pair(2))
        self.textbox = curses.textpad.Textbox(self.win, insert_mode=True)

    def show(self):
        self.win.erase()
        self.win.addstr(0, 1, " {}".format(self.prompt))

    def clear(self):
        self.win.clear()


def inputValidator(char):
    #if char == 27:
    if char == curses.KEY_F8:
        signal_handler(None, None)
        os.kill(os.getpid(), signal.SIGTERM)
        return 0
    elif char == curses.KEY_F3:
        secret = CursesInputDialog(stdscr, "Secret Comm Key: ").show()
        savesecret(socket, secret)
        return 1
    elif char == curses.KEY_F4:
        retrievesecret(socket)
        return 2
    elif char == curses.KEY_F2:
        requestinfo(socket)
        return 3
    elif char == curses.KEY_F5:
        secret = CursesInputDialog(stdscr, "<IP>:<PORT> ").show()
        addShipCommSystem(socket, secret)
    elif char == curses.KEY_ENTER or char == ord('\n'):
        return curses.ascii.BEL
    return char


def init_curses():
    stdscr = curses.initscr()
    curses.noecho()
    curses.cbreak()
    stdscr.keypad(1)
    curses.start_color()
    curses.init_pair(1, curses.COLOR_YELLOW, curses.COLOR_BLACK)
    curses.init_pair(2, curses.COLOR_BLUE, curses.COLOR_BLACK)
    curses.init_pair(3, curses.COLOR_RED, curses.COLOR_BLACK)
    curses.init_pair(4, curses.COLOR_GREEN, curses.COLOR_BLACK)
    stdscr.bkgd(curses.color_pair(2))
    stdscr.refresh()
    return stdscr


def close_curses(stdscr):
    curses.nocbreak()
    stdscr.keypad(0)
    curses.echo()
    curses.endwin()


def init_com():
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    try:
        # Connect to server and send data
        sock.connect((HOST, int(PORT)))
        ST = SenderThread(sock,DQUEUE)
        RT = ReceiverThread(sock,cw)
        ST.start()
        RT.start()
        cs.win.addstr(1, 48, " Status: {}".format("online "),curses.color_pair(4))
        return sock
    except Exception as ex:
        cw.adderror("connection error: {}".format(ex.message))
        close_com(socket)


def close_com(socket):
    try:
        DQUEUE.put('end')
        RT.stop()
    except:
        pass
    try:
        ST.stop()
    except:
        pass
    try:
        socket.close()
    except:
        pass


def login(socket, username, password):
    cw.addnotification("try to login user {}...".format(username))
    loginmsg = message.Message()
    loginmsg.mtype = message.MSG_LOGIN
    loginmsg.sid = CLIENTID
    loginmsg.content = "{}\n{}\n".format(username,password)
    try:
        socket.sendall(loginmsg.pack())
    except Exception as ex:
        cw.adderror("error sending login message: {}".format(ex.message))


def requestinfo(socket):
    cw.addnotification("try to get server info from  {}...".format(HOST))
    cw.win.refresh()
    msg = message.Message()
    msg.mtype = message.MSG_INFO
    msg.sid = CLIENTID
    try:
        socket.sendall(msg.pack())
    except Exception as ex:
        cw.adderror("error requesting server info: {}".format(ex.message))


def savesecret(socketfd, secret):
    cw.addnotification("try to save secret on  {}...".format(HOST))
    cw.win.refresh()
    msg = message.Message()
    msg.mtype = message.MSG_STORE
    msg.sid = CLIENTID
    msg.content = secret
    try:
        socketfd.sendall(msg.pack())
    except Exception as ex:
        cw.adderror("error save secret on  {}".format(ex.message))


def retrievesecret(socketfd):
    cw.addnotification("try to get secret from  {}...".format(HOST))
    cw.win.refresh()
    msg = message.Message()
    msg.mtype = message.MSG_RETR
    msg.sid = CLIENTID
    try:
        socketfd.sendall(msg.pack())
    except Exception as ex:
        cw.adderror("error  get secret from {}".format(ex.message))


def addShipCommSystem(socketfd, serverstring):
    cw.win.refresh()
    msg = message.Message()
    msg.mtype = message.MSG_SHIPS
    msg.sid = CLIENTID
    msg.content = serverstring
    try:
        socketfd.sendall(msg.pack())
    except Exception as ex:
        cw.adderror("error  get secret from {}".format(ex.message))

def signal_handler(signal, frame):
    close_curses(stdscr)
    print "\nshutting communication down"
    close_com(socket)
    sys.exit(0)


if __name__ == "__main__":

    parser = argparse.ArgumentParser(description='connect to the ships communication systems')
    parser = argparse.ArgumentParser(formatter_class=argparse.ArgumentDefaultsHelpFormatter)
    parser.add_argument('-s','--server', metavar='Host',nargs='?',default='localhost',type=str,help='server ip or hostname')
    parser.add_argument('-p','--port', metavar='Port',type=int,nargs='?',default=9999,help='serverport to use')
    parser.add_argument('-u','--user', metavar='Nick',type=str,nargs='?',default=None,help='username')
    args = parser.parse_args()

    HOST = args.server 
    PORT = args.port
    NICK = args.user

    stdscr = init_curses()

    (height, width) = stdscr.getmaxyx()

    if width < 80:
        close_curses(stdscr)
        print "terminalwindow too small, 80 char width min required!"
        sys.exit(1)
    if height < 20:
        close_curses(stdscr)
        print "terminalwindow too small, 20 char height min required!"
        sys.exit(1)

    if HOST is None:
        HOST = CursesInputDialog(stdscr, "SERVER: ").show()
    if PORT is None:
        PORT = CursesInputDialog(stdscr, "SERVER PORT: ").show()
    if NICK is None:
        NICK = CursesInputDialog(stdscr, "NICKNAME: ").show()
    PASS = CursesInputDialog(stdscr, "PASSWORD: ").show()

    CursesMenu(stdscr).show()
    cs = CursesStatus(stdscr)
    cs.show()
    cw = CursesChat(stdscr)
    cw.show()

    cw.addline("               INTERSPACE COM SYSTEM V4.3")
    cw.addline("                 ___")
    cw.addline("        ____..--'---`--..____")
    cw.addline("    =============================")
    cw.addline("      `---`--.._______..--'---'")
    cw.addline("        |       _|_|_       |")
    cw.addline("        `-----.'.---.`.-----'")
    cw.addline("              ||( O )||")
    cw.addline("               `-._.-'")
    cw.addline("                                 ________")
    cw.addline("                          ___---'--------`--..____")
    cw.addline("    ,-------------------.============================")
    cw.addline("    (__________________<|_)   `--.._______..--'")
    cw.addline("          |   |   ___,--' - _  /")
    cw.addline("       ,--'   `--'            |")
    cw.addline("       ~~~~~~~`-._            |")
    cw.addline("                  `-.______,-'")


    socket = init_com()
    login(socket, NICK, PASS)
    input = CursesInput(stdscr, '{} > '.format(NICK))
    stdscr.refresh()

    while True:
      
        input.show()
        stdscr.refresh()
        try:
            text = input.textbox.edit(inputValidator)
            if text == 0:
                break
            cw.addline(text[len('{} > '.format(NICK)):])
            cw.win.refresh()
            DQUEUE.put(text)
        
            stdscr.refresh()	
        except KeyboardInterrupt:
            break

    close_curses(stdscr)
    close_com(socket)
    try:
        ST.join()
        RT.join()
    except:
        pass 
    

