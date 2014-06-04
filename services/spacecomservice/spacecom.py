#!/usr/bin/python
#
# there is more than on bug, can u find them?
#
import os
import sys
import socket
import time
import signal
import threading
import Queue
import SocketServer
import hashlib
import random
import logging
import binascii
import message
import argparse


HOST = ''
PORT = 9999
DQUEUE = Queue.Queue()
SQUEUE = Queue.Queue()
SESSIONS = {}
USERS = {'admin': '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918'}
SECRETS = {}
SERVER = None
SHIPS = []
CHATTER = None
SERVERTHREAD = None
QUEUEGETTIMEOUT = 4
LOGPATH = "/var/log/spacecomservice/spacecom.log"
ID = binascii.b2a_hex(os.urandom(8))


class ThreadedServer(SocketServer.ThreadingMixIn, SocketServer.TCPServer):
    daemon_threads = True       
    allow_reuse_address = True
    timeout = 2

    def __init__(self, server_address, RequesthandlerClass):
        print "starting socket server..."
        SocketServer.TCPServer.__init__(self, server_address, RequesthandlerClass)

        self.dist = Distributor()
        self.dist.start()
        self.sdist = InterShipDistributor()
        self.sdist.start()
        self.socket.settimeout(0.5)
        self.serving = True

    def stop(self):
        self.dist.stop()
        self.sdist.stop()
        self.shutdown()
        self.server_close()


class ClientWriter(threading.Thread):

    def __init__(self, queue, filetowrite, sessionID):
        super(ClientWriter, self).__init__()
        self.stop_event = threading.Event()
        self.queue = queue
        self.filetowrite = filetowrite
        self.id = sessionID

    def run(self):
        while not self.stop_event.isSet():
            try:
                m = self.queue.get()
                if m is None:
                    break
            except:
                logging.error("error getting message from queue")
                self.stop()
                break
            try: 
                pmessage = m.pack()
            except:
                logging.error("could not pack message: {0}".format(m))
                continue
            try:
                self.filetowrite.sendall(pmessage)
                logging.debug("{} bytes sended: {}".format(len(pmessage), binascii.hexlify(pmessage)))
                del pmessage
            except:
                logging.error("could not write message on session {0}, already closed".format(self.id))
                self.stop()
                break
            finally:
                del m

        logging.debug("Clientwriter Thread {0} shutdown".format(self.id))

    def stop(self):
        self.stop_event.set()


