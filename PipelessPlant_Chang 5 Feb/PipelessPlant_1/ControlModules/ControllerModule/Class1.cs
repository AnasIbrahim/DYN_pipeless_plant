using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ControllerClass
{
    public class Controller
    {
        //Default Constructor
        public Controller(double TranslationGain, double RotationGain)
        {
            Translation_Gain    = TranslationGain;
            Rotation_Gain       = RotationGain;
        }

        //
        public double CalculateVelocity(double CurrentX, double CurrentY, double CurrentAngle, double DesiredX, double DesiredY)
        {

            // the distance between 2 wheels
            double L = 2.58;

            // the radius of each wheel
            double R = 0.34;


            double RotationThreshold    = 2;  //degrees

            double TranslationThreshold = 100; //mm

            double Distance = Math.Sqrt( (DesiredX-CurrentX)^2 + (DesiredY-CurrentY)^2 );

            // Linear velocity of the robot
            double V = 0;

            // Angular velocity of the robot
            double W = 0;

            // the slope we need to rotate
            double Slope = Math.Atan2((DesiredY - CurrentY), (DesiredX - CurrentX));


            //error calculation
            if (Math.Abs(Slope - CurrentAngle) < RotationThreshold)
            {
                if (Math.Abs(Distance) < TranslationThreshold)
                {
                    // we have reched the desired destination
                    return 0;
                }
                else
                {
                    V = Distance * Translation_Gain;
                    RobotDirection = DIRECTION.Forward;
                    return (V);
                }
            }
            else
            {

                W = (Slope - CurrentAngle) * Translation_Gain;
                
                if (W > 0)
                {
                    RobotDirection = DIRECTION.Clockwise;

                }
                else
                {
                    RobotDirection = DIRECTION.AntiClockwise;
                }
                return (W);
            }

            WheelsVelocities.RightWheelVelocity = (2 * V + W * L) / 2 * R;
            WheelsVelocities.LeftWheelVelocity = (2 * V - W * L) / 2 * R;

        }
        
        
        //Mutator Functions
        public void SetTranslationGain(double TranslationGain)
        {
            Translation_Gain = TranslationGain;
        }

        public void SetRotationGain(double RotationGain)
        {
            Rotation_Gain = RotationGain;
        }


        //Accessor Functions
        public double GetTranslationGain()
        {
            return (Translation_Gain);
        }

        public double GetRotationGain()
        {
            return (Rotation_Gain);
        }

        public double GetRightWheelVelocity()
        {
            return (WheelsVelocities.RightWheelVelocity);
        }

        public double GetLeftWheelVelocity()
        {
            return (WheelsVelocities.LeftWheelVelocity);
        }

        public string GetDirection()
        {
            switch (RobotDirection)
            {
                case(DIRECTION.Forward):
                    return ("Forward");
                    
                case(DIRECTION.Backward):
                    return ("Backward");

                case(DIRECTION.Clockwise):
                    return ("Clockwise");

                case(DIRECTION.AntiClockwise):
                    return ("AntiClockwise");

                default:
                    return ("Forward");
            }
        }



        //Data types
        private struct WHEELS_VELOCITIES
        {
            public double RightWheelVelocity;
            public double LeftWheelVelocity;
        }

        private enum DIRECTION
        {
            Forward         = 0,
            Backward        = 1,
            Clockwise       = 2,
            AntiClockwise   = 3
        }

        private enum MOTION_PHASE
        {
            Rotation_Phase      = 0,
            Translation_Phase   = 1
        }


        //Variable names
        private double Translation_Gain = 1;
        private double Rotation_Gain    = 1;

        private WHEELS_VELOCITIES WheelsVelocities = new WHEELS_VELOCITIES();
        private DIRECTION RobotDirection;

        private MOTION_PHASE Motion_Phase = MOTION_PHASE.Rotation_Phase;
    }
}
