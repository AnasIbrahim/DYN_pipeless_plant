#!/usr/bin/env python
import rospy
from geometry_msgs.msg import Twist

class RobotPublisher:

    def __init__(self):
        # Starts a new node
        ##rospy.init_node('ca_driver', anonymous=True)
        self.velocity_publisher = rospy.Publisher('/cmd_vel', Twist, queue_size=10)
        self.vel_msg = Twist()
        self.linearSpeed = 0.1
        self.angularSpeed = 0.5


    def moveForward(self,robotIndex):
        self.vel_msg.linear.x = abs(self.linearSpeed)
        # Since we are moving just in x-axis
        self.vel_msg.linear.y = 0
        self.vel_msg.linear.z = 0
        self.vel_msg.angular.x = 0
        self.vel_msg.angular.y = 0
        self.vel_msg.angular.z = 0
        # Publish the velocity
        self.velocity_publisher.publish(self.vel_msg)

    def moveBackward(self,robotIndex):
        # Checking if the movement is forward or backwards
        self.vel_msg.linear.x = -abs(self.linearSpeed)
        # Since we are moving just in x-axis
        self.vel_msg.linear.y = 0
        self.vel_msg.linear.z = 0
        self.vel_msg.angular.x = 0
        self.vel_msg.angular.y = 0
        self.vel_msg.angular.z = 0
        # Publish the velocity
        self.velocity_publisher.publish(self.vel_msg)

    def rotateCW(self, robotIndex):
        self.vel_msg.linear.x = 0
        self.vel_msg.linear.y = 0
        self.vel_msg.linear.z = 0
        self.vel_msg.angular.x = 0
        self.vel_msg.angular.y = 0
        self.vel_msg.angular.z = -abs(self.angularSpeed)
        # Publish the velocity
        self.velocity_publisher.publish(self.vel_msg)

    def rotateCCW(self, robotIndex):
        self.vel_msg.linear.x = 0
        self.vel_msg.linear.y = 0
        self.vel_msg.linear.z = 0
        self.vel_msg.angular.x = 0
        self.vel_msg.angular.y = 0
        self.vel_msg.angular.z = abs(self.angularSpeed)
        # Publish the velocity
        self.velocity_publisher.publish(self.vel_msg)

    def dock(self,robotIndex):
        # Checking if the movement is forward or backwards
        distance = 0.4
        speed = 0.08
        self.vel_msg.linear.x = abs(speed)
        # Since we are moving just in x-axis
        self.vel_msg.linear.y = 0
        self.vel_msg.linear.z = 0
        self.vel_msg.angular.x = 0
        self.vel_msg.angular.y = 0
        self.vel_msg.angular.z = 0

        t0 = rospy.Time.now().to_sec()
        current_distance = 0

        while (current_distance < distance):
            # Publish the velocity
            self.velocity_publisher.publish(self.vel_msg)
            t1 = rospy.Time.now().to_sec()
            current_distance = speed * (t1 - t0)

        self.vel_msg.linear.x = 0
        self.velocity_publisher.publish(self.vel_msg)

    def undock(self,robotIndex):
        # Checking if the movement is forward or backwards
        distance =0.3
        speed =0.08
        self.vel_msg.linear.x = -abs(speed)
        # Since we are moving just in x-axis
        self.vel_msg.linear.y = 0
        self.vel_msg.linear.z = 0
        self.vel_msg.angular.x = 0
        self.vel_msg.angular.y = 0
        self.vel_msg.angular.z = 0

        t0 = rospy.Time.now().to_sec()
        current_distance=0

        while (current_distance < distance):
            # Publish the velocity
            self.velocity_publisher.publish(self.vel_msg)
            t1=rospy.Time.now().to_sec()
            current_distance = speed*(t1-t0)

        self.vel_msg.linear.x = 0
        self.velocity_publisher.publish(self.vel_msg)


    def stop(self,robotIndex):
        self.vel_msg.linear.x = 0
        self.vel_msg.linear.y = 0
        self.vel_msg.linear.z = 0
        self.vel_msg.angular.x = 0
        self.vel_msg.angular.y = 0
        self.vel_msg.angular.z = 0
        # Publish the velocity
        self.velocity_publisher.publish(self.vel_msg)