class ClientHandler(SocketServer.BaseRequestHandler):

    def handle(self):

        logging.debug("connection opened from {0}".format(self.client_address[0]))

        while True:
            try:
                data = ''
                while len(data) < message.protocolsize():
                    recvdata = self.request.recv(message.protocolsize()-len(data))
                    if not recvdata:
                        break
                    else:
                        data += recvdata

                logging.debug("{} bytes recived: {}".format(len(data), binascii.hexlify(data)))
                m = message.Message(data)
                del data
                if m.contentlength > 0:
                    while len(m.content) < m.contentlength:
                        contentdata = self.request.recv(m.contentlength - len(m.content))
                        if not contentdata:
                            break
                        else:
                            m.content = m.content + contentdata
                    logging.debug("content {} bytes recived: {}".format(len(m.content), m.content))
                logging.debug("message from client {}: {}".format(self.client_address[0], m))
                print m  #FOR DEBUGGING
            except:
                break

            if not m.sid:
                logging.debug("no sid given from client {0}".format(self.client_address[0]))
                break
            if not len(m.sid) == 16:
                logging.debug("wrong sid given from client {0}".format(self.client_address[0]))
                break

            self.sessionid = binascii.hexlify(m.sid)

            if not self.sessionid in SESSIONS.keys():  #NOT LOGGED IN YET

                self.initSession()
                answer = message.Message()
                answer.sid = ID

                if m.mtype == message.MSG_LOGIN: #login message
                    logging.debug("try to login client {}".format(self.sessionid))
                    c = m.content.split('\n')
                    try:
                        user = c[0][:32]
                        passwd = c[1][:32]
                    except:
                        logging.debug("fail to login client {}: Wrong format".format(self.sessionid))
                        answer.content = "Wrong format"
                        answer.mtype = message.MSG_SERVERERROR
                        self.queue.put(answer)
                        break

                    SESSIONS[self.sessionid] = (user, self.queue)

                    if self.loginUser(user, passwd):
                        logging.debug("login user {} successfull".format(user))
                        answer.content = "Login successfull"
                        answer.mtype = message.MSG_LOGINSUCC
                        self.queue.put(answer)
                    else:
                        logging.debug("fail to login client {}: Wrong password".format(self.sessionid))
                        answer.content = "Login failed, wrong password"
                        answer.mtype = message.MSG_SERVERERROR
                        self.queue.put(answer)
                        continue

                elif m.mtype == message.MSG_TEXT:
                    DQUEUE.put(m)
                    break

                else:
                    answer.content = "Not logged in yet"
                    answer.mtype = message.MSG_SERVERERROR
                    self.queue.put(answer)
                    break

            else:   # ALLREADY LOGGED IN
            
                if m.ttl > 3: #check ttl
                    m.ttl = 1
                
                if m.ttl == 0:
                    logging.debug("TTL 0, dropping message from client {}: {}".format(self.client_address[0], m))
                    continue
                else:
                    m.ttl -= 1

                if m.mtype == message.MSG_TEXT:
                    DQUEUE.put(m)   

                elif m.mtype == message.MSG_STORE:
                    self.storeSecret(SESSIONS[self.sessionid][0], m.content)
                    answer = message.Message()
                    answer.mtype = message.MSG_INFO
                    answer.content = "secret stored"
                    answer.sid = ID
                    self.queue.put(answer)
                    del answer
                    if len(SHIPS) > 0:
                        SQUEUE.put(m)

                elif m.mtype == message.MSG_RETR:
                    secret = self.retrieveSecret(SESSIONS[self.sessionid][0])
                    answer = message.Message()
                    answer.content = "{0}:{1}".format('SECRET', secret)
                    answer.mtype = message.MSG_INFO
                    answer.sid = ID
                    self.queue.put(answer)
                    del answer

                elif m.mtype == message.MSG_INFO:
                    answer = message.Message()
                    answer.mtype = message.MSG_INFO
                    answer.content = info()
                    answer.sid = ID
                    self.queue.put(answer)
                    del answer

                elif m.mtype == message.MSG_SHIPS:  # this is still not completly implemented
                    c = m.content.split(':')
                    try:
                        bserver = c[0][:32]
                        bport = int(c[1][:32])
                        SHIPS.append((bserver, bport))
                    except:
                        answer = message.Message()
                        answer.content = "error adding new Ship Comm System"
                        answer.mtype = message.MSG_SERVERERROR
                        answer.sid = ID
                        self.queue.put(answer)
                        del answer

                elif m.mtype == message.MSG_ECHO:
                    self.queue.put(m) #echo back message
                    del m

                else:
                    answer = message.Message()
                    answer.content = "unknown message type"
                    answer.mtype = message.MSG_SERVERERROR
                    answer.sid = ID
                    self.queue.put(answer)
                    del answer
            del m

    def finish(self):
        self.closeSession()

    def initSession(self):
        logging.debug("init session with id {}".format(self.sessionid))
        self.queue = Queue.Queue()
        self.worker = ClientWriter(self.queue, self.request, self.sessionid)
        self.worker.start()

    def closeSession(self):
        logging.debug("close session with id {}".format(self.sessionid))
        try:
            self.queue.put(None)        # poison pill
            self.queue = Queue.Queue()  # weired, but this is done so
            with self.queue.mutex:
                self.queue.queue.clear()
            self.queue = None
        except:
            logging.debug("error clear queue {}".format(self.sessionid))
        try:
            if self.sessionid in SESSIONS:
                logging.debug("clear session {}".format(self.sessionid))
                del SESSIONS[self.sessionid]
            if hasattr(self, 'worker'):
                self.worker.stop()
                self.worker = None
        except:
            logging.error("could not remove session " + str(self.sessionid))

    def registerUser(self, username, password):
        if username in USERS:
            return False
        USERS[username] = hashlib.sha256(password).hexdigest()
        return True

    def loginUser(self, username, password):
        if username not in USERS:
            self.registerUser(username, password)
        if USERS[username] == hashlib.sha256(password).hexdigest():
            return True
        else:
            return False

    def storeSecret(self, username, secret):
        logging.debug("user {0} stored a secret".format(username))
        SECRETS[username] = secret

    def retrieveSecret(self, username):
        if username in SECRETS.keys():
            return SECRETS[username]
        else:
            return 'no secret stored'


