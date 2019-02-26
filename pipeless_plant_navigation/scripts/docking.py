#!/usr/bin/env python

import rospy
from ca_msgs.msg import Bumper
from std_srvs.srv import *
from geometry_msgs.msg import Twist

global bumper_msg, vel_pub, twist_msg

def dock_handler(req):
    global bumper_msg, vel_pub, twist_msg
    twist_msg.linear.x = 0.1
    r = rospy.Rate(5)
    while( not(bumper_msg.is_left_pressed or bumper_msg.is_right_pressed) ):
        vel_pub.publish(twist_msg)
        r.sleep()
    return EmptyResponse()

def undocking_handler(req):
    global vel_pub, twist_msg
    twist_msg.linear.x = -0.1
    r = rospy.Rate(10)
    for x in range(0, 65):
        vel_pub.publish(twist_msg)
        r.sleep()
    return EmptyResponse()

def bumper_callback(data):
    global bumper_msg
    bumper_msg = data

def main_program():
    global bumper_msg, vel_pub, twist_msg
    twist_msg = Twist()
    rospy.init_node('docking')
    bumper_sub = rospy.Subscriber('bumper', Bumper, bumper_callback)
    vel_pub = rospy.Publisher('cmd_vel', Twist, queue_size=10)
    rospy.Service('~dock', Empty, dock_handler)
    rospy.Service('~undock', Empty, undocking_handler)
    rospy.spin()

if __name__ == '__main__':
    try:
        main_program()
    except rospy.ROSInterruptException : pass
