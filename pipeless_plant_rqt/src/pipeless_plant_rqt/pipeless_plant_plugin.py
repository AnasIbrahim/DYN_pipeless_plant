import os
import rospy
import rospkg
import ctypes
import time
import Tkinter as tk
import  tkMessageBox as msgBox

from qt_gui.plugin import Plugin
from python_qt_binding import *
from python_qt_binding.QtCore import *
from python_qt_binding.QtWidgets import *

from std_msgs.msg import String
import roslib
import sys
import cv2
from sensor_msgs.msg import Image

from nav_msgs.msg import Odometry
from geometry_msgs.msg import *
from tf.msg import *
from cv_bridge import CvBridge, CvBridgeError
import numpy as np
from tf.transformations import euler_from_quaternion
from robot_publisher import RobotPublisher
from pipeless_plant_plc_controller.srv import *
from std_srvs.srv import *


#from pynput.keyboard import Key
#from pynput import keyboard



#from pynput.keyboard import Key


class PipelessPlantPlugin(Plugin):

    def __init__(self, context):
        super(PipelessPlantPlugin, self).__init__(context)
        # Give QObjects reasonable names
        self.setObjectName('PipelessPlantPlugin')

        # Process standalone plugin command-line arguments
        from argparse import ArgumentParser
        parser = ArgumentParser()
        # Add argument(s) to the parser.
        parser.add_argument("-q", "--quiet", action="store_true",
                      dest="quiet",
                      help="Put plugin in silent mode")
        args, unknowns = parser.parse_known_args(context.argv())
        if not args.quiet:
            print 'arguments: ', args
            print 'unknowns: ', unknowns

        # Create QWidget
        self._widget = QWidget()
        # Get path to UI file which should be in the "resource" folder of this package
        ui_file = os.path.join(rospkg.RosPack().get_path('pipeless_plant_rqt'), 'resource', 'pipeless_plant_plugin.ui')
        # Extend the widget with all attributes and children from UI file
        loadUi(ui_file, self._widget)
        # Give QObjects reasonable names
        self._widget.setObjectName('PipelessPlantPluginUi')

    ## Camera
        self.bridge = CvBridge()
        self.image_sub = rospy.Subscriber("/fiducial_images", Image, self.cameraCallback)

    ## Robots
        self._widget.moveForward_button.setAutoRepeat(True)
        self._widget.moveForward_button.setAutoRepeatInterval(40)
        self._widget.moveForward_button.pressed.connect(self._handle_moveForward_pressed)

        self._widget.moveBackward_button.setAutoRepeat(True)
        self._widget.moveBackward_button.setAutoRepeatInterval(40)
        self._widget.moveBackward_button.pressed.connect(self._handle_moveBackward_pressed)

        self._widget.rotateCW_button.setAutoRepeat(True)
        self._widget.rotateCW_button.setAutoRepeatInterval(40)
        self._widget.rotateCW_button.pressed.connect(self._handle_rotateCW_pressed)

        self._widget.rotateCCW_button.setAutoRepeat(True)
        self._widget.rotateCCW_button.setAutoRepeatInterval(40)
        self._widget.rotateCCW_button.pressed.connect(self._handle_rotateCCW_pressed)

        self.position_sub = rospy.Subscriber("/odometry/filtered", Odometry, self.positionCallback)

        self.robot= self._widget.robots_comboBox.currentText()
        self._widget.robots_comboBox.currentIndexChanged.connect(self._handle_robot_indexChanged)


    ## Filling
        self._widget.fillingRun_button.clicked[bool].connect(self._handle_fillingRun_clicked)
        self._widget.fillingStop_button.clicked[bool].connect(self._handle_fillingStop_clicked)
        validator = QtGui.QIntValidator(0, 9999, self)
        self._widget.fillingDuration_lineEdit.setValidator(validator)

    ## Dosing & Mixing
        self._widget.dosingRun_button.clicked[bool].connect(self._handle_dosingRun_clicked)
        self._widget.dosingStop_button.clicked[bool].connect(self._handle_dosingStop_clicked)
        self._widget.mixingVerticalGo_button.clicked[bool].connect(self._handle_mixingVerticalGo_clicked)
        self._widget.mixingVerticalStop_button.clicked[bool].connect(self._handle_mixingVerticalStop_clicked)
        self._widget.mixingMagnetOn_button.clicked[bool].connect(self._handle_mixingMagnetOn_clicked)
        self._widget.mixingMagnetOff_button.clicked[bool].connect(self._handle_mixingMagnetOff_clicked)

        self.doserSpeed=0
        self._widget.doserSpeed_slider.valueChanged[int].connect(self._handle_doserSpeed_valueChanged)

    ## Storage
        self._widget.storageHorizontalGo_button.clicked[bool].connect(self._handle_storageHorizontalGo_clicked)
        self._widget.storageVerticalGo_button.clicked[bool].connect(self._handle_storageVerticalGo_clicked)
        self._widget.storageEmergencyStop_button.clicked[bool].connect(self._handle_storageEmergencyStop_clicked)
        self._widget.storageMagnetOn_button.clicked[bool].connect(self._handle_storageMagnetOn_clicked)
        self._widget.storageMagnetOff_button.clicked[bool].connect(self._handle_storageMagnetOff_clicked)




    ## Automatic Mode
        self._widget.autoStart_button.clicked[bool].connect(self._handle_autoStart_clicked)
        self._widget.auto_label.setText(" This is a demo for automatic mode. \n When you click (Start), "+
                                        "The Robot will got to station 1 and the cup will" +
                                        " be filled with blue for 10 \n seconds, then it will go to station "+
                                        "2 and the cup will be filled with Yellow for 5 Seconds.\n " +
                                        "Then it will go to the mixing station and start mixing for 20 seconds.\n"+
                                        " Then The cup will be stored in position 1 in the Storage Station.")

        root = tk.Tk().withdraw()
        self.robot= RobotPublisher()

        if context.serial_number() > 1:
            self._widget.setWindowTitle(self._widget.windowTitle() + (' (%d)' % context.serial_number()))
        # Add widget to the user interface
        context.add_widget(self._widget)

    def shutdown_plugin(self):
        # TODO unregister all publishers here
        pass

    def save_settings(self, plugin_settings, instance_settings):
        # TODO save intrinsic configuration, usually using:
        # instance_settings.set_value(k, v)
        pass

    def restore_settings(self, plugin_settings, instance_settings):
        # TODO restore intrinsic configuration, usually using:
        # v = instance_settings.value(k)
        pass


