using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MULTIFORM_PCS.ControlModules.SchedulingModule.TAOptModel
{
    public class TAOptModelBuilder
    {
        #region singletonPattern;
        private static TAOptModelBuilder builder;
        public static TAOptModelBuilder getInstance()
        {
            if (builder == null)
            {
                builder = new TAOptModelBuilder();
            }
            return builder;
        }
        private TAOptModelBuilder()
        {
        }
        #endregion;


        public void buildModel(Datastructure.Model.Recipes.RecipeData d, string folder)
        {
            StreamWriter ordersWriter = new StreamWriter(folder + "\\orders.trtn", false);
            for (int i = 0; i < d.recipesCount; i++)
            {
                ordersWriter.WriteLine("" + (i + 1) + "\t" + (9001 + i) + "\t12345\t1");
            }
            ordersWriter.Flush();
            ordersWriter.Close();

            StreamWriter plantWriter = new StreamWriter(folder + "\\plant.trtn", false);
            plantWriter.WriteLine("3001\t1\t1");
            plantWriter.WriteLine("3002\t1\t1");
            plantWriter.WriteLine("3003\t1\t1");
            plantWriter.WriteLine("3004\t1\t1");
            int resourceIndex = 3010;
            for (int i = 0; i < d.recipesCount; i++)
            {
                plantWriter.WriteLine("" + (resourceIndex + i) + "\t1\t1");
            }
            plantWriter.WriteLine("3999\t1\t1");
            plantWriter.WriteLine("8000\t1\t1");
            plantWriter.WriteLine("#");
            plantWriter.WriteLine("3001\t1");
            plantWriter.WriteLine("3002\t1");
            plantWriter.WriteLine("3003\t1");
            plantWriter.WriteLine("3004\t1");
            for (int i = 0; i < d.recipesCount; i++)
            {
                plantWriter.WriteLine("" + (resourceIndex + i) + "\t1");
            }
            plantWriter.WriteLine("3999\t1");
            plantWriter.WriteLine("8000\t1");
            plantWriter.WriteLine("8000\t2");
            plantWriter.WriteLine("#");
            plantWriter.WriteLine("8000\t2\t1\t0\t0\t2");
            plantWriter.WriteLine("#");
            plantWriter.WriteLine("5001\t0\t0\t1\t1\t-1\t0");
            plantWriter.WriteLine("5002\t0\t0\t1\t1\t1\t0");
            plantWriter.WriteLine("5003\t0\t0\t1\t1\t1\t0");
            plantWriter.WriteLine("5004\t0\t0\t1\t1\t1\t0");
            plantWriter.WriteLine("7001\t0\t0\t" + (10 + d.recipesCount) + "\t2\t-1\t0");
            plantWriter.WriteLine("7002\t0\t0\t" + (10 + d.recipesCount) + "\t2\t-1\t0");
            plantWriter.Flush();
            plantWriter.Close();

            StreamWriter sequencesWriter = new StreamWriter(folder + "\\sequence_mapping.log", false);

            for (int i = 0; i < d.recipesCount; i++)
            {
                StreamWriter recipeWriter = new StreamWriter(folder + "\\recipe_" + (i + 1) + ".trtn", false);

                int indexTask = 101;
                int indexState = 201;
                int indexSpezial1 = 1901;
                int indexSpezial2 = 1951;
                int indexStateSpezial = 2901;

                recipeWriter.WriteLine("" + (9001 + i));
                //Task definition
                recipeWriter.WriteLine("#");
                for (int j = 0; j < d.layerCount[i]; j++)
                {
                    recipeWriter.WriteLine("" + indexTask + "1\t3001\t1\t" + (d.undockTime + d.storagePutVesselToAGV) + "\t" + (d.undockTime + d.storagePutVesselToAGV) + "\t0\t1");
                    sequencesWriter.WriteLine((9001 + i) + "\t" + (i + 1) + "\t" + indexTask + "1\t" + (i + 1) + indexTask + "1\t3001\tRELEASE+UNDOCK\t" + d.storagePutVesselToAGV + "+" + d.undockTime);
                    recipeWriter.WriteLine("" + indexTask + "2\t3001\t1\t" + (d.distancesInMovementTime[4][0] + d.dockingTime + d.undockTime + d.storagePutVesselToAGV) + "\t" + (d.distancesInMovementTime[4][0] + d.dockingTime + d.undockTime + d.storagePutVesselToAGV) + "\t0\t1");
                    sequencesWriter.WriteLine((9001 + i) + "\t" + (i + 1) + "\t" + indexTask + "2\t" + (i + 1) + indexTask + "2\t3001\tMOVE_INIT1_TO_STORAGE+DOCK+RELEASE+UNDOCK\t" + d.distancesInMovementTime[4][0] + "+" + d.dockingTime + "+" + d.storagePutVesselToAGV + "+" + d.undockTime); 
                    recipeWriter.WriteLine("" + indexTask + "3\t3001\t1\t" + (d.undockTime + d.storagePutVesselToAGV) + "\t" + (d.undockTime + d.storagePutVesselToAGV) + "\t0\t1");
                    sequencesWriter.WriteLine((9001 + i) + "\t" + (i + 1) + "\t" + indexTask + "3\t" + (i + 1) + indexTask + "3\t3001\tRELEASE+UNDOCK\t" + d.storagePutVesselToAGV + "+" + d.undockTime); 
                    recipeWriter.WriteLine("" + indexTask + "4\t3001\t1\t" + (d.distancesInMovementTime[5][0] + d.dockingTime + d.undockTime + d.storagePutVesselToAGV) + "\t" + (d.distancesInMovementTime[5][0] + d.dockingTime + d.undockTime + d.storagePutVesselToAGV) + "\t0\t1");
                    sequencesWriter.WriteLine((9001 + i) + "\t" + (i + 1) + "\t" + indexTask + "4\t" + (i + 1) + indexTask + "4\t3001\tMOVE_INIT2_TO_STORAGE+DOCK+RELEASE+UNDOCK\t" + d.distancesInMovementTime[5][0] + "+" + d.dockingTime + "+" + d.storagePutVesselToAGV + "+" + d.undockTime);  
                    indexTask++;
                    if (d.selectedRecipes[i][j] == Datastructure.Model.Recipes.RecipeData.RECIPE.BLACK || d.selectedRecipes[i][j] == Datastructure.Model.Recipes.RecipeData.RECIPE.YELLOW)
                    {
                        recipeWriter.WriteLine("" + indexTask + "0\t3002\t1\t" + (d.distancesInMovementTime[0][2] + d.dockingTime + d.fillingTime*2 + d.undockTime) + "\t" + (d.distancesInMovementTime[0][2] + d.dockingTime + d.fillingTime*2 + d.undockTime) + "\t0\t1");
                        if (d.selectedRecipes[i][j] == Datastructure.Model.Recipes.RecipeData.RECIPE.BLACK)
                        {
                            sequencesWriter.WriteLine((9001 + i) + "\t" + (i + 1) + "\t" + indexTask + "0\t" + (i + 1) + indexTask + "0\t3002\tMOVE_STORAGE_TO_COL1+DOCK+FILL_BLACK+UNDOCK\t" + d.distancesInMovementTime[0][2] + "+" + d.dockingTime + "+" + d.fillingTime*2 + "+" + d.undockTime);
                        }
                        else if (d.selectedRecipes[i][j] == Datastructure.Model.Recipes.RecipeData.RECIPE.YELLOW)
                        {
                            sequencesWriter.WriteLine((9001 + i) + "\t" + (i + 1) + "\t" + indexTask + "0\t" + (i + 1) + indexTask + "0\t3002\tMOVE_STORAGE_TO_COL1+DOCK+FILL_YELLOW+UNDOCK\t" + d.distancesInMovementTime[0][2] + "+" + d.dockingTime + "+" + d.fillingTime*2 + "+" + d.undockTime);
                        }
                        indexTask++;
                        recipeWriter.WriteLine("" + indexTask + "0\t3004\t1\t" + (d.distancesInMovementTime[2][1] + d.dockingTime + d.mixingGrabTime + d.mixingTime + d.mixingReleaseTime + d.undockTime) + "\t" + (d.distancesInMovementTime[2][1] + d.dockingTime + d.mixingGrabTime + d.mixingTime + d.mixingReleaseTime + d.undockTime) + "\t0\t1");
                        sequencesWriter.WriteLine((9001 + i) + "\t" + (i + 1) + "\t" + indexTask + "0\t" + (i + 1) + indexTask + "0\t3004\tMOVE_COL1_TO_MIX+DOCK+GRAB+MIX+RELEASE+UNDOCK\t" + d.distancesInMovementTime[2][1] + "+" + d.dockingTime + "+" + d.mixingGrabTime + "+" + d.mixingTime + "+" + d.mixingReleaseTime + "+" + d.undockTime);
                        indexTask++;
                    }
                    else if (d.selectedRecipes[i][j] == Datastructure.Model.Recipes.RecipeData.RECIPE.BLUE || d.selectedRecipes[i][j] == Datastructure.Model.Recipes.RecipeData.RECIPE.RED || d.selectedRecipes[i][j] == Datastructure.Model.Recipes.RecipeData.RECIPE.PURPLE)
                    {
                        recipeWriter.WriteLine("" + indexTask + "0\t3003\t1\t" + (d.distancesInMovementTime[0][3] + d.dockingTime + d.fillingTime*2 + d.undockTime) + "\t" + (d.distancesInMovementTime[0][3] + d.dockingTime + d.fillingTime*2 + d.undockTime) + "\t0\t1");
                        if (d.selectedRecipes[i][j] == Datastructure.Model.Recipes.RecipeData.RECIPE.BLUE)
                        {
                            sequencesWriter.WriteLine((9001 + i) + "\t" + (i + 1) + "\t" + indexTask + "0\t" + (i + 1) + indexTask + "0\t3003\tMOVE_STORAGE_TO_COL2+DOCK+FILL_BLUE+UNDOCK\t" + d.distancesInMovementTime[0][3] + "+" + d.dockingTime + "+" + d.fillingTime*2 + "+" + d.undockTime);
                        }
                        else if (d.selectedRecipes[i][j] == Datastructure.Model.Recipes.RecipeData.RECIPE.RED)
                        {
                            sequencesWriter.WriteLine((9001 + i) + "\t" + (i + 1) + "\t" + indexTask + "0\t" + (i + 1) + indexTask + "0\t3003\tMOVE_STORAGE_TO_COL2+DOCK+FILL_RED+UNDOCK\t" + d.distancesInMovementTime[0][3] + "+" + d.dockingTime + "+" + d.fillingTime * 2 + "+" + d.undockTime);
                        }
                        else if (d.selectedRecipes[i][j] == Datastructure.Model.Recipes.RecipeData.RECIPE.PURPLE)
                        {
                            sequencesWriter.WriteLine((9001 + i) + "\t" + (i + 1) + "\t" + indexTask + "0\t" + (i + 1) + indexTask + "0\t3003\tMOVE_STORAGE_TO_COL2+DOCK+FILL_PURPLE+UNDOCK\t" + d.distancesInMovementTime[0][3] + "+" + d.dockingTime + "+" + d.fillingTime * 2 + "+" + d.undockTime);
                        }
                        indexTask++;
                        recipeWriter.WriteLine("" + indexTask + "0\t3004\t1\t" + (d.distancesInMovementTime[3][1] + d.dockingTime + d.mixingGrabTime + d.mixingTime + d.mixingReleaseTime + d.undockTime) + "\t" + (d.distancesInMovementTime[3][1] + d.dockingTime + d.mixingGrabTime + d.mixingTime + d.mixingReleaseTime + d.undockTime) + "\t0\t1");
                        sequencesWriter.WriteLine((9001 + i) + "\t" + (i + 1) + "\t" + indexTask + "0\t" + (i + 1) + indexTask + "0\t3004\tMOVE_COL2_TO_MIX+DOCK+GRAB+MIX+RELEASE+UNDOCK\t" + d.distancesInMovementTime[3][1] + "+" + d.dockingTime + "+" + d.mixingGrabTime + "+" + d.mixingTime + "+" + d.mixingReleaseTime + "+" + d.undockTime);
                        indexTask++;
                    }
                    else if (d.selectedRecipes[i][j] == Datastructure.Model.Recipes.RecipeData.RECIPE.ORANGE || d.selectedRecipes[i][j] == Datastructure.Model.Recipes.RecipeData.RECIPE.GREEN)
                    {
                        recipeWriter.WriteLine("" + indexTask + "1\t3002\t1\t" + (d.distancesInMovementTime[0][2] + d.dockingTime + d.fillingTime + d.undockTime) + "\t" + (d.distancesInMovementTime[0][2] + d.dockingTime + d.fillingTime + d.undockTime) + "\t0\t1");
                        recipeWriter.WriteLine("" + indexTask + "2\t3003\t1\t" + (d.distancesInMovementTime[0][3] + d.dockingTime + d.fillingTime + d.undockTime) + "\t" + (d.distancesInMovementTime[0][3] + d.dockingTime + d.fillingTime + d.undockTime) + "\t0\t1");
                        if (d.selectedRecipes[i][j] == Datastructure.Model.Recipes.RecipeData.RECIPE.ORANGE)
                        {
                            sequencesWriter.WriteLine((9001 + i) + "\t" + (i + 1) + "\t" + indexTask + "1\t" + (i + 1) + indexTask + "1\t3002\tMOVE_STORAGE_TO_COL1+DOCK+FILL_ORANGE+UNDOCK\t" + d.distancesInMovementTime[0][2] + "+" + d.dockingTime + "+" + d.fillingTime + "+" + d.undockTime);
                            sequencesWriter.WriteLine((9001 + i) + "\t" + (i + 1) + "\t" + indexTask + "2\t" + (i + 1) + indexTask + "2\t3003\tMOVE_STORAGE_TO_COL2+DOCK+FILL_ORANGE+UNDOCK\t" + d.distancesInMovementTime[0][3] + "+" + d.dockingTime + "+" + d.fillingTime + "+" + d.undockTime);
                        }
                        else if (d.selectedRecipes[i][j] == Datastructure.Model.Recipes.RecipeData.RECIPE.GREEN)
                        {
                            sequencesWriter.WriteLine((9001 + i) + "\t" + (i + 1) + "\t" + indexTask + "1\t" + (i + 1) + indexTask + "1\t3002\tMOVE_STORAGE_TO_COL1+DOCK+FILL_GREEN+UNDOCK\t" + d.distancesInMovementTime[0][2] + "+" + d.dockingTime + "+" + d.fillingTime + "+" + d.undockTime);
                            sequencesWriter.WriteLine((9001 + i) + "\t" + (i + 1) + "\t" + indexTask + "2\t" + (i + 1) + indexTask + "2\t3003\tMOVE_STORAGE_TO_COL2+DOCK+FILL_GREEN+UNDOCK\t" + d.distancesInMovementTime[0][3] + "+" + d.dockingTime + "+" + d.fillingTime + "+" + d.undockTime);
                        }
                        indexTask++;
                        recipeWriter.WriteLine("" + indexTask + "1\t3003\t1\t" + (d.distancesInMovementTime[2][3] + d.dockingTime + d.fillingTime + d.undockTime) + "\t" + (d.distancesInMovementTime[2][3] + d.dockingTime + d.fillingTime + d.undockTime) + "\t0\t1");
                        recipeWriter.WriteLine("" + indexTask + "2\t3002\t1\t" + (d.distancesInMovementTime[3][2] + d.dockingTime + d.fillingTime + d.undockTime) + "\t" + (d.distancesInMovementTime[3][2] + d.dockingTime + d.fillingTime + d.undockTime) + "\t0\t1");
                        if (d.selectedRecipes[i][j] == Datastructure.Model.Recipes.RecipeData.RECIPE.ORANGE)
                        {
                            sequencesWriter.WriteLine((9001 + i) + "\t" + (i + 1) + "\t" + indexTask + "1\t" + (i + 1) + indexTask + "1\t3003\tMOVE_COL1_TO_COL2+DOCK+FILL_ORANGE+UNDOCK\t" + d.distancesInMovementTime[2][3] + "+" + d.dockingTime + "+" + d.fillingTime + "+" + d.undockTime);
                            sequencesWriter.WriteLine((9001 + i) + "\t" + (i + 1) + "\t" + indexTask + "2\t" + (i + 1) + indexTask + "2\t3002\tMOVE_COL2_TO_COL1+DOCK+FILL_ORANGE+UNDOCK\t" + d.distancesInMovementTime[3][2] + "+" + d.dockingTime + "+" + d.fillingTime + "+" + d.undockTime);
                        }
                        else if (d.selectedRecipes[i][j] == Datastructure.Model.Recipes.RecipeData.RECIPE.GREEN)
                        {
                            sequencesWriter.WriteLine((9001 + i) + "\t" + (i + 1) + "\t" + indexTask + "1\t" + (i + 1) + indexTask + "1\t3003\tMOVE_COL1_TO_COL2+DOCK+FILL_GREEN+UNDOCK\t" + d.distancesInMovementTime[2][3] + "+" + d.dockingTime + "+" + d.fillingTime + "+" + d.undockTime);
                            sequencesWriter.WriteLine((9001 + i) + "\t" + (i + 1) + "\t" + indexTask + "2\t" + (i + 1) + indexTask + "2\t3002\tMOVE_COL2_TO_COL1+DOCK+FILL_GREEN+UNDOCK\t" + d.distancesInMovementTime[3][2] + "+" + d.dockingTime + "+" + d.fillingTime + "+" + d.undockTime);
                        }
                        indexTask++;
                        recipeWriter.WriteLine("" + indexTask + "1\t3004\t1\t" + (d.distancesInMovementTime[3][1] + d.dockingTime + d.mixingGrabTime + d.mixingTime + d.mixingReleaseTime + d.undockTime) + "\t" + (d.distancesInMovementTime[3][1] + d.dockingTime + d.mixingGrabTime + d.mixingTime + d.mixingReleaseTime + d.undockTime) + "\t0\t1");
                        recipeWriter.WriteLine("" + indexTask + "2\t3004\t1\t" + (d.distancesInMovementTime[2][1] + d.dockingTime + d.mixingGrabTime + d.mixingTime + d.mixingReleaseTime + d.undockTime) + "\t" + (d.distancesInMovementTime[2][1] + d.dockingTime + d.mixingGrabTime + d.mixingTime + d.mixingReleaseTime + d.undockTime) + "\t0\t1");
                        sequencesWriter.WriteLine((9001 + i) + "\t" + (i + 1) + "\t" + indexTask + "1\t" + (i + 1) + indexTask + "1\t3004\tMOVE_COL2_TO_MIX+DOCK+GRAB+MIX+RELEASE+UNDOCK\t" + d.distancesInMovementTime[3][1] + "+" + d.dockingTime + "+" + d.mixingGrabTime + "+" + d.mixingTime + "+" + d.mixingReleaseTime + "+" + d.undockTime);
                        sequencesWriter.WriteLine((9001 + i) + "\t" + (i + 1) + "\t" + indexTask + "2\t" + (i + 1) + indexTask + "2\t3004\tMOVE_COL1_TO_MIX+DOCK+GRAB+MIX+RELEASE+UNDOCK\t" + d.distancesInMovementTime[2][1] + "+" + d.dockingTime + "+" + d.mixingGrabTime + "+" + d.mixingTime + "+" + d.mixingReleaseTime + "+" + d.undockTime);                        
                        indexTask++;
                    }
                    recipeWriter.WriteLine("" + indexTask + "0\t3001\t1\t" + (d.distancesInMovementTime[1][0] + d.dockingTime + d.storageTakeVesselFromAGV) + "\t" + (d.distancesInMovementTime[1][0] + d.dockingTime + d.storageTakeVesselFromAGV) + "\t0\t1");
                    sequencesWriter.WriteLine((9001 + i) + "\t" + (i + 1) + "\t" + indexTask + "0\t" + (i + 1) + indexTask + "0\t3001\tMOVE_MIX_TO_STORAGE+DOCK+GRAB\t" + d.distancesInMovementTime[1][0] + "+" + d.dockingTime + "+" + d.storageTakeVesselFromAGV);
                    indexTask++;
                    recipeWriter.WriteLine("" + indexSpezial1 + "\t3999\t1\t0\t0\t0\t1");
                    sequencesWriter.WriteLine((9001 + i) + "\t" + (i + 1) + "\t" + indexSpezial1 + "\t" + (i + 1) + indexSpezial1 + "\t3999\tDUMMY\t0");
                    indexSpezial1++;
                    recipeWriter.WriteLine("" + indexSpezial1 + "\t3999\t1\t0\t0\t0\t1");
                    sequencesWriter.WriteLine((9001 + i) + "\t" + (i + 1) + "\t" + indexSpezial1 + "\t" + (i + 1) + indexSpezial1 + "\t3999\tDUMMY\t0");
                    indexSpezial1++;
                    recipeWriter.WriteLine("" + indexSpezial2 + "\t3001\t1\t" + (d.distancesInMovementTime[1][0] + d.dockingTime + d.storageTakeVesselFromAGV) + "\t" + (d.distancesInMovementTime[1][0] + d.dockingTime + d.storageTakeVesselFromAGV) + "\t0\t1");
                    sequencesWriter.WriteLine((9001 + i) + "\t" + (i + 1) + "\t" + indexSpezial2 + "\t" + (i + 1) + indexSpezial2 + "\t3001\tMOVE_MIX_TO_STORAGE+MOVE_OTHER_AGV_TO_INIT+DOCK+GRAB\t" + d.distancesInMovementTime[1][0] + "+" + d.distancesInMovementTime[1][0] + "+" + d.dockingTime + "+" + d.storageTakeVesselFromAGV);
                    indexSpezial2++;
                    recipeWriter.WriteLine("" + indexSpezial2 + "\t3001\t1\t" + (d.distancesInMovementTime[1][0] + d.dockingTime + d.storageTakeVesselFromAGV) + "\t" + (d.distancesInMovementTime[1][0] + d.dockingTime + d.storageTakeVesselFromAGV) + "\t0\t1");
                    sequencesWriter.WriteLine((9001 + i) + "\t" + (i + 1) + "\t" + indexSpezial2 + "\t" + (i + 1) + indexSpezial2 + "\t3001\tMOVE_MIX_TO_STORAGE+MOVE_OTHER_AGV_TO_INIT+DOCK+GRAB\t" + d.distancesInMovementTime[1][0] + "+" + d.distancesInMovementTime[1][0] + "+" + d.dockingTime + "+" + d.storageTakeVesselFromAGV);
                    indexSpezial2++;
                    recipeWriter.WriteLine("" + indexTask + "0\t" + (resourceIndex + i) + "\t1\t" + d.hardeningTime + "\t" + d.hardeningTime + "\t0\t1");
                    sequencesWriter.WriteLine((9001 + i) + "\t" + (i + 1) + "\t" + indexTask + "0\t" + (i + 1) + indexTask + "0\t" + (resourceIndex + i) + "\tHARDEN\t"+d.hardeningTime);
                    indexTask++;
                }
                //State definition
                recipeWriter.WriteLine("#");
                for (int j = 0; j < d.layerCount[i]; j++)
                {
                    if (j == 0)
                    {
                        recipeWriter.WriteLine("" + indexState + "0\t1\t0\t-1\t1\t0\t0");
                        indexState++;
                    }
                    if (d.selectedRecipes[i][j] == Datastructure.Model.Recipes.RecipeData.RECIPE.BLACK || d.selectedRecipes[i][j] == Datastructure.Model.Recipes.RecipeData.RECIPE.YELLOW)
                    {
                        recipeWriter.WriteLine("" + indexState + "0\t1\t0\t-1\t0\t0\t0");
                        indexState++;
                        recipeWriter.WriteLine("" + indexState + "0\t1\t0\t-1\t0\t0\t0");
                        indexState++;
                    }
                    else if (d.selectedRecipes[i][j] == Datastructure.Model.Recipes.RecipeData.RECIPE.BLUE || d.selectedRecipes[i][j] == Datastructure.Model.Recipes.RecipeData.RECIPE.RED || d.selectedRecipes[i][j] == Datastructure.Model.Recipes.RecipeData.RECIPE.PURPLE)
                    {
                        recipeWriter.WriteLine("" + indexState + "0\t1\t0\t-1\t0\t0\t0");
                        indexState++;
                        recipeWriter.WriteLine("" + indexState + "0\t1\t0\t-1\t0\t0\t0");
                        indexState++;
                    }
                    else if (d.selectedRecipes[i][j] == Datastructure.Model.Recipes.RecipeData.RECIPE.ORANGE || d.selectedRecipes[i][j] == Datastructure.Model.Recipes.RecipeData.RECIPE.GREEN)
                    {
                        recipeWriter.WriteLine("" + indexState + "0\t1\t0\t-1\t0\t0\t0");
                        indexState++;
                        recipeWriter.WriteLine("" + indexState + "1\t1\t0\t-1\t0\t0\t0");
                        recipeWriter.WriteLine("" + indexState + "2\t1\t0\t-1\t0\t0\t0");
                        indexState++;
                        recipeWriter.WriteLine("" + indexState + "1\t1\t0\t-1\t0\t0\t0");
                        recipeWriter.WriteLine("" + indexState + "2\t1\t0\t-1\t0\t0\t0");
                        indexState++;
                    }
                    recipeWriter.WriteLine("" + indexState + "0\t1\t0\t-1\t0\t0\t0");
                    indexState++;
                    recipeWriter.WriteLine("" + indexStateSpezial + "\t1\t0\t0\t0\t0\t0");
                    indexStateSpezial++;
                    recipeWriter.WriteLine("" + indexState + "0\t1\t0\t-1\t0\t0\t0");
                    indexState++;
                    if (j == d.layerCount[i] - 1)
                    {
                        recipeWriter.WriteLine("" + indexState + "0\t1\t0\t-1\t0\t1\t0");
                        indexState++;
                    }
                    else
                    {
                        recipeWriter.WriteLine("" + indexState + "0\t1\t0\t-1\t0\t0\t0");
                        indexState++;
                    }
                }
                //State outgoing arcs
                indexTask = 101;
                indexState = 201;
                indexSpezial1 = 1901;
                indexSpezial2 = 1951;
                indexStateSpezial = 2901;
                recipeWriter.WriteLine("#");
                for (int j = 0; j < d.layerCount[i]; j++)
                {
                    recipeWriter.WriteLine("" + indexState + "0\t" + indexTask + "1\t1");
                    recipeWriter.WriteLine("7001\t" + indexTask + "1\t1");
                    recipeWriter.WriteLine("" + indexState + "0\t" + indexTask + "2\t1");
                    recipeWriter.WriteLine("7001\t" + indexTask + "2\t2");
                    recipeWriter.WriteLine("5001\t" + indexTask + "2\t1");
                    recipeWriter.WriteLine("" + indexState + "0\t" + indexTask + "3\t1");
                    recipeWriter.WriteLine("7002\t" + indexTask + "3\t1");
                    recipeWriter.WriteLine("" + indexState + "0\t" + indexTask + "4\t1");
                    recipeWriter.WriteLine("7002\t" + indexTask + "4\t2");
                    recipeWriter.WriteLine("5001\t" + indexTask + "4\t1");
                    indexTask++;
                    indexState++;
                    if (d.selectedRecipes[i][j] == Datastructure.Model.Recipes.RecipeData.RECIPE.BLACK || d.selectedRecipes[i][j] == Datastructure.Model.Recipes.RecipeData.RECIPE.YELLOW)
                    {
                        recipeWriter.WriteLine("" + indexState + "0\t" + indexTask + "0\t1");
                        recipeWriter.WriteLine("5002\t" + indexTask + "0\t1");
                        recipeWriter.WriteLine("5001\t" + indexTask + "0\t-1");
                        indexTask++;
                        indexState++;
                        recipeWriter.WriteLine("" + indexState + "0\t" + indexTask + "0\t1");
                        recipeWriter.WriteLine("5004\t" + indexTask + "0\t1");
                        recipeWriter.WriteLine("5002\t" + indexTask + "0\t-1");
                        indexTask++;
                        indexState++;
                    }
                    else if (d.selectedRecipes[i][j] == Datastructure.Model.Recipes.RecipeData.RECIPE.BLUE || d.selectedRecipes[i][j] == Datastructure.Model.Recipes.RecipeData.RECIPE.RED || d.selectedRecipes[i][j] == Datastructure.Model.Recipes.RecipeData.RECIPE.PURPLE)
                    {
                        recipeWriter.WriteLine("" + indexState + "0\t" + indexTask + "0\t1");
                        recipeWriter.WriteLine("5003\t" + indexTask + "0\t1");
                        recipeWriter.WriteLine("5001\t" + indexTask + "0\t-1");
                        indexTask++;
                        indexState++;
                        recipeWriter.WriteLine("" + indexState + "0\t" + indexTask + "0\t1");
                        recipeWriter.WriteLine("5004\t" + indexTask + "0\t1");
                        recipeWriter.WriteLine("5003\t" + indexTask + "0\t-1");
                        indexTask++;
                        indexState++;
                    }
                    else if (d.selectedRecipes[i][j] == Datastructure.Model.Recipes.RecipeData.RECIPE.ORANGE || d.selectedRecipes[i][j] == Datastructure.Model.Recipes.RecipeData.RECIPE.GREEN)
                    {
                        recipeWriter.WriteLine("" + indexState + "0\t" + indexTask + "1\t1");
                        recipeWriter.WriteLine("5002\t" + indexTask + "1\t1");
                        recipeWriter.WriteLine("5001\t" + indexTask + "1\t-1");
                        recipeWriter.WriteLine("" + indexState + "0\t" + indexTask + "2\t1");
                        recipeWriter.WriteLine("5003\t" + indexTask + "2\t1");
                        recipeWriter.WriteLine("5001\t" + indexTask + "2\t-1");
                        indexTask++;
                        indexState++;
                        recipeWriter.WriteLine("" + indexState + "1\t" + indexTask + "1\t1");
                        recipeWriter.WriteLine("5003\t" + indexTask + "1\t1");
                        recipeWriter.WriteLine("5002\t" + indexTask + "1\t-1");
                        recipeWriter.WriteLine("" + indexState + "2\t" + indexTask + "2\t1");
                        recipeWriter.WriteLine("5002\t" + indexTask + "2\t1");
                        recipeWriter.WriteLine("5003\t" + indexTask + "2\t-1");
                        indexTask++;
                        indexState++;
                        recipeWriter.WriteLine("" + indexState + "1\t" + indexTask + "1\t1");
                        recipeWriter.WriteLine("5004\t" + indexTask + "1\t1");
                        recipeWriter.WriteLine("5003\t" + indexTask + "1\t-1");
                        recipeWriter.WriteLine("" + indexState + "2\t" + indexTask + "2\t1");
                        recipeWriter.WriteLine("5004\t" + indexTask + "2\t1");
                        recipeWriter.WriteLine("5002\t" + indexTask + "2\t-1");
                        indexTask++;
                        indexState++;
                    }
                    recipeWriter.WriteLine("" + indexState + "0\t" + indexTask + "0\t1");
                    recipeWriter.WriteLine("5001\t" + indexTask + "0\t1");
                    recipeWriter.WriteLine("5004\t" + indexTask + "0\t-1");
                    indexTask++;
                    recipeWriter.WriteLine("7001\t" + indexSpezial2 + "\t" + (11 + i));
                    recipeWriter.WriteLine("7002\t" + indexSpezial2 + "\t1");
                    recipeWriter.WriteLine("" + indexState + "0\t" + indexSpezial2 + "\t1");
                    recipeWriter.WriteLine("5004\t" + indexSpezial2 + "\t-1");
                    indexSpezial2++;
                    recipeWriter.WriteLine("7001\t" + indexSpezial2 + "\t1");
                    recipeWriter.WriteLine("7002\t" + indexSpezial2 + "\t" + (11 + i));
                    recipeWriter.WriteLine("" + indexState + "0\t" + indexSpezial2 + "\t1");
                    recipeWriter.WriteLine("5004\t" + indexSpezial2 + "\t-1");
                    indexSpezial2++;
                    indexState++;
                    recipeWriter.WriteLine("7001\t" + indexSpezial1 + "\t" + (11 + i));
                    recipeWriter.WriteLine("" + indexStateSpezial + "\t" + indexSpezial1 + "\t1");
                    indexSpezial1++;
                    recipeWriter.WriteLine("7002\t" + indexSpezial1 + "\t" + (11 + i));
                    recipeWriter.WriteLine("" + indexStateSpezial + "\t" + indexSpezial1 + "\t1");
                    indexSpezial1++;
                    indexStateSpezial++;
                    recipeWriter.WriteLine("" + indexState + "0\t" + indexTask + "0\t1");
                    indexTask++;
                    indexState++;
                }
                //Task outgoing arcs
                indexTask = 101;
                indexState = 202;
                indexSpezial1 = 1901;
                indexSpezial2 = 1951;
                indexStateSpezial = 2901;
                recipeWriter.WriteLine("#");
                for (int j = 0; j < d.layerCount[i]; j++)
                {
                    recipeWriter.WriteLine("" + indexTask + "1\t" + indexState + "0\t1");
                    recipeWriter.WriteLine("" + indexTask + "1\t7001\t" + (11 + i));
                    recipeWriter.WriteLine("" + indexTask + "2\t" + indexState + "0\t1");
                    recipeWriter.WriteLine("" + indexTask + "2\t7001\t" + (11 + i));
                    recipeWriter.WriteLine("" + indexTask + "3\t" + indexState + "0\t1");
                    recipeWriter.WriteLine("" + indexTask + "3\t7002\t" + (11 + i));
                    recipeWriter.WriteLine("" + indexTask + "4\t" + indexState + "0\t1");
                    recipeWriter.WriteLine("" + indexTask + "4\t7002\t" + (11 + i));
                    indexTask++;
                    indexState++;
                    if (d.selectedRecipes[i][j] == Datastructure.Model.Recipes.RecipeData.RECIPE.BLACK || d.selectedRecipes[i][j] == Datastructure.Model.Recipes.RecipeData.RECIPE.YELLOW)
                    {
                        recipeWriter.WriteLine("" + indexTask + "0\t" + indexState + "0\t1");
                        indexTask++;
                        indexState++;
                        recipeWriter.WriteLine("" + indexTask + "0\t" + indexState + "0\t1");
                        indexTask++;
                        indexState++;
                    }
                    else if (d.selectedRecipes[i][j] == Datastructure.Model.Recipes.RecipeData.RECIPE.BLUE || d.selectedRecipes[i][j] == Datastructure.Model.Recipes.RecipeData.RECIPE.RED || d.selectedRecipes[i][j] == Datastructure.Model.Recipes.RecipeData.RECIPE.PURPLE)
                    {
                        recipeWriter.WriteLine("" + indexTask + "0\t" + indexState + "0\t1");
                        indexTask++;
                        indexState++;
                        recipeWriter.WriteLine("" + indexTask + "0\t" + indexState + "0\t1");
                        indexTask++;
                        indexState++;
                    }
                    else if (d.selectedRecipes[i][j] == Datastructure.Model.Recipes.RecipeData.RECIPE.ORANGE || d.selectedRecipes[i][j] == Datastructure.Model.Recipes.RecipeData.RECIPE.GREEN)
                    {
                        recipeWriter.WriteLine("" + indexTask + "1\t" + indexState + "1\t1");
                        recipeWriter.WriteLine("" + indexTask + "2\t" + indexState + "2\t1");
                        indexTask++;
                        indexState++;
                        recipeWriter.WriteLine("" + indexTask + "1\t" + indexState + "1\t1");
                        recipeWriter.WriteLine("" + indexTask + "2\t" + indexState + "2\t1");
                        indexTask++;
                        indexState++;
                        recipeWriter.WriteLine("" + indexTask + "1\t" + indexState + "0\t1");
                        recipeWriter.WriteLine("" + indexTask + "2\t" + indexState + "0\t1");
                        indexTask++;
                        indexState++;
                    }
                    recipeWriter.WriteLine("" + indexTask + "0\t" + indexStateSpezial + "\t1");
                    indexTask++;
                    indexStateSpezial++;
                    recipeWriter.WriteLine("" + indexSpezial1 + "\t" + indexState + "0\t1");
                    recipeWriter.WriteLine("" + indexSpezial1 + "\t7001\t1");
                    indexSpezial1++;
                    recipeWriter.WriteLine("" + indexSpezial1 + "\t" + indexState + "0\t1");
                    recipeWriter.WriteLine("" + indexSpezial1 + "\t7002\t1");
                    indexSpezial1++;
                    recipeWriter.WriteLine("" + indexSpezial2 + "\t" + indexState + "0\t1");
                    recipeWriter.WriteLine("" + indexSpezial2 + "\t7001\t1");
                    recipeWriter.WriteLine("" + indexSpezial2 + "\t7002\t2");
                    indexSpezial2++;
                    recipeWriter.WriteLine("" + indexSpezial2 + "\t" + indexState + "0\t1");
                    recipeWriter.WriteLine("" + indexSpezial2 + "\t7001\t2");
                    recipeWriter.WriteLine("" + indexSpezial2 + "\t7002\t1");
                    indexSpezial2++;
                    indexState++;
                    recipeWriter.WriteLine("" + indexTask + "0\t" + indexState + "0\t1");
                    indexTask++;
                    indexState++;
                }

                recipeWriter.Flush();
                recipeWriter.Close();

            }

            sequencesWriter.Flush();
            sequencesWriter.Close();

            GUI.PCSMainWindow.getInstance().postStatusMessage("RTN-Model for TAOpt written in output directory!");
        }
    }
}
