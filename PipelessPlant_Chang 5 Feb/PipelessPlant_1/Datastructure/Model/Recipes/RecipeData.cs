using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MULTIFORM_PCS.Datastructure.Model.Recipes
{
    public class RecipeData
    {
        public int storageCount = 1;
        public int mixingCount = 1;
        public int fillingCount = 2;
        public int agvUsed = 2;

        public enum POSITIONS
        {
            STORAGE1,
            MIXING1,
            FILLING1,
            FILLING2,
            INIT1,
            INIT2
        }

        public float dockingTime = 10;
        public float undockTime = 5;
        public float mixingTime = 85;
        public float mixingGrabTime = 36;
        public float mixingReleaseTime = 39;
        public float fillingTime = 6;
        public float storagePutVesselToAGV = 74;
        public float storageTakeVesselFromAGV = 87;
        public float hardeningTime = 360;

        public float[][] distancesInMovementTime = new float[][] {
            new float[] {0,-1,5,5,-1,-1},
            new float[] {5,0,-1,-1,-1,-1}, 
            new float[] {-1,5,0,5,-1,-1}, 
            new float[] {-1,5,5,0,-1,-1}, 
            new float[] {5,-1,-1,-1,0,-1}, 
            new float[] {5,-1,-1,-1,-1,0}
        };

        public int recipesCount;// = 8;
        public int[] layerCount = { 3, 3, 3, 3, 3, 3 };

        public enum RECIPE
        {
            YELLOW, //ColorStation 1
            BLACK, //ColorStation 1
            RED, //ColorStation 2
            BLUE, //ColorStation 2
            PURPLE, //ColorStation 2
            ORANGE, //ColorStation 1 and 2
            GREEN //ColorStation 1 and 2
        }
        public RECIPE[][] selectedRecipes = new RECIPE[][] {
            new RECIPE[] {RECIPE.YELLOW, RECIPE.RED, RECIPE.BLACK},
            new RECIPE[] {RECIPE.BLUE, RECIPE.YELLOW, RECIPE.BLUE},
            new RECIPE[] {RECIPE.GREEN, RECIPE.YELLOW, RECIPE.BLACK},
            new RECIPE[] {RECIPE.PURPLE, RECIPE.GREEN, RECIPE.ORANGE},
            new RECIPE[] {RECIPE.BLUE, RECIPE.BLUE, RECIPE.BLACK},
            new RECIPE[] {RECIPE.BLACK, RECIPE.ORANGE, RECIPE.RED}
        };

        public RecipeData(int numberOfRecipes)
        {
            this.recipesCount = numberOfRecipes;
        }

        public RecipeData deepCopy()
        {
            RecipeData r = new RecipeData(this.recipesCount);

            r.agvUsed = this.agvUsed;
            r.storageCount = this.storageCount;
            r.mixingCount = this.mixingCount;
            r.fillingCount = this.fillingCount;

            r.dockingTime = this.dockingTime;
            r.undockTime = this.undockTime;
            r.mixingTime = this.mixingTime;
            r.mixingGrabTime = this.mixingGrabTime;
            r.fillingTime = this.fillingTime;
            r.storagePutVesselToAGV = this.storagePutVesselToAGV;
            r.storageTakeVesselFromAGV = this.storageTakeVesselFromAGV;
            r.hardeningTime = this.hardeningTime;

            r.distancesInMovementTime = new float[this.distancesInMovementTime.Length][];
            for (int i = 0; i < this.distancesInMovementTime.Length; i++)
            {
                r.distancesInMovementTime[i] = new float[this.distancesInMovementTime[i].Length];
                for (int j = 0; j < this.distancesInMovementTime[i].Length; j++)
                {
                    r.distancesInMovementTime[i][j] = this.distancesInMovementTime[i][j];
                }
            }

            r.layerCount = new int[this.layerCount.Length];
            for (int i = 0; i < this.layerCount.Length; i++)
            {
                r.layerCount[i] = this.layerCount[i];
            }

            r.selectedRecipes = new RECIPE[this.selectedRecipes.Length][];
            for (int i = 0; i < this.selectedRecipes.Length; i++)
            {
                r.selectedRecipes[i] = new RECIPE[this.selectedRecipes[i].Length];
                for (int j = 0; j < this.selectedRecipes[i].Length; j++)
                {
                    r.selectedRecipes[i][j] = this.selectedRecipes[i][j];
                }
            }

            return r;
        }
    }
}
