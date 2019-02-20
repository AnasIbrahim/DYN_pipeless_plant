#!/usr/bin/env python

import rospy
from std_srvs.srv import *
from move_base_msgs.msg import *
import actionlib
from geometry_msgs.msg import *
import tf
from nav_msgs.msg import *

global vel_pub, yaw_current

def mixing_station_handle(req):
    goal = Pose()
    goal.position.x = 0.72
    goal.position.y = 0.015
    goal.orientation.z = -0.004
    goal.orientation.w = 0.999
    send_goal(goal)
    return EmptyResponse()

def storage_station_handle(req):
    goal = Pose()
    goal.position.x = -0.685
    goal.position.y = 0.0
    goal.orientation.z = 0.999
    goal.orientation.w = 0.04
    send_goal(goal)
    return EmptyResponse()

def black_yellow_filling_station_handle(req):
    goal = Pose()
    goal.position.x = 0.02
    goal.position.y = -1.10
    goal.orientation.z = -0.698
    goal.orientation.w = 0.715
    send_goal(goal)
    return EmptyResponse()

def red_blue_filling_station_handle(req):
    goal = Pose()
    goal.position.x = -0.01 
    goal.position.y = 1.09
    goal.orientation.z = 0.715
    goal.orientation.w = 0.69
    send_goal(goal)
    return EmptyResponse()

def send_goal(pose):
    rospy.loginfo("sending a goal")
    move_base_client = actionlib.SimpleActionClient('move_base', move_base_msgs.msg.MoveBaseAction)
    move_base_client.wait_for_server()
    goal = MoveBaseGoal()
    goal.target_pose.header.frame_id = 'odom'
    goal.target_pose.pose = pose
    move_base_client.send_goal(goal)
    rospy.loginfo("Goal Sent")
    move_base_client.wait_for_result()
    reach_exact_yaw(pose)

def reach_exact_yaw(goal):
    global vel_pub, yaw_current
    rospy.loginfo("reaching exact angle")
    quaternion = (goal.orientation.x, goal.orientation.x, goal.orientation.z, goal.orientation.w)
    goal_angles = tf.transformations.euler_from_quaternion(quaternion)
    yaw_goal = goal_angles[2]
    rospy.loginfo("yaw_goal" + str(yaw_goal))
    twist_msg = Twist()
    yaw_speed = 0.1
    #rotate till angle reached
    tolerance = 0.005
    r = rospy.Rate(5)
    while(yaw_goal > yaw_current+tolerance or yaw_goal < yaw_current-tolerance):
        rospy.loginfo(str(yaw_current))
        if(yaw_goal > yaw_current):
            twist_msg.angular.z = yaw_speed
            vel_pub.publish(twist_msg)
            rospy.loginfo("+z direction")
        elif(yaw_goal < yaw_current):
            twist_msg.angular.z = -yaw_speed
            vel_pub.publish(twist_msg)
            rospy.loginfo("-z direction")
        r.sleep()
    twist_msg.angular.z = 0
    vel_pub.publish(twist_msg)

def odom_callback(data):
    global yaw_current
    orientation = data.pose.pose.orientation
    quaternion = (orientation.x, orientation.y, orientation.z, orientation.w)
    current_angles = tf.transformations.euler_from_quaternion(quaternion)
    yaw_current = current_angles[2]
    #rospy.loginfo(str(yaw_current))

def main_program():
    global vel_pub , yaw_current
    rospy.init_node('move_to_station')
    mixing_station = rospy.Service('~mixing_station', Empty, mixing_station_handle)
    storage_station = rospy.Service('~storage_station', Empty, storage_station_handle)
    black_yellow_filling_station = rospy.Service('~black_yellow_filling_station', Empty, black_yellow_filling_station_handle)  
    red_blue_filling_station =rospy.Service('~red_blue_filling_station', Empty, red_blue_filling_station_handle)
    rospy.loginfo("moving stations advertised")
    vel_pub = rospy.Publisher('cmd_vel', Twist, queue_size=1)
    odom_sub = rospy.Subscriber('odometry/filtered', Odometry, odom_callback,queue_size=1)

    rospy.sleep(1) #wait till yaw is read at least once - TODO: implement it in a cleaner way
    #test
    #goal = Pose()
    #goal.position.x = 0.72
    #goal.position.y = 0.015
    #goal.orientation.z = -0.004
    #goal.orientation.w = 0.999
    #reach_exact_yaw(goal)

    rospy.spin()

if __name__ == '__main__':
    try:
        main_program()
    except rospy.ROSInterruptException: pass
