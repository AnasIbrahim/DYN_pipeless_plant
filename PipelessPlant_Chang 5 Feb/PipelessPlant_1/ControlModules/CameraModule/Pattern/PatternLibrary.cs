using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MULTIFORM_PCS.ControlModules.CameraModule.Pattern
{
    static class PatternLibrary
    {
        private static RobotMarker[] patternLib;

        public static RobotMarker[] getPatternLib()
        {
            if(patternLib == null)
            {
                RobotMarker id1 = new RobotMarker(1, 0);
                RobotMarker id2 = new RobotMarker(0, 1);
                RobotMarker id3 = new RobotMarker(0, 2);
                RobotMarker id4 = new RobotMarker(2, 0);
                RobotMarker id5 = new RobotMarker(1, 1);
                patternLib = new RobotMarker[] { id1, id2, id3, id4, id5 };
            }
            return patternLib;
        }
    }

    class RobotMarker
    {
        public int leftCount;
        public int rightCount;

        public RobotMarker(int l, int r)
        {
            this.leftCount = l;
            this.rightCount = r;
        }
    }
}
