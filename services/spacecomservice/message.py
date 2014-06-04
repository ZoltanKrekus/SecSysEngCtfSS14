import struct
import binascii
import os

PROTOCOLLAYOUT = '>16sBB16sh'

MSG_TEXT = 0
MSG_LOGIN = 1
MSG_STORE = 2
MSG_RETR = 3
MSG_SHIPS = 7
MSG_SERVERDOWN = 8
MSG_SERVERERROR = 9
MSG_LOGINSUCC = 10
MSG_INFO = 11
MSG_ECHO = 99


def protocolsize():
    return struct.calcsize(PROTOCOLLAYOUT)


class Message():
    mid = ''
    ttl = 1
    mtype = 0
    sid = ''
    contentlength = 0
    content = ''

    def __init__(self, message=None):
        if message != None:
            try:
                self.mid, self.ttl, self.mtype, self.sid, self.contentlength = struct.unpack(PROTOCOLLAYOUT, message[:36])
                self.content = message[struct.calcsize(PROTOCOLLAYOUT)+1:struct.calcsize(PROTOCOLLAYOUT)+1+self.contentlength]
            except:
                pass
        if not self.mid:
            self.mid = binascii.b2a_hex(os.urandom(8))
            ttl = 1

    def pack(self):
        return struct.pack(PROTOCOLLAYOUT, self.mid, self.ttl, self.mtype, self.sid, len(self.content.encode("ascii"))) + self.content.encode("ascii")

    def __repr__(self):
        return '[ MSG ID={}\tSENDER={}\t#TTL={}\t#TYPE={}\t{}]'.format(binascii.hexlify(self.mid),binascii.hexlify(self.sid), self.ttl, self.mtype, self.content)