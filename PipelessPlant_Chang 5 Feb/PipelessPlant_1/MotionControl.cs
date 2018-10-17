using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Threading;
using System.IO;
using System.Timers;
using MULTIFORM_PCS.ControlModules.CameraModule.Algorithm;

namespace MULTIFORM_PCS
{
    public class MotionControl
    {
        MULTIFORM_PCS.Gateway.ConnectionModule.iRobot.iRobotRouter.Robot robot;
        //robot position data from the camera
        double[] cameraPosition;
        //data from robot odometer
        double odometerDistance;
        double odometerAangle;
        //goal point needed to perform a specific product
        double[] goalPosition;
        //tuning parameters for the motion control law
        const double k_rho = 0.6;
        const double k_alpha = 1.2;
        const double k_beta = -0.3;
        //integral and derivative coefficients
        const double k_rho_i = 0.01;
        const double k_rho_d = 0.05;
        double integral = 0;
        double derivative = 0;
        //initial values for 
        double pre_rho = 0;
        double distance=0;
        double angle;
        double theta;
        double vel = 0;
        double omega = 0;
        double v_l = 0;
        double v_r = 0;
        double[] p = new double[3] { 0, 0, 0 };
        double[] delta_p = new double[3] { 0, 0, 0 };
        double alpha = 0;
        double beta = 0;
        double rho = 0;
        double Time0 = 0;
        double distanceAcc=0;
        double inverse_tan;
        //double[] currentPositon = new double[3];
        double[] robotVelocity = new double[2] { 0, 0 }; //the initial values of the left and right wheels velocities
        double[] odometerPosition = new double[3];// { 0, 0, (Math.PI / 180) * 0 };//{3000,1500,(Math.PI/180)*180} ;//initial assumed position
        double[] deltaPosition = new double[3] { 0, 0, 0 };
        double[] estimatedPosition = new double[3] { 0, 0, 0 };
        double[] oldGoal = new double[3];
        double[] deltaCameraposition = new double[3] { 0, 0, 0 };
        double[] oldCamera = new double[3];
        double[] correctedCamera = new double[3];
        bool finished = false;

        StopWatch S0 = new StopWatch(); //with the new stopwatch class not the embedded one in the system

        bool initialCameraPosTaken = false;//bool variable to assign initial postion from camera at the begining only 

        bool check1 = true;

        //class constructor 
        public MotionControl(MULTIFORM_PCS.Gateway.ConnectionModule.iRobot.iRobotRouter.Robot robot)
        {
            this.robot = robot;  

            var cameraPollTimer = new DispatcherTimer();
            cameraPollTimer.Interval = TimeSpan.FromMilliseconds(200);//every 100 millisencod this code will be executed               
            cameraPollTimer.Tick += (s, o) =>
            {
                
                if (!initialCameraPosTaken)
                {
                    takeInitialCameraPosition();
                }
                else
                {
                    if (S0.update())
                    {
                        var curPos = updateCurrentCameraPos();
                        if (curPos != null)
                        {
                            var kalPos = kalmanFilter(curPos, odometerPosition);
                            var vel = calculateNextVelocities(kalPos);
                            updateRobotVelocity(vel);
                        }
                    }
                }
                 
            };
            robot.connection.eosReached += () => {
                cameraPollTimer.Stop();
            };
            robot.connection.error += (e) =>
            {
                cameraPollTimer.Stop();
            };
//            cameraPollTimer.Start();

            var updateTimer = new DispatcherTimer();
            updateTimer.Interval = TimeSpan.FromMilliseconds(10);
            updateTimer.Tick += (s, o) =>
            {
                if (initialCameraPosTaken)
                {
                    if (S0.update())
                    {
                        S0.GetElapsedTimeSecs();
                        var curPos = odometer();
                        if (curPos != null)
                        {
                            var kalPos = kalmanFilter(cameraPosition, curPos);
                            var vel = calculateNextVelocities(kalPos);
                            updateRobotVelocity(vel);
                        }
                    }
                }
                else
                {
                    takeInitialCameraPosition();
                    odometerPosition = cameraPosition;
                }
            };
            updateTimer.Start();

            // code to read the distance from the odometry
            robot.protocol.debug += (msg, i) =>
            {
                if (!finished && msg == "distance")
                {
                    distance = distance + (double)i;
                    global_stuff.logcamera.WriteLine("i:" + i + " distance:" + distance);
                }
            };
        }