class Distributor(threading.Thread):

    def __init__(self):
        super(Distributor, self).__init__()
        self.stop_event = threading.Event()

    def stop(self):
        #debug
        logging.info("stopping Distributor")
        self.stop_event.set()
        DQUEUE.put(None)

    def run(self):
        logging.info("Distributor started")
        while not self.stop_event.isSet():
            
            try:
                #m = DQUEUE.get(true,QUEUEGETTIMEOUT) #block 2 seconds, too cpu intensive
                m = DQUEUE.get() #block
                if m is None:   #poison pill
                    break

                if len(SHIPS) > 0:
                    SQUEUE.put(m)
            except:
                continue                     #continue ( needed for stop check ) 

            tmpsessions = dict(SESSIONS)
            for sessionID in tmpsessions:
                logging.debug("try to send message {} to session {}".format(m.sid, sessionID))
                s = tmpsessions[sessionID][1]

                if not sessionID == binascii.hexlify(m.sid):  #dont send the message to the origin
                    try:
                        s.put(m)
                        #pass
                    except:
                        pass

            del tmpsessions
            del m
        logging.info("Distributor Thread stopped")


class InterShipDistributor(threading.Thread):
    """
    STILL EXPERIMENTAL! DO NOT USE IN REAL SPACE
    """

    def __init__(self):
        super(InterShipDistributor, self).__init__()
        self.stop_event = threading.Event()

    def stop(self):
        #debug
        logging.info("stopping InterShipDistributor")
        self.stop_event.set()
        SQUEUE.put(None)

    def run(self):
        logging.info("InterShipDistributor started")
        while not self.stop_event.isSet():

            try:
                m = SQUEUE.get()
                if m is None:   # poison pill
                    break
            except:
                logging.info("Error getting message form SQUEUE")
                continue

            if len(SHIPS) == 0:
                SQUEUE.queue.clear() # dont waste memory, if there are no other ships

            for (ip, port) in SHIPS:
                try:
                    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
                    sock.connect((ip, int(port)))
                    m.mtype = message.MSG_TEXT          # FIXME
                    sock.sendall(m.pack())
                    sock.close()

                except:
                    pass   # TODO logging
            #SQUEUE.task_done()
            del m
        logging.info("InterShipDistributor Thread stopped")


