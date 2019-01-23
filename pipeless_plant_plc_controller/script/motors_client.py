#!/usr/bin/env python

import sys
import rospy

from pipeless_plant_plc_controller.srv import *

def motors_client(op_id,motor_on,motor_pos,motor_stop):
    rospy.wait_for_service('TCP_PLC_Data')
    try:
        tcp_srv = rospy.ServiceProxy('TCP_PLC_Data',tcp_plc)
        motors1 = tcp_srv(op_id,motor_on,motor_pos,motor_stop,0,0,0)
        return motors1.status
    except rospy.ServiceException as e:
        print("Service call failed: %s" % e)

def usage():
    return "%s [Operation ID, Motor_ON Vertical/Horizontal_Position Motor_Stop(Optional)]"%sys.argv[0]

if __name__ == "__main__":
    if len(sys.argv) == 5:
        op_id = int(sys.argv[1])
        motor_on = int(sys.argv[2])
        motor_pos = int(sys.argv[3])
        motor_stop = int(sys.argv[4])
        motors_client(op_id,motor_on,motor_pos,motor_stop)
    elif len(sys.argv) == 4:
        op_id = int(sys.argv[1])
        motor_on = int(sys.argv[2])
        motor_pos = int(sys.argv[3])
        motors_client(op_id,motor_on,motor_pos,0)
    elif len(sys.argv) == 1:
        motors_client(0,0,0,0)
    else:
        print(usage())
        sys.exit(1)
