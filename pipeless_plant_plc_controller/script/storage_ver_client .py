#!/usr/bin/env python

import sys
import rospy

from plc_module.srv import *

def storage_ver_client(StrgPosDown,StrgPosUp,StrgPosCent,StrgVertStart,StrgVertStop,StrgVertRef,StrgVertRelPlate,StrgVertRelRobot):
    rospy.wait_for_service('storage_vertical')
    try:
        str_ver = rospy.ServiceProxy('storage_vertical',storage_ver)
        ver1 = str_ver(StrgPosDown,StrgPosUp,StrgPosCent,StrgVertStart,StrgVertStop,StrgVertRef,StrgVertRelPlate,StrgVertRelRobot)
        return ver1.status
    except rospy.ServiceException as e:
        print("Service call failed: %s" % e)

def usage():
    return "%s [StrgPosDown StrgPosUp StrgPosCent StrgVertStart StrgVertStop StrgVertRef StrgVertRelPlate StrgVertRelRobot]"%sys.argv[0]

if __name__ == "__main__":
    if len(sys.argv) == 9:
        StrgPosDown = bool(int(sys.argv[1]))
        StrgPosUp = bool(int(sys.argv[2]))
        StrgPosCent = bool(int(sys.argv[3]))
        StrgVertStart = bool(int(sys.argv[4]))
        StrgVertStop = bool(int(sys.argv[5]))
        StrgVertRef = bool(int(sys.argv[6]))
        StrgVertRelPlate = bool(int(sys.argv[7]))
        StrgVertRelRobot = bool(int(sys.argv[8]))
        storage_ver_client(StrgPosDown,StrgPosUp,StrgPosCent,StrgVertStart,StrgVertStop,StrgVertRef,StrgVertRelPlate,StrgVertRelRobot)
    elif len(sys.argv) == 1:
        storage_ver_client(0,0,0,0,0,0,0,0)
    else:
        print(usage())
        sys.exit(1)
