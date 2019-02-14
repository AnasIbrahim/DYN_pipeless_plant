#!/usr/bin/env python

import socket
import rospy
from pipeless_plant_plc_controller.srv import *

tx = bytearray(9)

def tcp_plc_handler(bytes):
    tx[1:7]=[bytes.b1,bytes.b2,bytes.b3,bytes.b4,bytes.b5,bytes.b6,bytes.b7]
    plc = socket.socket(socket.AF_INET,socket.SOCK_STREAM)
    plc.connect(('192.168.0.130',8000))
    plc.send(tx)
    plc.close()
    return tcp_plcResponse('Done')

def tcp_plc_server():
    rospy.init_node('tcp_plc_server')
    c = rospy.Service('TCP_PLC_Data',tcp_plc,tcp_plc_handler)
    print("TCP_PLC_Data Service Ready")
    rospy.spin()

if __name__=="__main__":
    tcp_plc_server()
