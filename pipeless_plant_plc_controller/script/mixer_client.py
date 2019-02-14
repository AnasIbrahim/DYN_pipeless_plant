#!/usr/bin/env python

import sys
import rospy

from pipeless_plant_plc_controller.srv import *

def mixer_client(op_id,mix_on,mix_time):
    rospy.wait_for_service('TCP_PLC_Data')
    try:
        tcp_srv = rospy.ServiceProxy('TCP_PLC_Data',tcp_plc)
        mixer1 = tcp_srv(op_id,mix_on,mix_time,0,0,0,0)
        return mixer1.status
    except rospy.ServiceException as e:
        print("Service call failed: %s" % e)

def usage():
    return "%s [Operation ID, Mixer_ON Mixer_Time]"%sys.argv[0]

if __name__ == "__main__":
    if len(sys.argv) == 4:
        op_id = int(sys.argv[1])
        mix_on = int(sys.argv[2])
        mix_time = int(sys.argv[3])
        mixer_client(op_id,mix_on,mix_time)
    elif len(sys.argv) == 1:
        mixer_client(0,0,0)
    else:
        print(usage())
        sys.exit(1)
