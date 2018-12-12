#!/usr/bin/env python

import sys
import rospy

from plc_module.srv import *

def colours_client(v1,v1_time,v2,v2_time,v3,v3_time,v4,v4_time,close_all):
    rospy.wait_for_service('colour_valves')
    try:
        colour_vals = rospy.ServiceProxy('colour_valves',colours)
        colour1 = colour_vals(v1,v1_time,v2,v2_time,v3,v3_time,v4,v4_time,close_all)
        return colour1.status
    except rospy.ServiceException as e:
        print("Service call failed: %s" % e)

def usage():
    return "%s [V1_Open V1_Time V2_Open V2_Time V3_Open V3_Time V4_Open V4_Time Close_Valves]"%sys.argv[0]

if __name__ == "__main__":
    if len(sys.argv) == 10:
        v1 = bool(int(sys.argv[1]))
        v1_time = int(sys.argv[2])
        v2 = bool(int(sys.argv[3]))
        v2_time = int(sys.argv[4])
        v3 = bool(int(sys.argv[5]))
        v3_time = int(sys.argv[6])
        v4 = bool(int(sys.argv[7]))
        v4_time = int(sys.argv[8])
        close_all = bool(int(sys.argv[9]))
        colours_client(v1,v1_time,v2,v2_time,v3,v3_time,v4,v4_time,close_all)
    elif len(sys.argv) == 1:
        colours_client(0,0,0,0,0,0,0,0,0)
    else:
        print(usage())
        sys.exit(1)
