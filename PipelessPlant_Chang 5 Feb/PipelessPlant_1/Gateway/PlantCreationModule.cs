using System;
using System.Collections.Generic;
using System.Text;

namespace MULTIFORM_PCS.Gateway
{
    class PlantCreationModule
    {
        public Datastructure.Model.Plant createNewPlant()
        {
            #region old;
            ///** Parameters **/
            //Datastructure.Model.Plant myNewPlant = new Datastructure.Model.Plant("test", 2);
            //myNewPlant.theSize = new Datastructure.Model.General.Size(400, 400);
            //myNewPlant.TwoArmStationHoldPositions = new double[] {  0, 10, 20, 30, 40, 50  };
            //myNewPlant.BatStationLoadRate = "0.02";
            //myNewPlant.ChoosenColorIDs = new int[] { 1, 2, 3, 4, 5};
            //myNewPlant.ColFillRate = "0.05";
            //myNewPlant.MixFillRate = "0.02";
            //myNewPlant.RobotBatteryCapacity = "1";
            //myNewPlant.RobotBatteryLossRate = "0.001";
            //myNewPlant.ContainerCuringTimePerVolume = "3";
            //myNewPlant.ContainerMixingTimePerVolume = "1";

            ///** Containers **/
            //myNewPlant.addContainer(0, 0, 300, 1);

            ///** ContainerFake for screenshot! **/
            ///**arena.AllContainers[0].theLayers[0].theIngredients[0] = new PipelessPlant_1.Model.Ingredient(arena.ChoosenColorIDs[0], 5, 1.5, 1.5, Model.MyColors.getInstance().Names[arena.ChoosenColorIDs[0]], 2);
            //arena.AllContainers[0].theLayers[0].theIngredients[1] = new PipelessPlant_1.Model.Ingredient(arena.ChoosenColorIDs[1], 5, 1.5, 1.5, Model.MyColors.getInstance().Names[arena.ChoosenColorIDs[1]], 2);
            //arena.AllContainers[0].theLayers[0].theIngredients[2] = new PipelessPlant_1.Model.Ingredient(arena.ChoosenColorIDs[2], 5, 1.5, 1.5, Model.MyColors.getInstance().Names[arena.ChoosenColorIDs[2]], 2);
            //arena.AllContainers[0].theLayers[0].theIngredients[3] = new PipelessPlant_1.Model.Ingredient(arena.ChoosenColorIDs[3], 5, 1.5, 1.5, Model.MyColors.getInstance().Names[arena.ChoosenColorIDs[3]], 2);
            //arena.AllContainers[0].theLayers[0].theIngredients[4] = new PipelessPlant_1.Model.Ingredient(arena.ChoosenColorIDs[4], 5, 1.5, 1.5, Model.MyColors.getInstance().Names[arena.ChoosenColorIDs[4]], 2);*/


            //myNewPlant.addContainer(0, 0, 300, 2);
            //myNewPlant.addContainer(0, 0, 300, 3);
            //myNewPlant.addContainer(0, 0, 300, 4);
            //myNewPlant.addContainer(0, 0, 300, 5);

            ///** Robots **/
            //myNewPlant.addRobot(1, "red cross", myNewPlant.AllVessels[2], 200, 200, 0, 25, 25, 1);
            //myNewPlant.addRobot(2, "green cross", null, 150, 150, 45, 25, 25, 1);
            ////arena.addRobot(3, "blue dot", null, 275, 275, 45, 25, 25, 1);

            ///** Recipes **/
            //myNewPlant.addRecipe(1, "first recipe", "mix chork", null);
            //myNewPlant.AllRecipes[0].theLayers.Add(new PipelessPlant_1.Datastructure.Model.Recipes.RecipeLayer("light blue"));
            //myNewPlant.AllRecipes[0].theLayers[0].addIngredient(myNewPlant.ChoosenColorIDs[0], 5, 1.5, 1.5, Datastructure.Model.General.MyColors.getInstance().Names[myNewPlant.ChoosenColorIDs[0]], 2);
            //myNewPlant.AllRecipes[0].theLayers[0].addIngredient(myNewPlant.ChoosenColorIDs[1], 5, 1.5, 1.5, Datastructure.Model.General.MyColors.getInstance().Names[myNewPlant.ChoosenColorIDs[1]], 2);
            ////arena.AllRecipes[0].theLayers[0].addIngredient(arena.ChoosenColorIDs[2], 5, 1.5, 1.5, Model.MyColors.getInstance().Names[arena.ChoosenColorIDs[2]], 2);

            //myNewPlant.addRecipe(2, "second recipe", "mix chork", null);
            //myNewPlant.AllRecipes[1].theLayers.Add(new PipelessPlant_1.Datastructure.Model.Recipes.RecipeLayer("light blue"));
            //myNewPlant.AllRecipes[1].theLayers[0].addIngredient(myNewPlant.ChoosenColorIDs[0], 5, 1.5, 1.5, Datastructure.Model.General.MyColors.getInstance().Names[myNewPlant.ChoosenColorIDs[0]], 2);
            //myNewPlant.AllRecipes[1].theLayers[0].addIngredient(myNewPlant.ChoosenColorIDs[1], 5, 1.5, 1.5, Datastructure.Model.General.MyColors.getInstance().Names[myNewPlant.ChoosenColorIDs[1]], 2);

            ///** Recipe with 5 Layers and full of Colors for screenshots :) **/
            //myNewPlant.addRecipe(3, "third recipe", "mix chork", null);
            //myNewPlant.AllRecipes[2].theLayers.Add(new PipelessPlant_1.Datastructure.Model.Recipes.RecipeLayer("light blue"));
            //myNewPlant.AllRecipes[2].theLayers[0].addIngredient(myNewPlant.ChoosenColorIDs[0], 4, 1.5, 1.5, Datastructure.Model.General.MyColors.getInstance().Names[myNewPlant.ChoosenColorIDs[0]], 2);
            //myNewPlant.AllRecipes[2].theLayers[0].addIngredient(myNewPlant.ChoosenColorIDs[1], 4, 1.5, 1.5, Datastructure.Model.General.MyColors.getInstance().Names[myNewPlant.ChoosenColorIDs[1]], 2);
            //myNewPlant.AllRecipes[2].theLayers[0].addIngredient(myNewPlant.ChoosenColorIDs[2], 4, 1.5, 1.5, Datastructure.Model.General.MyColors.getInstance().Names[myNewPlant.ChoosenColorIDs[2]], 2);


            //myNewPlant.addRecipe(4, "third recipe", "mix chork", null);
            //myNewPlant.addRecipe(5, "third recipe", "mix chork", null);
            ////arena.AllRecipes[2].theLayers.Add(new PipelessPlant_1.Model.RecipeLayer("light blue"));
            ////arena.AllRecipes[2].theLayers[0].addIngredient(arena.ChoosenColorIDs[0], 4, 1.5, 1.5, Model.MyColors.getInstance().Names[arena.ChoosenColorIDs[0]], 2);
            ////arena.AllRecipes[2].theLayers[0].addIngredient(arena.ChoosenColorIDs[1], 3, 1.5, 1.5, Model.MyColors.getInstance().Names[arena.ChoosenColorIDs[1]], 2);
            ////arena.AllRecipes[2].theLayers[0].addIngredient(arena.ChoosenColorIDs[2], 3, 1.5, 1.5, Model.MyColors.getInstance().Names[arena.ChoosenColorIDs[2]], 2);

            ///** Stations **/
            //    /** Color **/
            //myNewPlant.addStation(new Datastructure.Model.Stations.ColorStation(1, "blue", 330, 335, 40, 40, 270, 0, 0, 0, null, 2, 0.5));
            //myNewPlant.addStation(new Datastructure.Model.Stations.ColorStation(2, "white", 350, 155, 40, 40, 180, 0, 0, 0, null, 1, 0.5));
            //myNewPlant.addStation(new Datastructure.Model.Stations.ColorStation(3, "red", 350, 50, 40, 40, 180, 150, 150, 90, myNewPlant.AllVessels[1], 3, 0.5));
            //Datastructure.Model.Stations.ColorStation col = (Datastructure.Model.Stations.ColorStation)myNewPlant.AllStations[2];
            //    /** Battery **/
            ////myNewPlant.addStation(new Datastructure.Model.Stations.BatteryStation(0.5, 4, "load", 50, 350, 20, 20, 270, 0, 0, 0));
            //    /** Mixing **/
            //myNewPlant.addStation(new Datastructure.Model.Stations.MixingStation(5, "mix1", 50, 50, 40, 40, 45, 0, 0, 0, 0, 5, new double[] { 0, 5, 10, 20, 25 }, null, false, 0.05, 10));
            //    /** TwoArm **/
            //myNewPlant.addStation(new Datastructure.Model.Stations.StorageStation(6, "two1", 50, 200, 40, 40, 0, 0, 0, 0, 10, 10, new Datastructure.Model.Vessel.Vessel[] { null, null, myNewPlant.AllVessels[3], null, myNewPlant.AllVessels[4], myNewPlant.AllVessels[0] }, new double[] { 0, 10, 20, 30, 40, 50 }, new double[] { 5, 5 }, new double[] { 0, 5, 10, 15, 35 }));
            
            
            //for (int i = 0; i < myNewPlant.AllStations.Count; i++)
            //{
            //    myNewPlant.computeNormalDockPosition(myNewPlant.AllStations[i]);
            //}
            #endregion;

            #region PGWS1011;
            /** Parameters **/
            Datastructure.Model.Plant plant = new Datastructure.Model.Plant("PGWS1011", 1);
            plant.StorageStationHoldPositions = new double[] { 0, -33, -23, -13, 13, 23, 33 };
            plant.BatStationLoadRate = "0.02";
            plant.ChoosenColorIDs = new int[] { 0,1,2,3 };//Yellow, Black, Red, Blue(no other colors possible, only combinations)
            plant.ColFillRate = "0.5";
            plant.MixFillRate = "0.25";
            plant.AGVBatteryCapacity = "1";
            plant.AGVBatteryLossRate = "0.001";
            plant.VesselCuringTimePerVolume = "3";
            plant.VesselMixingTimePerVolume = "1";
            plant.theSize = new Datastructure.Model.General.Size(300, 400);

            /** Containers **/
            plant.addVessel(0, 0, 300, 1);
            plant.addVessel(0, 0, 300, 2);
            plant.addVessel(0, 0, 300, 3);
            plant.addVessel(0, 0, 300, 4);
            plant.addVessel(0, 0, 300, 5);
            plant.addVessel(0, 0, 300, 6);

            /** Robots **/
            plant.addRobot(0, null, 1300, 50, 180, 33, 1);
            plant.addRobot(1, null, 1300, 100, 180, 33, 1);
            plant.addRobot(2, null, 1300, 150, 180, 33, 1);
            plant.addRobot(3, null, 1300, 200, 180, 33, 1);
            plant.addRobot(4, null, 1300, 250, 180, 33, 1);

            /** Recipes **/


            /** Stations **/
            /** Color **/
            plant.addStation(new Datastructure.Model.Stations.FillingStation(1, "Yellow and Black", 33, 150, 70.71, 70.71, 0, 60, 150, 180, null, 0,1, 0.5, 0.5));
            plant.addStation(new Datastructure.Model.Stations.FillingStation(3, "Red and Blue", 367, 150, 70.71, 70.71, 180, 340, 150, 0, null, 2,3, 0.5, 0.5));
            /** Battery **/
            plant.addStation(new Datastructure.Model.Stations.ChargingStation(0.5, 4, "load1", 72, 220, 45, 45, 315, 72, 230, 135));
            plant.addStation(new Datastructure.Model.Stations.ChargingStation(0.5, 7, "load2", 328, 220, 45, 45, 225, 328, 230, 45));
            /** Mixing **/
            plant.addStation(new Datastructure.Model.Stations.MixingStation(5, "mix1", 200,10 , 70.71, 70.71, 90, 200, 50, 270, 3, 5, new double[] { 0, 5, 10, 20, 25 }, null, false, 0.05, 10));
            /** TwoArm **/
            plant.addStation(new Datastructure.Model.Stations.StorageStation(6, "two1",170, 319.5,150 , 50,270,200,250,90, 5, 4, new Datastructure.Model.Vessel.Vessel[] { null, plant.AllVessels[2], plant.AllVessels[3], plant.AllVessels[1], plant.AllVessels[4], plant.AllVessels[0], plant.AllVessels[5] }, plant.StorageStationHoldPositions, new double[] { 5, 5 }, new double[] { 0, 5, 10, 15, 35 }));


            for (int i = 0; i < plant.AllStations.Count; i++)
            {
                plant.computeNormalDockPosition(plant.AllStations[i]);
            }
            #endregion;
            return plant;
        }
    }
}
