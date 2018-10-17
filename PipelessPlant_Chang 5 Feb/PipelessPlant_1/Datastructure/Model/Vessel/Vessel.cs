using System;
using System.Collections.Generic;
using System.Text;

namespace MULTIFORM_PCS.Datastructure.Model.Vessel
{
    public class Vessel
    {
        private double curFillWater;
        public double theCurFillWater
        {
            get { return curFillWater; }
            set { curFillWater = value; }
        }
        private double curFillHard;
        public double theCurFillHard
        {
            get { return curFillHard; }
            set { curFillHard = value; }
        }
        private double maxVolume;
        public double theMaxVolume
        {
            get { return maxVolume; }
            set { maxVolume = value; }
        }
        private int id;
        public int theId
        {
            get { return id; }
            set { id = value; }
        }
        private Datastructure.Model.Recipes.RecipeLayer[] layers;
        public Datastructure.Model.Recipes.RecipeLayer[] theLayers
        {
            get { return layers; }
            set { layers = value; }
        }
        private int[] colors;
        public int[] Colors
        {
            get { return colors; }
            set { colors = value; }
        }
        private bool[] finished;
        public bool[] Finished
        {
            get { return finished; }
            set { finished = value; }
        }

        public Vessel(double curFillWater, double curFillHard, double maxVolume, int id, int[] choosenColorIDs)
        {
            this.colors = new int[] { -1, -1, -1 };
            this.finished = new bool[] { false, false, false };
            this.curFillWater = curFillWater;
            this.theCurFillHard = curFillHard;
            this.maxVolume = maxVolume;
            this.id = id;
            layers = new Datastructure.Model.Recipes.RecipeLayer[5];
            for (int i = 0; i < layers.Length; i++)
            {
                layers[i] = new Datastructure.Model.Recipes.RecipeLayer("");
                for (int j = 0; j < choosenColorIDs.Length; j++)
                {
                    layers[i].theIngredients.Add(new Recipes.Ingredient(choosenColorIDs[j], 0, 0, 0, "", 0)); 
                }
            }
        }
    }
}
