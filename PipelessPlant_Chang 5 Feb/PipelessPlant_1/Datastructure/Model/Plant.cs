using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace MULTIFORM_PCS.Datastructure.Model
{
    public class Plant
    {
        private double toRad = Math.PI / 180;

        #region parameterList;
        private double[] storageStationHoldPositions;
        public double[] StorageStationHoldPositions
        {
            get { return storageStationHoldPositions; }
            set { storageStationHoldPositions = value; }
        }
        public double batStationLoadRateInDouble()
        {
            return Convert.ToDouble(batStationLoadRate);
        }
        private String batStationLoadRate;
        public String BatStationLoadRate
        {
            get { return batStationLoadRate; }
            set { batStationLoadRate = value; }
        }
        private String colFillRate;
        public String ColFillRate
        {
            get { return colFillRate; }
            set { colFillRate = value; }
        }
        private String mixFillRate;
        public String MixFillRate
        {
            get { return mixFillRate; }
            set { mixFillRate = value; }
        }
        private String agvBatteryLossRate;
        public String AGVBatteryLossRate
        {
            get { return agvBatteryLossRate; }
            set { agvBatteryLossRate = value; }
        }
        private String vesselCuringTimePerVolume;
        public String VesselCuringTimePerVolume
        {
            get { return vesselCuringTimePerVolume; }
            set { vesselCuringTimePerVolume = value; }
        }
        private String vesselMixingTimePerVolume;
        public String VesselMixingTimePerVolume
        {
            get { return vesselMixingTimePerVolume; }
            set { vesselMixingTimePerVolume = value; }
        }
        private String agvBatteryCapacity;
        public String AGVBatteryCapacity
        {
            get { return agvBatteryCapacity; }
            set { agvBatteryCapacity = value; }
        }
        private int[] choosenColorIDs;
        public int[] ChoosenColorIDs
        {
            get { return choosenColorIDs; }
            set { choosenColorIDs = value; }
        }
        private double curSimTime;
        public double theCurSimTime
        {
            get { return curSimTime; }
            set { curSimTime = value; }
        }
        private double samplingRate;
        public double theSamplingRate
        {
            get { return samplingRate; }
            set { samplingRate = value; }
        }
        private double port;
        public double thePort
        {
            get { return port; }
            set { port = value; }
        }
        private String ipAdress;
        public String theIpAdress
        {
            get { return ipAdress; }
            set { ipAdress = value; }
        }
        #endregion;

        private Recipes.RecipeData recipeData;
        public Recipes.RecipeData RecipeData
        {
            get { return recipeData; }
            set { recipeData = value; }
        }
        private String name;
        public String theName
        {
            get { return name; }
            set { name = value; }
        }
        private List<Stations.AbstractStation> allStations;
        public List<Stations.AbstractStation> AllStations
        {
            get { return allStations; }
            set { allStations = value; }
        }
        private ObservableCollection<AGV.AGV> agvs;
        public ObservableCollection<AGV.AGV> AGVs
        {
            get { return agvs; }
            set { agvs = value; }
        }
        private List<AGV.AGV> allAGVs;
        public List<AGV.AGV> AllAGVs
        {
            get { return allAGVs; }
            set { allAGVs = value; }
        }
        private List<Recipes.Recipe> allRecipes;
        public List<Recipes.Recipe> AllRecipes
        {
            get { return allRecipes; }
            set { allRecipes = value; }
        }
        private List<Vessel.Vessel> allVessels;
        public List<Vessel.Vessel> AllVessels
        {
            get { return allVessels; }
            set { allVessels = value; }
        }
        private double scaling;
        public double theScaling
        {
            get { return scaling; }
            set { scaling = value; }
        }
        private General.Size size;
        public General.Size theSize
        {
            get { return size; }
            set { size = value; }
        }

        public Plant(String name, double scaling)
        {
            this.name = name;
            this.scaling = scaling;
            allRecipes = new List<Recipes.Recipe>();
            allStations = new List<Stations.AbstractStation>();
            allAGVs = new List<AGV.AGV>();
            allVessels = new List<Vessel.Vessel>();
            agvs = new ObservableCollection<AGV.AGV>();
            choosenColorIDs = new int[5];
            recipeData = new Recipes.RecipeData(6);
        }

        public Plant()
        {
            allRecipes = new List<Recipes.Recipe>();
            allStations = new List<Stations.AbstractStation>();
            allAGVs = new List<AGV.AGV>();
            allVessels = new List<Vessel.Vessel>();
            agvs = new ObservableCollection<AGV.AGV>();
            choosenColorIDs = new int[5];
            recipeData = new Recipes.RecipeData(6);
        }

        public void addRobot(int id, Vessel.Vessel vessel, double x, double y, double rotation, double diameter, double batteryLoad)
        {
            allAGVs.Add(new AGV.AGV(id, vessel, x, y,rotation, diameter, batteryLoad));
            agvs.Add(new AGV.AGV(id, vessel, x, y, rotation, diameter, batteryLoad));
        }
        public void removeRobot(int id)
        {
            for (int i = 0; i < allAGVs.Count; i++)
            {
                if (allAGVs[i].Id == id)
                {
                    allAGVs.RemoveAt(i);
                    agvs.RemoveAt(i);
                    return;
                }
            }
        }

        public void addStation(Stations.AbstractStation station)
        {
            allStations.Add(station);
        }
        public void removeStation(int id)
        {
            for (int i = 0; i < allStations.Count; i++)
            {
                if (allStations[i].theId == id)
                {
                    allStations.RemoveAt(i);
                    return;
                }
            }
        }

        public void addRecipe(int id, String name, String description, List<Recipes.Ingredient> ingred)
        {
            allRecipes.Add(new Recipes.Recipe(id, name, description));
        }
        public void removeRecipe(int id)
        {
            for (int i = 0; i < allRecipes.Count; i++)
            {
                if (allRecipes[i].theId == id)
                {
                    allRecipes.RemoveAt(i);
                    return;
                }
            }
        }

        public void addVessel(double curFillWater, double curFillHard, double maxVolume, int id )
        {
            allVessels.Add(new Vessel.Vessel(curFillWater, curFillHard, maxVolume, id, choosenColorIDs));
        }
        public void removeVessel(int id)
        {
            for (int i = 0; i < allVessels.Count; i++)
            {
                if (allVessels[i].theId == id)
                {
                    allVessels.RemoveAt(i);
                    return;
                }
            }
        }

        public int getBattStatCount()
        {
            int count = 0;
            for (int i = 0; i < allStations.Count; i++)
            {
                if (allStations[i].isChargingStation())
                {
                    count++;
                }
            }
            return count;
        }
        public int getMixStatCount()
        {
            int count = 0;
            for (int i = 0; i < allStations.Count; i++)
            {
                if (allStations[i].isMixingStation())
                {
                    count++;
                }
            }
            return count;
        }
        public int getColorStatCount()
        {
            int count = 0;
            for (int i = 0; i < allStations.Count; i++)
            {
                if (allStations[i].isFillingStation())
                {
                    count++;
                }
            }
            return count;
        }
        public int getStorageStationCount()
        {
            int count = 0;
            for (int i = 0; i < allStations.Count; i++)
            {
                if (allStations[i].isStorageStation())
                {
                    count++;
                }
            }
            return count;
        }

        public int getPosOfRobot(int id)
        {
            for (int i = 0; i < allAGVs.Count; i++)
            {
                if (allAGVs[i].Id == id)
                {
                    return i;
                }
            }
            return 0;
        }

        public int getPosOfStation(int id)
        {
            int bat = -1;
            int col = -1;
            int two = -1;
            int mix = -1;
            for (int i = 0; i < allStations.Count; i++)
            {
                if(allStations[i].isChargingStation())
                {
                    bat++;
                    if (allStations[i].theId == id)
                    {
                        return bat;
                    }
                }
                else if (allStations[i].isFillingStation())
                {
                    col++;
                    if (allStations[i].theId == id)
                    {
                        return col;
                    }
                }
                else if (allStations[i].isMixingStation())
                {
                    mix++;
                    if (allStations[i].theId == id)
                    {
                        return mix;
                    }
                }
                else if (allStations[i].isStorageStation())
                {
                    two++;
                    if (allStations[i].theId == id)
                    {
                        return two;
                    }
                }
            }
            return 0;
        }

        public Vessel.Vessel getContainer(int id)
        {
            for (int i = 0; i < allVessels.Count; i++)
            {
                if (allVessels[i].theId == id)
                {
                    return allVessels[i];
                }
            }
            return null;
        }

        public void computeNormalDockPosition(Stations.AbstractStation stat)
        {
            stat.theDockingRot = (stat.theRotation + 180) % 360;
            stat.theDockingPos.X = (AllAGVs[0].Diameter / 2 + 5) * Math.Cos(stat.theRotation * toRad);
            stat.theDockingPos.Y = (AllAGVs[0].Diameter / 2 + 5) * Math.Sin(stat.theRotation * toRad);
            stat.theDockingPos.X += stat.thePosition.X;
            stat.theDockingPos.Y += stat.thePosition.Y;
        }

        public AGV.AGV getAGV(int id)
        {
            for (int i = 0; i < AllAGVs.Count; i++)
            {
                if (AllAGVs[i].Id == id)
                {
                    return AllAGVs[i];
                }
            }
            return null;
        }

        public Stations.StorageStation getStorageStationWithVessel(int vesselID)
        {
            for (int i = 0; i < AllStations.Count; i++)
            {
                if (allStations[i].isStorageStation())
                {
                    Stations.StorageStation storage = (Stations.StorageStation)allStations[i];
                    for (int j = 0; j < storage.theVessels.Length; j++)
                    {
                        if (storage.theVessels[j] != null && storage.theVessels[j].theId == vesselID)
                        {
                            return storage;
                        }
                    }
                }
            }
            return null;
        }

        public int getPosOfConInTwoArm(int containerID, Stations.StorageStation two)
        {
            for (int i = 0; i < two.theVessels.Length; i++)
            {
                if (two.theVessels[i] != null && two.theVessels[i].theId == containerID)
                {
                    return (i+1);
                }
            }
            return 0;
        }
        public Stations.AbstractStation getStation(int id)
        {
            for (int i = 0; i < allStations.Count; i++)
            {
                if (allStations[i].theId == id)
                {
                    return allStations[i];
                }
            }
            return null;
        }
        public Stations.AbstractStation getStation(Datastructure.Model.General.Position dockPos)
        {
            for (int i = 0; i < allStations.Count; i++)
            {
                if (allStations[i].theDockingPos.X == dockPos.X && allStations[i].theDockingPos.Y == dockPos.Y)
                {
                    return allStations[i];
                }
            }
            return null;
        }

        public int getContainerCMDID(int conID)
        {
            foreach (AGV.AGV rob in allAGVs)
            {
                if (rob.theVessel != null && rob.theVessel.theId == conID)
                {
                    return rob.Id;
                }
            }
            foreach (Stations.AbstractStation stat in allStations)
            {
                if (stat.isFillingStation())
                {
                    Stations.FillingStation col = (Stations.FillingStation)stat;
                    if (col.theCurContainer != null && col.theCurContainer.theId == conID)
                    {
                        return col.theId;
                    }
                }
                else if (stat.isMixingStation())
                {
                    Stations.MixingStation mix = (Stations.MixingStation)stat;
                    if (mix.theCurrentVessel != null && mix.theCurrentVessel.theId == conID)
                    {
                        return mix.theId;
                    }
                }
                else if (stat.isStorageStation())
                {
                    Stations.StorageStation two = (Stations.StorageStation)stat;
                    for (int i = 0; i < two.theVessels.Length; i++)
                    {
                        if (two.theVessels[i] != null && two.theVessels[i].theId == conID)
                        {
                            return two.theId;
                        }
                    }
                }
            }
            return 0;
        }

        public List<Stations.ChargingStation> getBatteryStations()
        {
            List<Stations.ChargingStation> bats = new List<Stations.ChargingStation>();
            for (int i = 0; i < allStations.Count; i++)
            {
                if (allStations[i].isChargingStation())
                {
                    bats.Add((Stations.ChargingStation)allStations[i]);
                }
            }
            return bats;
        }
        public List<Stations.MixingStation> getMixingStations()
        {
            List<Stations.MixingStation> mixs = new List<Stations.MixingStation>();
            for (int i = 0; i < allStations.Count; i++)
            {
                if (allStations[i].isMixingStation())
                {
                    mixs.Add((Stations.MixingStation)allStations[i]);
                }
            }
            return mixs;
        }
        public List<Stations.FillingStation> getColorStations()
        {
            List<Stations.FillingStation> cols = new List<Stations.FillingStation>();
            for (int i = 0; i < allStations.Count; i++)
            {
                if (allStations[i].isFillingStation())
                {
                    cols.Add((Stations.FillingStation)allStations[i]);
                }
            }
            return cols;
        }
        public List<Stations.StorageStation> getStorageStations()
        {
            List<Stations.StorageStation> twos = new List<Stations.StorageStation>();
            for (int i = 0; i < allStations.Count; i++)
            {
                if (allStations[i].isStorageStation())
                {
                    twos.Add((Stations.StorageStation)allStations[i]);
                }
            }
            return twos;
        }
        public double getDistanceOfStations(int stationID1, int stationID2)
        {
            General.Position pos1 = getStation(stationID1).theDockingPos;
            General.Position pos2 = getStation(stationID2).theDockingPos;
            return Math.Sqrt(Math.Pow((pos1.X - pos2.X),2) + Math.Pow((pos1.Y - pos2.Y),2));
        }
    }
}
