#!/usr/bin/env python

import socket
import rospy
from pipeless_plant_plc_controller.srv import *

data = bytearray(9)

def tcp_send(tx):
    plc = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    plc.connect(('192.168.0.130', 8000))
    plc.send(tx)
    plc.close()
    return

def colour_red_handler(color):
    if color.time==0:
        data[1:7]=[4,0,0,1,0,0,0]
    else:
        data[1:7] = [4, 1, color.time, 0, 0, 0, 0]
    tcp_send(data)
    return colourResponse()

def colour_black_handler(color):
    if color.time==0:
        data[1:7]=[2,0,0,1,0,0,0]
    else:
        data[1:7] = [2, 1, color.time, 0, 0, 0, 0]
    tcp_send(data)
    return colourResponse()

def colour_yellow_handler(color):
    if color.time==0:
        data[1:7]=[1,0,0,1,0,0,0]
    else:
        data[1:7] = [1, 1, color.time, 0, 0, 0, 0]
    tcp_send(data)
    return colourResponse()

def colour_blue_handler(color):
    if color.time==0:
        data[1:7]=[3,0,0,1,0,0,0]
    else:
        data[1:7] = [3, 1, color.time, 0, 0, 0, 0]
    tcp_send(data)
    return colourResponse()

def doser_handler(dose):
    if dose.time==0:
        data[1:7]=[8,0,0,0,0,0,0]
    else:
        data[1:7] = [8, 1, dose.time, dose.speed, 0, 0, 0]
    tcp_send(data)
    return doserResponse()

def dsr_mix_ver_handler(pos):
    if pos.position==0:
        data[1:7]=[9,0,0,1,0,0,0]
    else:
        data[1:7] = [9, 1, pos.position, 0, 0, 0, 0]
    tcp_send(data)
    return motorResponse()

def strg_ver_handler(pos):
    if pos.position==0:
        data[1:7]=[5,0,0,1,0,0,0]
    else:
        data[1:7] = [5, 1, pos.position, 0, 0, 0, 0]
    tcp_send(data)
    return motorResponse()

def strg_hor_handler(pos):
    if pos.position==0:
        data[1:7]=[6,0,0,1,0,0,0]
    else:
        data[1:7] = [6, 1, pos.position, 0, 0, 0, 0]
    tcp_send(data)
    return motorResponse()

def tcp_plc_server():
    rospy.init_node('tcp_plc_server')
    col_r = rospy.Service( 'colour_station/red' , colour , colour_red_handler )
    col_bk = rospy.Service( '/colour_station/black' , colour, colour_black_handler )
    col_y = rospy.Service( '/colour_station/yellow', colour , colour_yellow_handler )
    col_bl = rospy.Service( '/colour_station/blue' , colour , colour_blue_handler )
    dos = rospy.Service('/doser', doser , doser_handler)
    dos_ver = rospy.Service('/doser_mixer_vertical', motor, dsr_mix_ver_handler)
    strg_ver = rospy.Service('/storage/vertical', motor, strg_ver_handler)
    strg_hor = rospy.Service('/storage/horizontal', motor, strg_hor_handler)
    print("TCP_PLC_Data Service Ready")
    rospy.spin()

if __name__=="__main__":
    tcp_plc_server()
