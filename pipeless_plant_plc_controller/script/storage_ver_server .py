#!/usr/bin/env python

import socket
import rospy
from plc_module.srv import *

tx = bytearray(64)

def stor_ver_handler(inp):
    tx[28:33]=[inp.StrgPosDown,inp.StrgPosUp,inp.StrgPosCent,inp.StrgVertStart,
               inp.StrgVertStop,inp.StrgVertRef]
    tx[44:45]=[inp.StrgVertRelPlate,inp.StrgVertRelRobot]
    plc = socket.socket(socket.AF_INET,socket.SOCK_STREAM)
    plc.connect(('192.168.0.130',8000))
    plc.send(tx)
    plc.close()
    return storage_verResponse('done')

def storage_ver_server():
    rospy.init_node('storage_ver_server')
    c = rospy.Service('storage_vertical',storage_ver,stor_ver_handler)
    print("Storage Vertical Service Ready")
    rospy.spin()

if __name__=="__main__":
    storage_ver_server()