## Camera Handlers
    def cameraCallback(self,data):

        try:
            cv_image = self.bridge.imgmsg_to_cv2(data, "bgr8")
        except CvBridgeError as e:
            self._widget.posYValue_label.setText(e)

        (rows, cols, channels) = cv_image.shape
        if cols > 60 and rows > 60:
            cv2.circle(cv_image, (50, 50), 10, 255)

        #cv2.imshow("Image window", cv_image)
        cv2.waitKey(3)

        #scene =QGraphicsScene()
        #scene.addPixmap(QtGui.QPixmap(QtGui.QImage(cv_image, cols, rows, 3 * cols, QtGui.QImage.Format_RGB888)))
        #sceneItem = QGraphicsPixmapItem(QtGui.QPixmap(QtGui.QImage(cv_image, cols,rows,3*cols,QtGui.QImage.Format_RGB888)))
        #scene.addItem(sceneItem)
        self._widget.cameraView_label.setPixmap(QtGui.QPixmap(QtGui.QImage(cv_image, cols,rows,3*cols,QtGui.QImage.Format_RGB888)))
        self._widget.cameraView_label.setScaledContents(True)
        #scene.setSceneRect(self._widget.camera_view.sceneRect())
        #self._widget.label_12.setPixmap(QtGui.QPixmap(QtGui.QImage(cv_image, cols,rows,3*cols,QtGui.QImage.Format_RGB888)))
#.scaled(741,421,Qt.KeepAspectRatio)

## calling a service: http://wiki.ros.org/ROS/Tutorials/WritingServiceClient%28python%29