        public void updateGoal(double[] goalPoint) 
        {
            goalPosition = goalPoint;
        }


        private void takeInitialCameraPosition()
        {
             Datastructure.Model.Plant p = Gateway.ObserverModule.getInstance().getCurrentPlant();

             if (p.AllAGVs == null)
                 return;
             for (int j = 0; j < p.AllAGVs.Count; j++)
             {
                 if (p.AllAGVs[j] != null && p.AllAGVs[j].Id == robot.RobotID)
                 {
                     cameraPosition = new double[] { 10*p.AllAGVs[j].theCurPosition.X,
                                          10*p.AllAGVs[j].theCurPosition.Y,
                                          (Math.PI/180)*p.AllAGVs[j].theRotation };
                    global_stuff.logcamera.WriteLine(" camera x: " + cameraPosition[0] + " camera y: " + cameraPosition[1] + " camera theta: " + cameraPosition[2]);

                    oldGoal = goalPosition;
                    oldCamera = cameraPosition;

                    if (cameraPosition[0] > goalPosition[0])
                    {
                        goalPosition[0] = cameraPosition[0] - goalPosition[0];
                        goalPosition[1] = cameraPosition[1] - goalPosition[1];
                        cameraPosition[0] = 0;
                        cameraPosition[1] = 0;
                        cameraPosition[2] = cameraPosition[2] - Math.PI;
                        goalPosition[2] = goalPosition[2] - Math.PI;
                    }
                    else if (cameraPosition[0] < goalPosition[0])
                    {
                        goalPosition[0] = goalPosition[0] - cameraPosition[0];
                        goalPosition[1] = goalPosition[1] - cameraPosition[1];
                        cameraPosition[0] = 0;
                        cameraPosition[1] = 0;
                    }
                    if (goalPosition[2] > Math.PI)
                        goalPosition[2] = goalPosition[2] - 2 * Math.PI;
                    else if (goalPosition[2] < -Math.PI)
                        goalPosition[2] = 2 * Math.PI + goalPosition[2];

                    if (cameraPosition[2] > Math.PI)
                        cameraPosition[2] = cameraPosition[2] - 2 * Math.PI;
                    else if (odometerPosition[2] < -Math.PI)
                        cameraPosition[2] = 2 * Math.PI + cameraPosition[2];


                    initialCameraPosTaken = true;
                }
             }
             return;
        }


