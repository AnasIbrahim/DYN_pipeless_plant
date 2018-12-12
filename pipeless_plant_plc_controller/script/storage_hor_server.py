#!/usr/bin/env python

import socket
import rospy
from plc_module.srv import *

tx = bytearray(64)

def stor_hor_handler(inp):
    tx[34:43]=[inp.StrgPos1,inp.StrgPos2,inp.StrgPos3,inp.StrgPosGap,
               inp.StrgPos4,inp.StrgPos5,inp.StrgPos6,inp.StrgHorStart,
               inp.StrgHorStop,inp.StrgHorRef]
    plc = socket.socket(socket.AF_INET,socket.SOCK_STREAM)
    plc.connect(('192.168.0.130',8000))
    plc.send(tx)
    plc.close()
    return storage_horResponse('done')

def storage_hor_server():
    rospy.init_node('storage_hor_server')
    c = rospy.Service('storage_horizontal',storage_hor,stor_hor_handler)
    print("Storage Horizontal Service Ready")
    rospy.spin()

if __name__=="__main__":
    storage_hor_server()