## Robot Handlers
    def positionCallback(self, data):
        self._widget.posXValue_label.setText(str("%.2f" % data.pose.pose.position.x)+ ' m')
        self._widget.posYValue_label.setText(str("%.2f" % data.pose.pose.position.y)+ ' m')
        orientation_q = data.pose.pose.orientation
        orientation_list = [orientation_q.x, orientation_q.y, orientation_q.z, orientation_q.w]
        (roll, pitch, yaw) = euler_from_quaternion(orientation_list)
        theta = yaw * 180 * 7 /22
        self._widget.angleValue_label.setText(str("%.1f" % theta) )


    def _handle_moveForward_pressed(self):
        try:
            self.robot.moveForward(self.robot)
        except rospy.ServiceException, e:
            print "Move Forward failed: %s" % e


    def _handle_moveBackward_pressed(self):
        try:
            self.robot.moveBackward(self.robot)
        except rospy.ServiceException, e:
            print "Move Forward failed: %s" % e

    def _handle_rotateCW_pressed(self):
        try:
            self.robot.rotateCW(self.robot)
        except rospy.ServiceException, e:
            print "Move Forward failed: %s" % e

    def _handle_rotateCCW_pressed(self):
        try:
            self.robot.rotateCCW(self.robot)
        except rospy.ServiceException, e:
            print "Move Forward failed: %s" % e

    def _handle_robot_indexChanged(self):
        self.robot = self._widget.robots_comboBox.currentText()


## Filling Handlers
    def _handle_fillingRun_clicked(self):

        if (self._widget.fillingDuration_lineEdit.text()=='' or int(self._widget.fillingDuration_lineEdit.text())==0):
            msgBox.showwarning('Filling Station Error','Please enter valid duration')
        else:
            duration = int(self._widget.fillingDuration_lineEdit.text())
            #self._widget.fillingTimeRemaining_label.setText(self._widget.fillingDuration_lineEdit.text())
            colorServiceName =  '/pipeless_plant_plc/filling_station/' + self._widget.filling_comboBox.currentText().lower()
            rospy.wait_for_service(colorServiceName)
            try:
                callService = rospy.ServiceProxy(colorServiceName, colour)
                callService(duration)


            except rospy.ServiceException, e:
                print "Filling Run Service call failed: %s" % e

    def _handle_fillingStop_clicked(self):
        colorServiceName = '/pipeless_plant_plc/filling_station/' + self._widget.filling_comboBox.currentText().lower()
        rospy.wait_for_service(colorServiceName)
        try:
            callService = rospy.ServiceProxy(colorServiceName, colour)
            callService(0)

        except rospy.ServiceException, e:
            print "Filling Run Service call failed: %s" % e

    ## Dosing Handlers
    def _handle_dosingRun_clicked(self):
        if (self._widget.dosingDuration_lineEdit.text()=='' or int(self._widget.dosingDuration_lineEdit.text())==0):
            msgBox.showwarning('Dosing Station Error', 'Please enter valid duration')
        else:
            duration = int(self._widget.dosingDuration_lineEdit.text())
            #self._widget.fillingTimeRemaining_label.setText(self._widget.dosingDuration_lineEdit.text())
            serviceName = '/pipeless_plant_plc/doser'
            rospy.wait_for_service(serviceName)
            try:
                callService = rospy.ServiceProxy(serviceName, doser)
                callService(duration,self.doserSpeed)

            except rospy.ServiceException, e:
                print "Filling Run Service call failed: %s" % e


    def _handle_dosingStop_clicked(self):
        serviceName = '/pipeless_plant_plc/doser'
        rospy.wait_for_service(serviceName)
        try:
            callService = rospy.ServiceProxy(serviceName, doser)
            callService(0,self.doserSpeed)

        except rospy.ServiceException, e:
            print "Filling Run Service call failed: %s" % e

    def _handle_mixingVerticalGo_clicked(self):
        position = self._widget.mixingVertical_comboBox.currentText()
        if (position == 'Down'):
             positionIndex=1
        elif(position == 'Up'):
            positionIndex = 2
        else:
            positionIndex = 3

        serviceName= '/pipeless_plant_plc/doser_mixer_vertical'
        rospy.wait_for_service(serviceName)
        try:
            callService = rospy.ServiceProxy(serviceName, motor)
            callService(positionIndex)

        except rospy.ServiceException, e:
            print "Filling Run Service call failed: %s" % e

    def _handle_mixingVerticalStop_clicked(self):
        serviceName = '/pipeless_plant_plc/doser_mixer_vertical'
        rospy.wait_for_service(serviceName)
        try:
            callService = rospy.ServiceProxy(serviceName, motor)
            callService(0)

        except rospy.ServiceException, e:
            print "Filling Run Service call failed: %s" % e

    def _handle_mixingMagnetOn_clicked(self):
        serviceName = '/pipeless_plant_plc/magnets/doser_mixer'
        rospy.wait_for_service(serviceName)
        try:
            callService = rospy.ServiceProxy(serviceName, magnet)
            callService(0)
            self._widget.mixingMagnet_label.setStyleSheet("QLabel{background-color: green;}")
            self._widget.mixingMagnet_label.setText("ON")

        except rospy.ServiceException, e:
            print "Filling Run Service call failed: %s" % e


    def _handle_mixingMagnetOff_clicked(self):
        serviceName = '/pipeless_plant_plc/magnets/doser_mixer'
        rospy.wait_for_service(serviceName)
        try:
            callService = rospy.ServiceProxy(serviceName, magnet)
            callService(1)
            self._widget.mixingMagnet_label.setStyleSheet("QLabel{background-color: red;}")
            self._widget.mixingMagnet_label.setText("OFF")

        except rospy.ServiceException, e:
            print "Filling Run Service call failed: %s" % e

    def  _handle_doserSpeed_valueChanged(self, value):
        self.doserSpeed= value


