#!/usr/bin/env python

import socket
import rospy
from plc_module.srv import *

tx = bytearray(64)

def colours_handler(valves):
    tx[1:8]=[valves.v1_open,valves.v1_time,
           valves.v2_open,valves.v2_time,
           valves.v3_open,valves.v3_time,
           valves.v4_open,valves.v4_time]
    tx[17]=valves.close_all
    plc = socket.socket(socket.AF_INET,socket.SOCK_STREAM)
    plc.connect(('192.168.0.130',8000))
    plc.send(tx)
    plc.close()
    return coloursResponse('done')

def colours_server():
    rospy.init_node('colours_server')
    c = rospy.Service('colour_valves',colours,colours_handler)
    print("Colour Valves Service Ready")
    rospy.spin()

if __name__=="__main__":
    colours_server()
