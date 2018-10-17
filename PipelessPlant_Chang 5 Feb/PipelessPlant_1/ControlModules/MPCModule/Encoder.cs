using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MULTIFORM_PCS.ControlModules.MPCModule
{
    class Encoder
    {
        public int[] odometerValues={0,0,0,0,0,0};

        
        public void values(string message)
        {
            
            string[] words = message.Split(' ');
                 if ((message.Contains("192.168.1.101")))
                 {
                     if (words[2] == "distance")
                     {
                         int deltaDist = Convert.ToInt32(words[3]);
                         odometerValues[0] += deltaDist;
                     }
                     else if (words[2] == "angle")
                     {
                         int deltaAngle = Convert.ToInt32(words[3]);
                         odometerValues[1] += deltaAngle;
                     }

                 }

                 if ((message.Contains("192.168.1.102")))
                 {
                     if (words[2] == "distance")
                     {
                         int deltaDist = Convert.ToInt32(words[3]);
                         odometerValues[2] += deltaDist;
                     }
                     else if (words[2] == "angle")
                     {
                         int deltaAngle = Convert.ToInt32(words[3]);
                         odometerValues[3] += deltaAngle;
                     }

                 }
                 if ((message.Contains("192.168.1.104")))
                 {
                     if (words[2] == "distance")
                     {
                         int deltaDist = Convert.ToInt32(words[3]);
                         odometerValues[4] += deltaDist;
                     }
                     else if (words[2] == "angle")
                     {
                         int deltaAngle = Convert.ToInt32(words[3]);
                         odometerValues[5] += deltaAngle;
                     }
                 }
                 returnValues(odometerValues);
            
                 //Console.WriteLine(odometerValues[4]);          
             }
        public int[] returnValues(int[] values)
        {
            int[] val = values; 
            return val;
        }
        
    }
}
