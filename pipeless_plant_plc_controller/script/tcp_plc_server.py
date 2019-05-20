#!/usr/bin/env python

import socket
import rospy
from pipeless_plant_plc_controller.srv import *

data = bytearray(9)

def tcp_send(tx):        # function for sending provided data to the PLC
    plc = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    plc.connect(('192.168.0.130', 8000))  # fixed IP address of the PLC (update here if changed)
    plc.send(tx)
    plc.close()
    return

def colour_red_handler(color):  # function for Red Colour Valve data
    if color.time==0:
        data[1:7]=[3,0,0,1,0,0,0]
    else:
        data[1:7] = [3, 1, color.time, 0, 0, 0, 0]
    tcp_send(data)
    return colourResponse()

def colour_black_handler(color): # function for Black Colour Valve data
    if color.time==0:
        data[1:7]=[2,0,0,1,0,0,0]
    else:
        data[1:7] = [2, 1, color.time, 0, 0, 0, 0]
    tcp_send(data)
    return colourResponse()

def colour_yellow_handler(color):  # function for Yellow Colour Valve data
    if color.time==0:
        data[1:7]=[1,0,0,1,0,0,0]
    else:
        data[1:7] = [1, 1, color.time, 0, 0, 0, 0]
    tcp_send(data)
    return colourResponse()

def colour_blue_handler(color):  # function for Blue Colour Valve data
    if color.time==0:
        data[1:7]=[4,0,0,1,0,0,0]
    else:
        data[1:7] = [4, 1, color.time, 0, 0, 0, 0]
    tcp_send(data)
    return colourResponse()

def doser_handler(dose):  # function for Doser Unit data
    if dose.time==0:
        data[1:7]=[8,0,0,0,0,0,0]
    else:
        data[1:7] = [8, 1, dose.time, dose.speed*25, 0, 0, 0]
    tcp_send(data)
    return doserResponse()

def dsr_mix_ver_handler(pos): # function for Dosing & Mixing Station - Vertical Motor data
    if pos.position==0:
        data[1:7]=[9,0,0,1,0,0,0]
    else:
        data[1:7] = [9, 1, pos.position, 0, 0, 0, 0]
    tcp_send(data)
    return motorResponse()

def strg_ver_handler(pos): # function for Storage Station - Vertical Motor data
    if pos.position==0:
        data[1:7]=[5,0,0,1,0,0,0]
    else:
        data[1:7] = [5, 1, pos.position, 0, 0, 0, 0]
    tcp_send(data)
    return motorResponse()

def strg_hor_handler(pos): # function for Storage Station - Horizontal Motor data
    if pos.position==0:
        data[1:7]=[6,0,0,1,0,0,0]
    else:
        data[1:7] = [6, 1, pos.position, 0, 0, 0, 0]
    tcp_send(data)
    return motorResponse()

def magnet1_handler(magnet): # function for Dosing & Mixing Station - Magnets data
    data[1:7] = [10, 1, magnet.release, 0, 0, 0, 0]
    tcp_send(data)
    return magnetResponse()

def magnet2_handler(magnet): # function for Storage Station - Magnets data
    data[1:7] = [10, 2, magnet.release, 0, 0, 0, 0]
    tcp_send(data)
    return magnetResponse()

def tcp_plc_server():
    rospy.init_node('pipeless_plant_plc')
    col_r = rospy.Service( '~filling_station/red' , colour , colour_red_handler )
    col_bk = rospy.Service( '~filling_station/black' , colour, colour_black_handler )
    col_y = rospy.Service( '~filling_station/yellow', colour , colour_yellow_handler )
    col_bl = rospy.Service( '~filling_station/blue' , colour , colour_blue_handler )
    dos = rospy.Service('~doser', doser , doser_handler)
    dos_ver = rospy.Service('~doser_mixer_vertical', motor, dsr_mix_ver_handler)
    strg_ver = rospy.Service('~storage/vertical', motor, strg_ver_handler)
    strg_hor = rospy.Service('~storage/horizontal', motor, strg_hor_handler)
    mags1 = rospy.Service('~magnets/doser_mixer', magnet, magnet1_handler)
    mags2 = rospy.Service('~magnets/storage', magnet, magnet2_handler)
    rospy.loginfo("Pipeless Plant PLC Services Ready")
    rospy.spin()

if __name__=="__main__":
    tcp_plc_server()
