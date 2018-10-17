using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MULTIFORM_PCS.ControlModules.ConnectionModule
{
    class RobotBatteryStatusPublishers
    {
        /*This class is assumed to publish an Event whenever the Robot's battery status is low*/
        public class CustomEventArgs : EventArgs
        {
            public CustomEventArgs(string s)
            {
                message = s;
            }
            private string message;

            public string Message
            {
                get { return message; }
                set { message = value; }
            }
        }

        public class PublisherBatteryAGV1
        {
            public event EventHandler<CustomEventArgs> lowBatteryEvent;

            public void LowBatt()
            {
                reportLowBattery(new CustomEventArgs("AGV1 Battery Low!"));
            }

            protected virtual void reportLowBattery(CustomEventArgs e)
            {
                EventHandler<CustomEventArgs> handler = lowBatteryEvent;

                // Event will be null if there are no subscribers 
                if (handler != null)
                {
                    e.Message += String.Format(" at {0}", DateTime.Now.ToString());
                    handler(this, e);
                }
            }
        }

        public class PublisherBatteryAGV2
        {
            public event EventHandler<CustomEventArgs> lowBatteryEvent;

            public void LowBatt()
            {
                reportLowBattery(new CustomEventArgs("AGV2 Battery Low!"));
            }

            protected virtual void reportLowBattery(CustomEventArgs e)
            {
                EventHandler<CustomEventArgs> handler = lowBatteryEvent;
                if (handler != null)
                {
                    e.Message += String.Format(" at {0}", DateTime.Now.ToString());
                    handler(this, e);
                }
            }
        }

        public class PublisherBatteryAGV3
        {
            public event EventHandler<CustomEventArgs> lowBatteryEvent;

            public void LowBatt(double timeOfLowBatt)
            {
                reportLowBattery(new CustomEventArgs("AGV3 Battery Low!"),timeOfLowBatt);
            }

            protected virtual void reportLowBattery(CustomEventArgs e,double timeOfLowBatt)
            {
                EventHandler<CustomEventArgs> handler = lowBatteryEvent;
                if (handler != null)
                {
                    e.Message += String.Format(" at {0}", timeOfLowBatt);//, DateTime.Now.ToString());
                    handler(this, e);
                }
            }
        }


    }
}
