#!/usr/bin/env python

import sys
import rospy

from pipeless_plant_plc_controller.srv import *

def colours_client(op_id,v_open,v_time,v_close):
    rospy.wait_for_service('TCP_PLC_Data')
    try:
        tcp_srv = rospy.ServiceProxy('TCP_PLC_Data',tcp_plc)
        colour1 = tcp_srv(op_id,v_open,v_time,v_close,0,0,0)
        return colour1.status
    except rospy.ServiceException as e:
        print("Service call failed: %s" % e)

def usage():
    return "%s [Operation ID, Valve_Open Valve_Time Valve_Close(Optional)]"%sys.argv[0]

if __name__ == "__main__":
    if len(sys.argv) == 5:
        op_id = int(sys.argv[1])
        v_open = int(sys.argv[2])
        v_time = int(sys.argv[3])
        v_close = int(sys.argv[4])
        colours_client(op_id,v_open,v_time,v_close)
    elif len(sys.argv) == 4:
        op_id = int(sys.argv[1])
        v_open = int(sys.argv[2])
        v_time = int(sys.argv[3])
        colours_client(op_id,v_open,v_time,0)
    elif len(sys.argv) == 1:
        colours_client(0,0,0,0)
    else:
        print(usage())
        sys.exit(1)