        //method to calculate the robot location(x,y,theta) using odometer data
        private double[] odometer()
        {
            global_stuff.logcamera.WriteLine(" dist:" + distance + "  distanceAcc: " + distanceAcc);
            odometerDistance = distance - distanceAcc;//contains the incremented distance from the robor odometer
            //first calculate deltaTheta by knowing the left and right wheels speeds and the time interval
            omega = (robotVelocity[0] - robotVelocity[1]) / 263; //in rad/sec
            deltaPosition[2] = omega * S0.GetElapsedTimeSecs();  //in radians
            //calculating deltaX and deltaY
            deltaPosition[0] = odometerDistance * Math.Cos(odometerPosition[2] + deltaPosition[2] / 2);//delta x in millimeter
            deltaPosition[1] = odometerDistance * Math.Sin(odometerPosition[2] + deltaPosition[2] / 2);//delta y in millimeter
            //update the new position from the Motion model
            odometerPosition[0] = odometerPosition[0] + deltaPosition[0];
            odometerPosition[1] = odometerPosition[1] + deltaPosition[1];
            odometerPosition[2] = odometerPosition[2] + deltaPosition[2]; // in radian

            distanceAcc = distance;

            global_stuff.logcamera.WriteLine(" X_odo:" + odometerPosition[0] + " Y_odo: " + odometerPosition[1] + " theta_odo: " + odometerPosition[2] + " time: " + S0.GetElapsedTimeSecs());

            return odometerPosition;// mm,mm,radinas
        }
        // camera
        private double[] updateCurrentCameraPos()
        {
            Datastructure.Model.Plant p = Gateway.ObserverModule.getInstance().getCurrentPlant();

            if (p.AllAGVs == null)
                return null;
            for (int j = 0; j < p.AllAGVs.Count; j++)
            {
                if (p.AllAGVs[j] != null && p.AllAGVs[j].Id == robot.RobotID)
                {
                    correctedCamera = cameraPosition; // in robot space

                    cameraPosition = new double[] { 10*p.AllAGVs[j].theCurPosition.X,
                                          10*p.AllAGVs[j].theCurPosition.Y,
                                          (Math.PI/180)*p.AllAGVs[j].theRotation };
                  
                    global_stuff.logcamera.WriteLine(" camera before X:" + cameraPosition[0] + " camera before Y:" + cameraPosition[1] + " camera before theta:" + (180 / Math.PI) * cameraPosition[2] + "\r\n");

                    

                    global_stuff.logcamera.WriteLine(" init camera after X:" + cameraPosition[0] + " init camera after Y:" + cameraPosition[1] + " init camera after theta:" + (180 / Math.PI) * cameraPosition[2] + "\r\n");

                    if (global_stuff.timeStamp[j] != p.AllAGVs[j].LastUpdateCam)
                    {
                        if (cameraPosition[0] > oldGoal[0])
                        {
                            // goalPosition[0] = cameraPosition[0] - goalPosition[0];
                            //goalPosition[1] = cameraPosition[1] - goalPosition[1];
                            deltaCameraposition[0] = oldCamera[0] - cameraPosition[0];
                            deltaCameraposition[1] = oldCamera[1] - cameraPosition[1];
                            deltaCameraposition[2] = cameraPosition[2] - oldCamera[2];
                        }
                        else if (cameraPosition[0] < oldGoal[0])
                        {
                            deltaCameraposition[0] = cameraPosition[0] - oldCamera[0];
                            deltaCameraposition[1] = cameraPosition[1] - oldCamera[1];
                            deltaCameraposition[2] = oldCamera[2] - cameraPosition[2];
                        }



                        correctedCamera[0] = correctedCamera[0] + deltaCameraposition[0];
                        correctedCamera[1] = correctedCamera[1] + deltaCameraposition[1];
                        correctedCamera[2] = correctedCamera[2] + deltaCameraposition[2];

                        if (cameraPosition[2] > Math.PI)
                            cameraPosition[2] = cameraPosition[2] - 2 * Math.PI;
                        else if (cameraPosition[2] < -Math.PI)
                            cameraPosition[2] = 2 * Math.PI + cameraPosition[2];

                        oldCamera = cameraPosition;
                        cameraPosition = correctedCamera;
                        global_stuff.logcamera.WriteLine(" camera after X:" + cameraPosition[0] + " camera after Y:" + cameraPosition[1] + " camera after theta:" + (180 / Math.PI) * cameraPosition[2] + "\r\n");
                        global_stuff.logcamera.WriteLine(" deltaCamera X:" + deltaCameraposition[0] + " deltaCamera Y:" + deltaCameraposition[1] + " deltaCamera theta:" + (180 / Math.PI) * deltaCameraposition[2] + "\r\n");

                        global_stuff.timeStamp[j] = p.AllAGVs[j].LastUpdateCam;
                        return cameraPosition; // mm,mm,radians
                    }
                    else
                    {
                        return null;
                    }// for the time stampp if
                }
            }
            throw new Exception("updateCurrentCameraPos fails");
        }

        //method to calculate the estimates robot location(x,y,theta) using camera and odometer data
        private double[] kalmanFilter(double[] camPos, double[] odoPos)
        {
            return odoPos;
            //return estimatedPosition;
        }

