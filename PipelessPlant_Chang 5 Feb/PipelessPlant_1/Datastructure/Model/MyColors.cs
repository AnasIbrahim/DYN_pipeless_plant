using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;

namespace MULTIFORM_PCS.Datastructure.Model
{
    public class MyColors
    {
        private Color[] colors = new Color[] { Color.Yellow, Color.Black, Color.Red, Color.Blue };
        private string[] colorNames = new string[] {"Yellow", "Black", "Red", "Blue"};

        #region singletonPattern;
        private static MyColors col;
        private MyColors()
        {   
        }
        public static MyColors getInstance()
        {
            if (col == null)
            {
                col = new MyColors();
            }
            return col;
        }
        #endregion;

        public Color getColor(int colorID)
        {
            if (colorID >= 0 && colorID < 4)
            {
                return colors[colorID];
            }
            else return Color.White;
        }
        public string getName(int colorID)
        {
            if (colorID >= 0 && colorID < 4)
            {
                return colorNames[colorID];
            }
            else return "NONE";
        }
    }
}
