#!/usr/bin/env python

import rospy
from std_srvs.srv import *
from move_base_msgs.msg import *
import actionlib
from geometry_msgs.msg import *

def mixing_station_handle(req):
    goal = Pose()
    goal.position.x = 0.51
    goal.position.y = 0.07
    goal.orientation.w = 1
    send_goal(goal)

def storage_station_handle(req):
    goal = Pose()
    goal.position.x = -0.44 
    goal.position.y = -0.02
    goal.orientation.z = 1
    send_goal(goal)

def black_yellow_filling_station_handle(req):
    goal = Pose()
    goal.position.x = 0.01
    goal.position.y = -0.8
    goal.orientation.z = -0.700
    goal.orientation.w = 0.713
    send_goal(goal)

def red_blue_filling_station_handle(req):
    goal = Pose()
    goal.position.x = 0.01 
    goal.position.y = 0.82
    goal.orientation.z = 0.74
    goal.orientation.w = 0.67
    send_goal(goal)

def send_goal(pose):
    rospy.loginfo("sending a goal")
    move_base_client = actionlib.SimpleActionClient('move_base', move_base_msgs.msg.MoveBaseAction)
    move_base_client.wait_for_server()
    move_base_client.cancel_all_goals()
    goal = MoveBaseGoal()
    goal.target_pose.header.frame_id = 'odom'
    goal.target_pose.pose = pose
    move_base_client.send_goal(goal)
    rospy.loginfo("Goal Sent")
    move_base_client.wait_for_result()

def main_program():
    rospy.init_node('move_to_station')
    mixing_station = rospy.Service('~mixing_station', Empty, mixing_station_handle)
    storage_station = rospy.Service('~storage_station', Empty, storage_station_handle)
    black_yellow_filling_station = rospy.Service('~black_yellow_filling_station', Empty, black_yellow_filling_station_handle)  
    red_blue_filling_station =rospy.Service('~red_blue_filling_station', Empty, red_blue_filling_station_handle)
    rospy.loginfo("moving stations advertised")
    rospy.spin()

if __name__ == '__main__':
    try:
        main_program()
    except rospy.ROSInterruptException: pass