## Storage Handlers
    def _handle_storageHorizontalGo_clicked(self):
        positionIndex = self._widget.storageHorizontal_comboBox.currentIndex()
        if (positionIndex == 7):
            positionIndex = 8
        elif (positionIndex == 0):
            positionIndex = 7

        serviceName = '/pipeless_plant_plc/storage/horizontal'
        rospy.wait_for_service(serviceName)
        try:
            callService = rospy.ServiceProxy(serviceName, motor)
            callService(positionIndex)

        except rospy.ServiceException, e:
            print "Filling Run Service call failed: %s" % e

    def _handle_storageVerticalGo_clicked(self):
        position = self._widget.storageVertical_comboBox.currentText()
        if (position == 'Down'):
            positionIndex = 1
        elif (position == 'Middle'):
            positionIndex = 2
        elif (position == 'Up'):
            positionIndex = 3
        else:
            positionIndex = 4
        serviceName = '/pipeless_plant_plc/storage/vertical'
        rospy.wait_for_service(serviceName)
        try:
            callService = rospy.ServiceProxy(serviceName, motor)
            callService(positionIndex)

        except rospy.ServiceException, e:
            print "Filling Run Service call failed: %s" % e

    def _handle_storageEmergencyStop_clicked(self):
        positionIndex = 0
        serviceName1 = '/pipeless_plant_plc/storage/vertical'
        serviceName2 = '/pipeless_plant_plc/storage/horizontal'
        rospy.wait_for_service(serviceName1)
        rospy.wait_for_service(serviceName2)
        try:
            callService1 = rospy.ServiceProxy(serviceName1, motor)
            callService1(positionIndex)
            callService2 = rospy.ServiceProxy(serviceName2, motor)
            callService2(positionIndex)

        except rospy.ServiceException, e:
            print "Filling Run Service call failed: %s" % e

    def _handle_storageMagnetOn_clicked(self):
        serviceName = '/pipeless_plant_plc/magnets/storage'
        rospy.wait_for_service(serviceName)
        try:
            callService = rospy.ServiceProxy(serviceName, magnet)
            callService(0)
            self._widget.storageMagnet_label.setStyleSheet("QLabel{background-color: green;}")
            self._widget.storageMagnet_label.setText("ON")

        except rospy.ServiceException, e:
            print "Filling Run Service call failed: %s" % e

    def _handle_storageMagnetOff_clicked(self):
        serviceName = '/pipeless_plant_plc/magnets/storage'
        rospy.wait_for_service(serviceName)
        try:
            callService = rospy.ServiceProxy(serviceName, magnet)
            callService(1)
            self._widget.storageMagnet_label.setStyleSheet("QLabel{background-color: red;}")
            self._widget.storageMagnet_label.setText("OFF")

        except rospy.ServiceException, e:
            print "Filling Run Service call failed: %s" % e

