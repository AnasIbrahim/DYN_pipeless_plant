using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MULTIFORM_PCS
{
    public static class global_stuff
    { 
           public static double cameraPosition_X ;
        public static double cameraPosition_Y;
       public static double cameraAngle;
       public static StreamWriter logcamera = new StreamWriter("C:\\Users\\Pipelessplant\\Desktop\\logcamera.txt");
       public static DateTime[] timeStamp= new DateTime[4];

    }
}
