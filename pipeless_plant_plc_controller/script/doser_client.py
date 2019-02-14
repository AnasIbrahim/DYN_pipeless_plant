#!/usr/bin/env python

import sys
import rospy

from pipeless_plant_plc_controller.srv import *

def doser_client(op_id,doser_on,doser_time,doser_speed):
    rospy.wait_for_service('TCP_PLC_Data')
    try:
        tcp_srv = rospy.ServiceProxy('TCP_PLC_Data',tcp_plc)
        doser1 = tcp_srv(op_id,doser_on,doser_time,doser_speed,0,0,0)
        return doser1.status
    except rospy.ServiceException as e:
        print("Service call failed: %s" % e)

def usage():
    return "%s [Operation ID, Doser_ON Doser_Time Doser_Speed]"%sys.argv[0]

if __name__ == "__main__":
    if len(sys.argv) == 5:
        op_id = int(sys.argv[1])
        doser_on = int(sys.argv[2])
        doser_time = int(sys.argv[3])
        doser_speed = int(sys.argv[4])
        doser_client(op_id,doser_on,doser_time,doser_speed)
    elif len(sys.argv) == 1:
        doser_client(0,0,0,0)
    else:
        print(usage())
        sys.exit(1)
