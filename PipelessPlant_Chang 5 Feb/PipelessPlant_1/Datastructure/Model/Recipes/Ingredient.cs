using System;
using System.Collections.Generic;
using System.Text;

namespace MULTIFORM_PCS.Datastructure.Model.Recipes
{
    public class Ingredient
    {
        private int colorID;
        public int theColorID
        {
            get { return colorID; }
            set { colorID = value; }
        }
        private String name;
        public String theName
        {
            get { return name; }
            set { name = value; }
        }
        private double fillTime;
        public double theFillTime
        {
            get { return fillTime; }
            set { fillTime = value; }
        }
        private double mixTime;
        public double theMixTime
        {
            get { return mixTime; }
            set { mixTime = value; }
        }
        private double mixFillTime;
        public double theMixFillTime
        {
            get { return mixFillTime; }
            set { mixFillTime = value; }
        }
        private double curVolume;
        public double theCurVolume
        {
            get { return curVolume; }
            set { curVolume = value; }
        }

        public Ingredient(int colorID, double fillTime, double mixTime, double mixFillTime,String name, double vol)
        {
            this.colorID = colorID;
            this.fillTime = fillTime;
            this.mixFillTime = mixFillTime;
            this.mixTime = mixTime;
            this.name = name;
            this.theCurVolume = vol;
        }
    }
}