## Automatic Mode Handler
    def _handle_autoStart_clicked(self):
        rospy.wait_for_service('/pipeless_plant_plc/storage/vertical')
        rospy.wait_for_service('/pipeless_plant_plc/storage/horizontal')
        rospy.wait_for_service('/pipeless_plant_plc/magnets/storage')
        rospy.wait_for_service('/pipeless_plant_plc/magnets/doser_mixer')
        rospy.wait_for_service('/pipeless_plant_plc/doser_mixer_vertical')
        rospy.wait_for_service('/pipeless_plant_plc/filling_station/blue')
        rospy.wait_for_service('/pipeless_plant_plc/filling_station/yellow')
        rospy.wait_for_service('/move_to_station/black_yellow_filling_station')
        rospy.wait_for_service('/move_to_station/red_blue_filling_station')
        rospy.wait_for_service('/move_to_station/storage_station')
        rospy.wait_for_service('/move_to_station/mixing_station')
        try:
            storageVertical = rospy.ServiceProxy('/pipeless_plant_plc/storage/vertical',motor)
            storageHorizontal = rospy.ServiceProxy('/pipeless_plant_plc/storage/horizontal',motor)
            storageMagnet = rospy.ServiceProxy('/pipeless_plant_plc/magnets/storage',magnet)
            mixerMagnet = rospy.ServiceProxy('/pipeless_plant_plc/magnets/doser_mixer',magnet)
            mixerVertical = rospy.ServiceProxy('/pipeless_plant_plc/doser_mixer_vertical',motor)
            callBlue = rospy.ServiceProxy('/pipeless_plant_plc/filling_station/blue', colour)
            callYellow = rospy.ServiceProxy('/pipeless_plant_plc/filling_station/yellow', colour)
            goToYellow = rospy.ServiceProxy('/move_to_station/black_yellow_filling_station', Empty)
            goToBlue = rospy.ServiceProxy('/move_to_station/red_blue_filling_station', Empty)
            goToStorage = rospy.ServiceProxy('/move_to_station/storage_station', Empty)
            goToMixing = rospy.ServiceProxy('/move_to_station/mixing_station', Empty)


            #goToBlue()
            self.robot.dock(self.robot)
            time.sleep(4)
            callBlue(10)
            time.sleep(12)
            self.robot.undock(self.robot)
            time.sleep(3)
            #goToYellow()
            self.robot.dock(self.robot)
            time.sleep(4)
            callYellow(5)
            time.sleep(7)
            self.robot.undock(self.robot)
            time.sleep(7)
            mixerVertical(2)
            time.sleep(3)
            #goToMixing()
            self.robot.dock(self.robot)
            time.sleep(4)
            mixerVertical(1)
            time.sleep(15)
            mixerMagnet(0)
            time.sleep(7)
            mixerVertical(2)
            time.sleep(20)
            mixerVertical(1)
            time.sleep(20)
            mixerMagnet(1)
            time.sleep(5)
            mixerVertical(2)
            time.sleep(20)
            self.robot.undock(self.robot)
            storageHorizontal(7)
            time.sleep(10)
            storageVertical(3)
            time.sleep(8)
            #goToStorage()
            self.robot.dock(self.robot)
            time.sleep(4)
            storageVertical(1)
            time.sleep(20)
            storageMagnet(0)
            time.sleep(5)
            storageVertical(3)
            time.sleep(20)
            storageHorizontal(5)
            time.sleep(10)
            storageVertical(2)
            time.sleep(10)
            storageMagnet(1)
            time.sleep(3)
            storageMagnet(1)





        except rospy.ServiceException, e:
            print "Filling Run Service call failed: %s" % e


#    def on_press(self,key):
#        if (key == pynput.keyboard.Key.up):
#            self._handle_moveForward_pressed()
#        elif(key == pynput.keyboard.Key.down):
#            self._handle_moveBackward_pressed()
#        elif(key == pynput.keyboard.Key.right):
#            self._handle_rotateCW_pressed()
#        elif(key == pynput.keyboard.Key.left):
#            self._handle_rotateCCW_pressed()

#    # Collect events until released
#    with Listener(on_press=on_press) as listener:
#        listener.join()