class Chatter(threading.Thread):

    bn = ["USS Enterprise (NCC-1701-E)", "Deathstar", "Millenium Falcon", "Warbird", "Bird of Prey", "Borg Cube", "KlingonWarrior23@Warbird", "B0t0nd"]
    messages = ["nope", "%n > %s prepare to be entered", "%n > You will be assimilated", "help us %s, we are under attack", "anyone found the bugs in the comm system yet?", "%s check the comm system!", "Bird of Prey > yaS vImojpu'", "%s Duj ghoStaH", "Bird of Prey > tlhonchaj chIljaj", "Bird of Prey > %s Ich fuehle mich geehrt, Sie zu sehen", "qaStaH nuq jay'", "%n > zzzasxzzzxzxxzzxxzzxzxz* Univeraltranslator failed ", "%n we want to buy some nukes or anti mater weapons, can someone help us?", "%s maybe its the session handling??", "%n > Energie!", "%n > Photonentorpedos auf mein Kommando!", "       ______\n      (_   __) .-\"\"\"\"-.\n        ) (___/        '.\n       (   ___     O    :\n       _) (__ \        .'\n      (______) '-....-'\n", "           __\n         /|\n        / \\\n        \  \\\n        }]::)==-{)\n        /  /\n        \ /\n         \|__\n", ]

    def __init__(self):
        threading.Thread.__init__(self)
        self.stop_event = threading.Event()

    def run(self):
        logging.info("Starting Chatter Thread")
        while not self.stop_event.isSet():
            if len(USERS) > 0:
                n = self.bn[random.randint(0, len(self.bn)-1)]
                messagetext = self.messages[random.randint(0, len(self.messages)-1)]
                messagetext = messagetext.replace("%s", USERS.keys()[random.randint(0, len(USERS)-1)])
                messagetext = messagetext.replace("%n", n)
                m = message.Message()
                m.content = messagetext.encode("ascii")
                m.sid = ID
                DQUEUE.put(m)
                del m
            sleeptime = random.randint(3, 20)
            x = 0
            while x < sleeptime:        # wake every sec up to check if thread should be shutdown
                time.sleep(1)
                x += 1
                if self.stop_event.isSet():
                    break
        logging.info("Chatter Thread stopped")

    def stop(self):
        logging.info("stopping Chatter Thread")
        self.stop_event.set()


def info():
    i = "Communicaion Server Info\n"
    i += "\t# of Users: {}\n".format(len(USERS))
    i += "\t# of Sessions: {}\n".format(len(SESSIONS))
    i += "\t# of saved secrets: {}\n".format(len(SECRETS))
    i += "User:\n\t"
    for u in USERS.keys():
        i += "{}, ".format(u)
    i += "\nShips:\n\t"
    for s in SHIPS:
        i += "{}, ".format(s)
    return i


def signal_handler(signal, frame):
    global DQUEUE
    logging.info("shutting communication down")
    print "shutting communication down"
    try:
        shutdownmsg = message.Message()
        shutdownmsg.content = "Communication server shutdown now ..."
        shutdownmsg.mtype = message.MSG_SERVERDOWN
        shutdownmsg.sid = ID
        DQUEUE.put(shutdownmsg)
        CHATTER.stop()
        SERVER.stop()
        DQUEUE = Queue.Queue()
        with DQUEUE.mutex:
            DQUEUE.queue.clear()
        DQUEUE.queue = None

        print "communication shutdown completed"
    except Exception as ex:
        logging.error(ex)
        logging.shutdown()
    logging.shutdown()
    sys.exit(0)


############ PROGRAM START ###########################

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description='ship communication system v4.1')
    parser = argparse.ArgumentParser(formatter_class=argparse.ArgumentDefaultsHelpFormatter)
    parser.add_argument('-p', '--port', metavar='Port', type=int, nargs='?', default=PORT, help='serverport to use')
    args = parser.parse_args()

    PORT = args.port

    LOGGER = logging.getLogger()
    try:
        hdlr = logging.FileHandler(LOGPATH)
        formatter = logging.Formatter('%(asctime)s %(levelname)s %(message)s')
        hdlr.setFormatter(formatter)
        LOGGER.addHandler(hdlr)
        LOGGER.setLevel(logging.INFO)
        #LOGGER.setLevel(logging.DEBUG)
        LOGGER.info("starting communication ...")
    except:
        print "error opening log file {0}".format(LOGPATH)

    signal.signal(signal.SIGINT, signal_handler)
    
    try:
        print "going to start threadserver"
        SERVER = ThreadedServer((HOST, PORT), ClientHandler)
        print "threadserver running"
    except Exception as e:
        print e
        logging.error(e)
        logging.shutdown()
        sys.exit(-1)

    SERVERTHREAD = threading.Thread(target=SERVER.serve_forever)
    SERVERTHREAD.daemon = True
    
    try:
        SERVERTHREAD.start()
    except Exception as e:
        logging.error(e)

    CHATTER = Chatter()
    CHATTER.start()

    signal.pause()

    del SERVER
    CHATTER.join()
    SERVERTHREAD.join()