        //method that takes the goal point and returns the left and right wheels velocities
        private double[] calculateNextVelocities(double[] currentPositon)
        {
            //alternatives for choosing the current position: (odometer / camera / kalman) 
           
            //currentPositon[2] = cameraPosition[2];
           
            //global_stuff.logcamera.WriteLine("robot ID: " + robot.RobotID + " odo X:" + Math.Round(currentPositon[0],2) + " odo Y:" + Math.Round(currentPositon[1],2) + " odo theta:" + Math.Round(currentPositon[2],2)+'\r\n');
            //global_stuff.logcamera.Flush();

            global_stuff.logcamera.WriteLine(" camera X:" + currentPositon[0] + " camera Y:" + currentPositon[1] + " camera theta:" + (180 / Math.PI) * currentPositon[2] + "\r\n");
            //logCamera.Flush();
            //currentPositon = kalmanFilter();
            
            //S3.Stop();
            //Time3 = 1000 * S3.ElapsedMilliseconds;
            //S3.Start();
            theta = currentPositon[2]; // in radians

            if (theta > Math.PI)
                theta = theta - 2 * Math.PI;
            else if (theta < -Math.PI)
                theta = 2 * Math.PI + theta;

            rho = Math.Sqrt(Math.Pow((goalPosition[1] - currentPositon[1]), 2) + Math.Pow((goalPosition[0] - currentPositon[0]), 2));
            alpha = -theta + Math.Atan2((goalPosition[1] - currentPositon[1]), (goalPosition[0] - currentPositon[0]));
            beta = -theta - alpha + goalPosition[2];

            //only activate the integral part slightly before the goal point to eliminate the steady state error
            if (Math.Abs(rho) < 50)
            {
                integral = integral + rho * S0.GetElapsedTimeSecs();
            }
            derivative = (rho - pre_rho) / S0.GetElapsedTimeSecs();

            vel = k_rho * rho + k_rho_i * integral;// +k_rho_d * derivative;
            omega = k_alpha * alpha + k_beta * beta; //omega in radians/sec
            //calculate and scale down the left and right wheels velocities from vel and omega
            v_l = vel - 131.5 * omega;
            v_r = 2 * vel - v_l;
            global_stuff.logcamera.WriteLine(" theta:" + Math.Round(((180 / Math.PI) * theta),2) + " beta:" + Math.Round(((180 / Math.PI) * beta),2) + " deravative:" + derivative + " integral:" + integral + " vel:" + vel + " omega:" + omega + " rho:" + rho);

            if (v_l > 250 || v_r > 250)
            {
                if (v_l > v_r)
                {
                    v_r = 250 * v_r / v_l;
                    v_l = 250;
                }
                else if (v_r > v_l)
                {
                    v_l = 250 * v_l / v_r;
                    v_r = 250;
                }
            }
            global_stuff.logcamera.WriteLine("  left speed  " + v_l + "  right speed  " + v_r);
            global_stuff.logcamera.Flush();

            //return the left and right wheels velocities
            robotVelocity[0] = v_r; //  mm/sec
           robotVelocity[1] = v_l; //  mm/sec
            //robotVelocity[0] = 0;// v_r; //  mm/sec
            //robotVelocity[1] = 0;// v_l; //  mm/sec

            //condition to stop the robot when reaching the goal point within a certain threshold
            if (Math.Abs(currentPositon[2] - goalPosition[2]) <= 0.1)
                global_stuff.logcamera.WriteLine("GOAL REACHED");
            if (rho <= 60 && Math.Abs(currentPositon[2] - goalPosition[2]) <= 0.1)
            {
                robotVelocity[0] = 0;
                robotVelocity[1] = 0;
                currentPositon = goalPosition;
                rho = 0;
                pre_rho = 0;
                integral = 0;
                derivative = 0;
                alpha = 0;
                beta = 0;
                theta = 0;
                finished = true;
                vel = 0;
                omega = 0;
                S0.Stop();
            }

            pre_rho = rho; //will be used for the derivative part of the controller

            global_stuff.logcamera.Flush();
            return robotVelocity;
        }

        public void updateRobotVelocity(double[] vel)
        {
            robot.DriveDirect((short)vel[1], (short)vel[0]);
        }
    }      
};
