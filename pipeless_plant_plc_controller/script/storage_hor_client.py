#!/usr/bin/env python

import sys
import rospy

from plc_module.srv import *

def storage_hor_client(StrgPos1,StrgPos2,StrgPos3,StrgPosGap,StrgPos4,StrgPos5,StrgPos6,StrgHorStart,StrgHorStop,StrgHorRef):
    rospy.wait_for_service('storage_horizontal')
    try:
        str_hor = rospy.ServiceProxy('storage_horizontal',storage_hor)
        hor1 = str_hor(StrgPos1,StrgPos2,StrgPos3,StrgPosGap,StrgPos4,StrgPos5,StrgPos6,StrgHorStart,StrgHorStop,StrgHorRef)
        return hor1.status
    except rospy.ServiceException as e:
        print("Service call failed: %s" % e)

def usage():
    return "%s [StrgPos1 StrgPos2 StrgPos3 StrgPosGap StrgPos4 StrgPos5 StrgPos6 StrgHorStart StrgHorStop StrgHorRef]"%sys.argv[0]

if __name__ == "__main__":
    if len(sys.argv) == 11:
        StrgPos1 = bool(int(sys.argv[1]))
        StrgPos2 = bool(int(sys.argv[2]))
        StrgPos3 = bool(int(sys.argv[3]))
        StrgPosGap = bool(int(sys.argv[4]))
        StrgPos4 = bool(int(sys.argv[5]))
        StrgPos5 = bool(int(sys.argv[6]))
        StrgPos6 = bool(int(sys.argv[7]))
        StrgHorStart = bool(int(sys.argv[8]))
        StrgHorStop = bool(int(sys.argv[9]))
        StrgHorRef = bool(int(sys.argv[10]))
        storage_hor_client(StrgPos1,StrgPos2,StrgPos3,StrgPosGap,StrgPos4,StrgPos5,StrgPos6,StrgHorStart,StrgHorStop,StrgHorRef)
    elif len(sys.argv) == 1:
        storage_hor_client(0,0,0,0,0,0,0,0,0,0)
    else:
        print(usage())
        sys.exit(1)
