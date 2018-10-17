using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MULTIFORM_PCS.Gateway
{
    class LoadSaveModule
    {
        #region singleton pattern
        private static LoadSaveModule theErstellungsModul;
        private LoadSaveModule() { }
        public static LoadSaveModule getInstance()
        {
            if (theErstellungsModul == null)
            {
                theErstellungsModul = new LoadSaveModule();
            }
            return theErstellungsModul;
        }
        #endregion

        public void writeModelToModelica(String filename, bool oldversion)
        {
            TextWriter modelicaWriter = null;
            if (!oldversion)
            {
                modelicaWriter = new StreamWriter(filename + ".mo");
            }
            else
            {
                modelicaWriter = new StreamWriter(filename + "_modelica2.mo");
            }
            Datastructure.Model.Plant plant = ObserverModule.getInstance().getCurrentPlant();

            modelicaWriter.WriteLine("package " + plant.theName + " \"the pipeless plant representation\"");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("  connector CSharpInput = input Modelica.Blocks.Interfaces.RealInput;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine();
            #region robot;
            modelicaWriter.WriteLine("  model Robot \"the robot going to walk\"");
            modelicaWriter.WriteLine("    parameter String name;");
            modelicaWriter.WriteLine("    parameter Integer id;");
            modelicaWriter.WriteLine("    parameter Real length;");
            modelicaWriter.WriteLine("    parameter Real width;");
            modelicaWriter.WriteLine("    parameter Real batteryCapacity=" + plant.AGVBatteryCapacity + ";");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("    Real speed \"speed in cm per second\";");
            modelicaWriter.WriteLine("    Real rotation;");
            modelicaWriter.WriteLine("    Real realRotation(start = 0);");
            modelicaWriter.WriteLine("    Real rotationSpeed \"speed of the rotation in cm per second\";");
            modelicaWriter.WriteLine("    Real positionX;");
            modelicaWriter.WriteLine("    Real positionY;");
            modelicaWriter.WriteLine("    Integer container;");
            modelicaWriter.WriteLine("    Real battery;");
            modelicaWriter.WriteLine("    Real nonDiscContainer(start=0);");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("    CSharpInput posX;");
            modelicaWriter.WriteLine("    CSharpInput posY;");
            modelicaWriter.WriteLine("    CSharpInput rot;");
            modelicaWriter.WriteLine("    CSharpInput con;");
            modelicaWriter.WriteLine("    CSharpInput bat;");
            modelicaWriter.WriteLine("  equation");
            modelicaWriter.WriteLine("    der(positionX) = speed * Modelica.Math.cos(Modelica.SIunits.Conversions.from_deg(realRotation));");
            modelicaWriter.WriteLine("    der(positionY) = speed * Modelica.Math.sin(Modelica.SIunits.Conversions.from_deg(realRotation));");
            modelicaWriter.WriteLine("    der(rotation) = rotationSpeed;");
            modelicaWriter.WriteLine("    realRotation = mod(rotation,360);");
            modelicaWriter.WriteLine("    der(nonDiscContainer) = if (container > nonDiscContainer and (container-nonDiscContainer) > 0.05) then 5 else if (container < nonDiscContainer and (nonDiscContainer-container) > 0.05) then -5 else 0;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("    posX = positionX;");
            modelicaWriter.WriteLine("    posY = positionY;");
            modelicaWriter.WriteLine("    bat = battery;");
            modelicaWriter.WriteLine("    rot = realRotation;");
            modelicaWriter.WriteLine("    con = nonDiscContainer;");
            modelicaWriter.WriteLine("  end Robot;");
            modelicaWriter.WriteLine();
            #endregion;
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine();
            #region stations;
            modelicaWriter.WriteLine("   partial model AbstractStation");
            modelicaWriter.WriteLine("     parameter Real dockingX;");
            modelicaWriter.WriteLine("     parameter Real dockingY;");
            modelicaWriter.WriteLine("     parameter Real dockingRot;");
            modelicaWriter.WriteLine("     parameter Integer id;");
            modelicaWriter.WriteLine("     parameter String name;");
            modelicaWriter.WriteLine("     parameter Real positionX;");
            modelicaWriter.WriteLine("     parameter Real positionY;");
            modelicaWriter.WriteLine("     parameter Real rotation;");
            modelicaWriter.WriteLine("     parameter Real length;");
            modelicaWriter.WriteLine("     parameter Real width;");
            modelicaWriter.WriteLine("   end AbstractStation;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("   model BatteryStation");
            modelicaWriter.WriteLine("     extends AbstractStation;");
            modelicaWriter.WriteLine("     Real loadRate;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("     CSharpInput rat;");
            modelicaWriter.WriteLine("   equation");
            modelicaWriter.WriteLine("     rat = loadRate;");
            modelicaWriter.WriteLine("   end BatteryStation;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("   model BatteryStationController ");
            modelicaWriter.WriteLine("     parameter Real loadRate = " + plant.BatStationLoadRate + ";");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("     Real instruction;");
            modelicaWriter.WriteLine("   end BatteryStationController;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("   partial model OneArmStation");
            modelicaWriter.WriteLine("     extends AbstractStation;");
            modelicaWriter.WriteLine("     Real altitude;");
            modelicaWriter.WriteLine("     Real armVelocity;");
            modelicaWriter.WriteLine("   equation");
            modelicaWriter.WriteLine("     der(altitude) = armVelocity;");
            modelicaWriter.WriteLine("   end OneArmStation;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("   model TwoArmStation ");
            modelicaWriter.WriteLine("     extends AbstractStation;");
            modelicaWriter.WriteLine("     Real[2] armVelocity;");
            modelicaWriter.WriteLine("     Integer[7] containerID;");
            modelicaWriter.WriteLine("     Real altitude;");
            modelicaWriter.WriteLine("     Real traverse;");
            modelicaWriter.WriteLine("     Real[7] nonDiscContainerID;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("     CSharpInput alt;");
            modelicaWriter.WriteLine("     CSharpInput tra;");
            modelicaWriter.WriteLine("     CSharpInput[7] con;");
            modelicaWriter.WriteLine("   equation ");
            modelicaWriter.WriteLine("     der(altitude) = armVelocity[1];");
            modelicaWriter.WriteLine("     der(traverse) = armVelocity[2];");
            modelicaWriter.WriteLine("     for i in 1 : 7 loop");
            modelicaWriter.WriteLine("       der(nonDiscContainerID[i]) = if(containerID[i] > nonDiscContainerID[i] and (containerID[i]-nonDiscContainerID[i]) > 0.05) then 5 else if(containerID[i] < nonDiscContainerID[i] and (nonDiscContainerID[i]-containerID[i]) > 0.05) then -5 else 0;");
            modelicaWriter.WriteLine("     end for;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("     for i in 1 : 7 loop");
            modelicaWriter.WriteLine("       con[i] = nonDiscContainerID[i];");
            modelicaWriter.WriteLine("     end for;");
            modelicaWriter.WriteLine("     alt = altitude;");
            modelicaWriter.WriteLine("     tra = traverse;");
            modelicaWriter.WriteLine("   end TwoArmStation;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("   model TwoArmStationController ");
            modelicaWriter.WriteLine("     parameter Real[5] positions;");
            modelicaWriter.WriteLine("     parameter Real[7] holdPositions = {" + plant.StorageStationHoldPositions[0] + ", " + plant.StorageStationHoldPositions[1] + ", " + plant.StorageStationHoldPositions[2] + ", " + plant.StorageStationHoldPositions[3] + ", " + plant.StorageStationHoldPositions[4] + ", " + plant.StorageStationHoldPositions[5] + ", " + plant.StorageStationHoldPositions[6] + "};");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("     Real[3] instruction;");
            modelicaWriter.WriteLine("     Real[2] armVelocity;");
            modelicaWriter.WriteLine("   end TwoArmStationController;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("   model ColorStation ");
            modelicaWriter.WriteLine("     extends AbstractStation;");
            modelicaWriter.WriteLine("     parameter Integer colorID_1;");
            modelicaWriter.WriteLine("     parameter Integer colorID_2;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("     Real fillRate_1;");
            modelicaWriter.WriteLine("     Real fillRate_2;");
            modelicaWriter.WriteLine("     Integer containerID;");
            modelicaWriter.WriteLine("     Real nonDiscContainerID(start=0);");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("     CSharpInput rat_1;");
            modelicaWriter.WriteLine("     CSharpInput rat_2;");
            modelicaWriter.WriteLine("     CSharpInput con;");
            modelicaWriter.WriteLine("   equation ");
            modelicaWriter.WriteLine("     der(nonDiscContainerID) = if(containerID > nonDiscContainerID and (containerID-nonDiscContainerID) >0.05) then 5 else if(containerID < nonDiscContainerID and (nonDiscContainerID-containerID) > 0.05) then -5 else 0;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("     con = nonDiscContainerID;");
            modelicaWriter.WriteLine("     rat_1 = fillRate_1;");
            modelicaWriter.WriteLine("     rat_2 = fillRate_2;");
            modelicaWriter.WriteLine("   end ColorStation;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("   model ColorStationController ");
            modelicaWriter.WriteLine("     parameter Real fillRate_1;");
            modelicaWriter.WriteLine("     parameter Real fillRate_2;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("     Real[2] instruction;");
            modelicaWriter.WriteLine("   end ColorStationController;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("   model MixingStation ");
            modelicaWriter.WriteLine("     extends AbstractStation;");
            modelicaWriter.WriteLine("     Real fillRate;");
            modelicaWriter.WriteLine("     Integer containerID;");
            modelicaWriter.WriteLine("     Real altitude;");
            modelicaWriter.WriteLine("     Real armVelocity;");
            modelicaWriter.WriteLine("     Real rotating;");
            modelicaWriter.WriteLine("     Real fillStateSensor;");
            modelicaWriter.WriteLine("     Real nonDiscContainerID(start=0);");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("     CSharpInput alt;");
            modelicaWriter.WriteLine("     CSharpInput rot;");
            modelicaWriter.WriteLine("     CSharpInput fillState;");
            modelicaWriter.WriteLine("     CSharpInput con;");
            modelicaWriter.WriteLine("     CSharpInput rat;");
            modelicaWriter.WriteLine("   equation ");
            modelicaWriter.WriteLine("     der(altitude) = armVelocity;");
            modelicaWriter.WriteLine("     der(nonDiscContainerID) = if(containerID > nonDiscContainerID and (containerID-nonDiscContainerID) > 0.05) then 5 else if(containerID < nonDiscContainerID and (nonDiscContainerID-containerID) > 0.05) then -5 else 0;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("     con = nonDiscContainerID;");
            modelicaWriter.WriteLine("     alt = altitude;");
            modelicaWriter.WriteLine("     rot = rotating;");
            modelicaWriter.WriteLine("     fillState = fillStateSensor;");
            modelicaWriter.WriteLine("     rat = fillRate;");
            modelicaWriter.WriteLine("   end MixingStation;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("   model MixingStationController ");
            modelicaWriter.WriteLine("     parameter Real[5] positions;");
            modelicaWriter.WriteLine("     parameter Real[3] fillStateStages = {0, 5, 10};");
            modelicaWriter.WriteLine("     parameter Real fillRate;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("     Real[4] instruction;");
            modelicaWriter.WriteLine("     Real armVelocity;");
            modelicaWriter.WriteLine("   end MixingStationController;");
            #endregion;
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine();
            #region container;
            modelicaWriter.WriteLine("   model Container ");
            modelicaWriter.WriteLine("     parameter Integer id;");
            modelicaWriter.WriteLine("     parameter Real maxVolume;");
            modelicaWriter.WriteLine("     parameter Real curingTimePerVolume = " + plant.VesselCuringTimePerVolume + ";");
            modelicaWriter.WriteLine("     parameter Real mixingTimePerVolume = " + plant.VesselMixingTimePerVolume + ";");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("     Real[5,4] volume;");
            modelicaWriter.WriteLine("     Real[5] mixStartTime;");
            modelicaWriter.WriteLine("     Real[5] curLayerVolume;");
            modelicaWriter.WriteLine("     Real[5] curLayerWater;");
            modelicaWriter.WriteLine("     Real[5] cementVolume;");
            modelicaWriter.WriteLine("     Boolean[5] mixed;");
            modelicaWriter.WriteLine("     Real[5] hard(start={0,0,0,0,0});");
            modelicaWriter.WriteLine("     Integer curWorkingLayer;");
            modelicaWriter.WriteLine("     Real curFillStateHard;");
            modelicaWriter.WriteLine("     Real curFillStateWater;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("     CSharpInput fillHard;");
            modelicaWriter.WriteLine("     CSharpInput fillWater;");
            modelicaWriter.WriteLine("     CSharpInput[5,4] layerVol;");
            modelicaWriter.WriteLine("   equation ");
            modelicaWriter.WriteLine("     curFillStateWater =  curLayerVolume[1] + curLayerVolume[2] + curLayerVolume[3] + curLayerVolume[4] + curLayerVolume[5];");
            modelicaWriter.WriteLine("     curFillStateHard = (if (hard[1] >= 1) then curLayerVolume[1] else 0) + (if (hard[2] >= 1) then curLayerVolume[2] else 0)");
            modelicaWriter.WriteLine("                      + (if (hard[3] >= 1) then curLayerVolume[3] else 0) + (if (hard[4] >= 1) then curLayerVolume[4] else 0)");
            modelicaWriter.WriteLine("                      + (if (hard[5] >= 1) then curLayerVolume[5] else 0);");
            modelicaWriter.WriteLine("     curWorkingLayer = if (hard[5] >= 1) then 6 else if (hard[4] >= 1) then 5 else if (hard[3] >= 1) then 4 else ");
            modelicaWriter.WriteLine("                       if (hard[2] >= 1) then 3 else if (hard[1] >= 1) then 2 else 1;");
            modelicaWriter.WriteLine("     for i in 1:5 loop");
            modelicaWriter.WriteLine("       curLayerVolume[i] = volume[i,1] + volume[i,2] + volume[i,3] + volume[i,4]  + cementVolume[i];");
            modelicaWriter.WriteLine("       curLayerWater[i] = volume[i,1] + volume[i,2] + volume[i,3] + volume[i,4];");
            modelicaWriter.WriteLine("       if (cementVolume[i] > 0 and hard[i] <= 1 and curLayerWater[i] > 0) then");
            modelicaWriter.WriteLine("         der(hard[i]) = 1 / (curingTimePerVolume * curLayerVolume[i]);");
            modelicaWriter.WriteLine("       else");
            modelicaWriter.WriteLine("         der(hard[i]) = 0;");
            modelicaWriter.WriteLine("       end if;");
            modelicaWriter.WriteLine("       if (mixStartTime[i] > (mixingTimePerVolume * curLayerVolume[i])) then");
            modelicaWriter.WriteLine("         mixed[i] = true;");
            modelicaWriter.WriteLine("       else");
            modelicaWriter.WriteLine("         mixed[i] = false;");
            modelicaWriter.WriteLine("       end if;");
            modelicaWriter.WriteLine("       for j in 1:4 loop");
            modelicaWriter.WriteLine("         layerVol[i,j] = volume[i,j];");
            modelicaWriter.WriteLine("       end for;");
            modelicaWriter.WriteLine("     end for;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("     fillHard = curFillStateHard;");
            modelicaWriter.WriteLine("     fillWater = curFillStateWater;");
            modelicaWriter.WriteLine("   end Container;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine();
            #endregion;
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("  model Arena \"the pipelessPlant\"");
            modelicaWriter.WriteLine("    parameter Real batteryLossRate = -" + plant.AGVBatteryLossRate + ";");
            modelicaWriter.WriteLine("    parameter Real arenaWidth = " + plant.theScaling * 200 + ";");
            modelicaWriter.WriteLine("    parameter Real arenaLength = " + plant.theScaling * 200 + ";");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("    " + plant.theName + ".Control.ExternalControl control;");
            modelicaWriter.WriteLine();
            /**
             * the robots 
             */
            for (int i = 0; i < plant.AllAGVs.Count; i++)
            {
                Datastructure.Model.AGV.AGV robo = plant.AllAGVs[i];
                modelicaWriter.WriteLine("    Robot robot" + robo.Id + "(name=\"" + robo.Id + "\", id=" + robo.Id + ", speed(start=0), rotation(start=" + robo.theRotation + "), rotationSpeed(start=0), positionX(start=" + robo.theCurPosition.X + "), positionY(start=" + robo.theCurPosition.Y + "), length=" + robo.Diameter + ", width=" + robo.Diameter + ", container(start=" + ((robo.theVessel != null) ? "" + robo.theVessel.theId : "" + 0) + "), battery(start=" + robo.theBatteryLoad + "));");
                modelicaWriter.WriteLine("    Integer robot" + robo.Id + "ConTmp(start=0);");
            }
            modelicaWriter.WriteLine();
            /**
             * the containers
             */
            for (int i = 0; i < plant.AllVessels.Count; i++)
            {
                Datastructure.Model.Vessel.Vessel cont = plant.AllVessels[i];
                modelicaWriter.WriteLine("    Container con" + cont.theId + "(id=" + cont.theId + ", maxVolume=" + cont.theMaxVolume + ");");
            }
            modelicaWriter.WriteLine();
            /**
             * the stations
             */
            for (int j = 0; j < plant.AllStations.Count; j++)
            {
                Datastructure.Model.Stations.AbstractStation stat = plant.AllStations[j];
                String dockingX = "", dockingY = "";
                dockingX += stat.theDockingPos.X;
                dockingY += stat.theDockingPos.Y;
                dockingX = dockingX.Replace(',', '.');
                dockingY = dockingY.Replace(',', '.');
                if (stat.isChargingStation())
                {
                    Datastructure.Model.Stations.ChargingStation bat = (Datastructure.Model.Stations.ChargingStation)(stat);
                    modelicaWriter.WriteLine("    BatteryStation bat" + bat.theId + "(dockingX=" + dockingX + ", dockingY=" + dockingY + ", dockingRot=" + bat.theDockingRot + ", id=" + bat.theId + ", name=\"" + bat.theName + "\", positionX=" + bat.thePosition.X + ", positionY=" + bat.thePosition.Y + ", rotation=" + bat.theRotation + ", length=" + bat.getLength() + ", width=" + bat.getWidth() + /**", notUseAbleSpace={" + supportPoly + "}*/");");
                    modelicaWriter.WriteLine("    BatteryStationController bat" + bat.theId + "CTRL;");
                }
                else if (stat.isFillingStation())
                {
                    Datastructure.Model.Stations.FillingStation col = (Datastructure.Model.Stations.FillingStation)(stat);
                    modelicaWriter.WriteLine("    ColorStation col" + col.theId + "(dockingX=" + dockingX + ", dockingY=" + dockingY + ", dockingRot=" + col.theDockingRot + ", id=" + col.theId + ", name=\"" + col.theName + "\", positionX=" + col.thePosition.X + ", positionY=" + col.thePosition.Y + ", rotation=" + col.theRotation + ", length=" + col.getLength() + ", width=" + col.getWidth() + ", colorID_1=" + (col.theColorID[0]+1) + ", colorID_2=" + (col.theColorID[1]+1) + ", containerID(start=" + ((col.theCurContainer != null) ? "" + col.theCurContainer.theId : "" + 0) + "));");
                    modelicaWriter.WriteLine("    ColorStationController col" + col.theId + "CTRL(fillRate_1=" + plant.ColFillRate + ", fillRate_2=" + plant.ColFillRate + ");");
                }
                else if (stat.isMixingStation())
                {
                    Datastructure.Model.Stations.MixingStation mix = (Datastructure.Model.Stations.MixingStation)(stat);
                    modelicaWriter.WriteLine("    MixingStation mix" + mix.theId + "(dockingX=" + dockingX + ", dockingY=" + dockingY + ", dockingRot=" + mix.theDockingRot + ", id=" + mix.theId + ", name=\"" + mix.theName + "\", positionX=" + mix.thePosition.X + ", positionY=" + mix.thePosition.Y + ", rotation=" + mix.theRotation + ", length=" + mix.getLength() + ", width=" + mix.getWidth() + ", containerID(start=" + ((mix.theCurrentVessel != null) ? "" + mix.theCurrentVessel.theId : "" + 0) + "));");
                    modelicaWriter.WriteLine("    MixingStationController mix" + mix.theId + "CTRL(positions={" + mix.theArmpositions[0] + "," + mix.theArmpositions[1] + "," + mix.theArmpositions[2] + "," + mix.theArmpositions[3] + "," + mix.theArmpositions[4] + "}, fillRate=" + plant.MixFillRate + ");");
                }
                else if (stat.isStorageStation())
                {
                    Datastructure.Model.Stations.StorageStation two = (Datastructure.Model.Stations.StorageStation)(stat);
                    String containers = "";
                    for (int z = 0; z < two.theVessels.Length; z++)
                    {
                        if (two.theVessels[z] != null && two.theVessels[z].theId != 0)
                        {
                            if (z == 0)
                            {
                                containers += two.theVessels[z].theId;
                            }
                            else
                            {
                                containers += "," + two.theVessels[z].theId;
                            }
                        }
                        else
                        {
                            if (z == 0)
                            {
                                containers += "0";
                            }
                            else
                            {
                                containers += ", 0";
                            }
                        }
                    }
                    modelicaWriter.WriteLine("    TwoArmStation two" + two.theId + "(dockingX=" + dockingX + ", dockingY=" + dockingY + ", dockingRot=" + two.theDockingRot + ", id=" + two.theId + ", name=\"" + two.theName + "\", positionX=" + two.thePosition.X + ", positionY=" + two.thePosition.Y + ", rotation=" + two.theRotation + ", length=" + two.getLength() + ", width=" + two.getWidth() + ", containerID(start ={" + containers + "}));");
                    modelicaWriter.WriteLine("    TwoArmStationController two" + two.theId + "CTRL(positions={" + two.theVArmPositions[0] + "," + two.theVArmPositions[1] + "," + two.theVArmPositions[2] + "," + two.theVArmPositions[3] + "," + two.theVArmPositions[4] + "});");
                }
            }
            modelicaWriter.WriteLine("  equation");
            /**
             * Container, cementVolume, curingStartTime, mixStartTime, Volume
             */
            modelicaWriter.WriteLine("    for i in 1:5 loop");
            for (int i = 0; i < plant.AllVessels.Count; i++)
            {
                modelicaWriter.WriteLine("      der(con" + plant.AllVessels[i].theId + ".cementVolume[i]) = ");
                for (int j = 0; j < plant.AllStations.Count; j++)
                {
                    if (plant.AllStations[j].isMixingStation())
                    {
                        modelicaWriter.WriteLine("        if (mix" + plant.AllStations[j].theId + ".containerID == con" + plant.AllVessels[i].theId + ".id and mix" + plant.AllStations[j].theId + ".fillRate > 0 and i == con" + plant.AllVessels[i].theId + ".curWorkingLayer) then mix" + plant.AllStations[j].theId + ".fillRate else");
                    }
                }
                modelicaWriter.WriteLine("        0;");
                modelicaWriter.WriteLine("      der(con" + plant.AllVessels[i].theId + ".mixStartTime[i]) = ");
                for (int j = 0; j < plant.AllStations.Count; j++)
                {
                    if (plant.AllStations[j].isMixingStation())
                    {
                        modelicaWriter.WriteLine("        if  (mix" + plant.AllStations[j].theId + ".containerID == con" + plant.AllVessels[i].theId + ".id and mix" + plant.AllStations[j].theId + ".rotating > 0) then 0.5 else ");
                    }
                }
                modelicaWriter.WriteLine("        0;");
            }
            modelicaWriter.WriteLine("      for j in 1:4 loop");
            for (int i = 0; i < plant.AllVessels.Count; i++)
            {
                modelicaWriter.WriteLine("        der(con" + plant.AllVessels[i].theId + ".volume[i,j]) = ");
                for (int j = 0; j < plant.AllStations.Count; j++)
                {
                    if (plant.AllStations[j].isFillingStation())
                    {
                        modelicaWriter.WriteLine("            if (col" + plant.AllStations[j].theId + ".containerID == con" + plant.AllVessels[i].theId + ".id and j ==col" + plant.AllStations[j].theId + ".colorID_1 and col" + plant.AllStations[j].theId + ".fillRate_1 > 0 and i == con" + plant.AllVessels[i].theId + ".curWorkingLayer) then col" + plant.AllStations[j].theId + ".fillRate_1 else ");
                        modelicaWriter.WriteLine("              if (col" + plant.AllStations[j].theId + ".containerID == con" + plant.AllVessels[i].theId + ".id and j ==col" + plant.AllStations[j].theId + ".colorID_2 and col" + plant.AllStations[j].theId + ".fillRate_2 > 0 and i == con" + plant.AllVessels[i].theId + ".curWorkingLayer) then col" + plant.AllStations[j].theId + ".fillRate_2 else ");
                    }
                }
                modelicaWriter.WriteLine("              0;");
            }
            modelicaWriter.WriteLine("      end for;");
            modelicaWriter.WriteLine("    end for;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine();
            /**
             * robots, speed, rotspeed, times, container
             */
            for (int i = 0; i < plant.AllAGVs.Count; i++)
            {
                modelicaWriter.WriteLine("    robot" + plant.AllAGVs[i].Id + ".speed = control.digitalOutputs[" + (i * 2 + 1) + "];");
                modelicaWriter.WriteLine("    robot" + plant.AllAGVs[i].Id + ".rotationSpeed = control.digitalOutputs[" + (i * 2 + 2) + "];");
                modelicaWriter.WriteLine("    connect(robot" + plant.AllAGVs[i].Id + ".posX, control.robot" + plant.AllAGVs[i].Id + "X);");
                modelicaWriter.WriteLine("    connect(robot" + plant.AllAGVs[i].Id + ".posY, control.robot" + plant.AllAGVs[i].Id + "Y);");
                modelicaWriter.WriteLine("    connect(robot" + plant.AllAGVs[i].Id + ".rot, control.robot" + plant.AllAGVs[i].Id + "Rot);");
                modelicaWriter.WriteLine("    connect(robot" + plant.AllAGVs[i].Id + ".con, control.robot" + plant.AllAGVs[i].Id + "Con);");
                modelicaWriter.WriteLine("    connect(robot" + plant.AllAGVs[i].Id + ".bat, control.robot" + plant.AllAGVs[i].Id + "Bat);");
                modelicaWriter.WriteLine();
            }
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine();
            for (int j = 0; j < plant.AllAGVs.Count; j++)
            {
                bool firstBat = true;
                modelicaWriter.WriteLine("    der(robot" + plant.AllAGVs[j].Id + ".battery) = if(");
                for (int i = 0; i < plant.AllStations.Count; i++)
                {
                    if (plant.AllStations[i].isChargingStation())
                    {
                        if (!firstBat)
                        {
                            modelicaWriter.WriteLine("    else if(");
                        }
                        modelicaWriter.WriteLine("      robot" + plant.AllAGVs[j].Id + ".posX >= (bat" + plant.AllStations[i].theId + ".dockingX - 2) and robot" + plant.AllAGVs[j].Id + ".posX <= (bat" + plant.AllStations[i].theId + ".dockingX + 2)");
                        modelicaWriter.WriteLine("      and robot" + plant.AllAGVs[j].Id + ".posY >= (bat" + plant.AllStations[i].theId + ".dockingY - 2) and robot" + plant.AllAGVs[j].Id + ".posY <= (bat" + plant.AllStations[i].theId + ".dockingY + 2)");
                        modelicaWriter.WriteLine("      and robot" + plant.AllAGVs[j].Id + ".rot >= (bat" + plant.AllStations[i].theId + ".dockingRot - 5) and robot" + plant.AllAGVs[j].Id + ".rot <= (bat" + plant.AllStations[i].theId + ".dockingRot +5)");
                        modelicaWriter.WriteLine("      and bat" + plant.AllStations[i].theId + "CTRL.instruction >= 0.5 and robot" + plant.AllAGVs[j].Id + ".battery <= robot" + plant.AllAGVs[j].Id + ".batteryCapacity) then");
                        modelicaWriter.WriteLine("        bat" + plant.AllStations[i].theId + ".loadRate");
                        firstBat = false;
                    }
                }
                if (firstBat)
                {
                    modelicaWriter.WriteLine("      true) then 0 else 0;");
                }
                else
                {
                    modelicaWriter.WriteLine("      else if(robot" + plant.AllAGVs[j].Id + ".battery >= 0) then batteryLossRate else 0;");
                }
            }
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine();
            /**
             * the stations, container
             */
            for (int i = 0; i < plant.AllStations.Count; i++)
            {
                Datastructure.Model.Stations.AbstractStation stat = plant.AllStations[i];
                if (stat.isFillingStation())
                {
                    Datastructure.Model.Stations.FillingStation col = (Datastructure.Model.Stations.FillingStation)stat;
                    modelicaWriter.WriteLine("    der(col" + col.theId + ".fillRate_1) = if (col" + col.theId + "CTRL.instruction[1] > 0 and col" + col.theId + ".fillRate_1 <= col" + col.theId + "CTRL.fillRate_1) then 0.5 else ");
                    modelicaWriter.WriteLine("                                         if (col" + col.theId + "CTRL.instruction[1] <= 0 and col" + col.theId + ".fillRate_1 >= 0) then -0.5 else");
                    modelicaWriter.WriteLine("                                         0;");
                    modelicaWriter.WriteLine("    der(col" + col.theId + ".fillRate_2) = if (col" + col.theId + "CTRL.instruction[2] > 0 and col" + col.theId + ".fillRate_2 <= col" + col.theId + "CTRL.fillRate_2) then 0.5 else ");
                    modelicaWriter.WriteLine("                                         if (col" + col.theId + "CTRL.instruction[2] <= 0 and col" + col.theId + ".fillRate_2 >= 0) then -0.5 else");
                    modelicaWriter.WriteLine("                                         0;");
                    modelicaWriter.WriteLine("    connect(col" + plant.AllStations[i].theId + ".con, control.col" + plant.AllStations[i].theId + "Con);");
                    modelicaWriter.WriteLine("    connect(col" + plant.AllStations[i].theId + ".rat_1, control.col" + plant.AllStations[i].theId + "FRate_1);");
                    modelicaWriter.WriteLine("    connect(col" + plant.AllStations[i].theId + ".rat_2, control.col" + plant.AllStations[i].theId + "FRate_2);");
                }
                else if (stat.isMixingStation())
                {
                    Datastructure.Model.Stations.MixingStation mix = (Datastructure.Model.Stations.MixingStation)stat;
                    modelicaWriter.WriteLine("    der(mix" + mix.theId + ".rotating) = if (mix" + mix.theId + "CTRL.instruction[3] > 0 and mix" + mix.theId + ".rotating <=1 ) then 10 else");
                    modelicaWriter.WriteLine("                                         if (mix" + mix.theId + "CTRL.instruction[3] <= 0 and mix" + mix.theId + ".rotating > 0 ) then -10 else");
                    modelicaWriter.WriteLine("                                         0;");
                    modelicaWriter.WriteLine("    der(mix" + mix.theId + ".fillRate) = if (mix" + mix.theId + "CTRL.instruction[2] > 0 and mix" + mix.theId + ".fillRate <= mix" + mix.theId + "CTRL.fillRate) then 0.01 else ");
                    modelicaWriter.WriteLine("                                         if (mix" + mix.theId + "CTRL.instruction[2] <= 0 and mix" + mix.theId + ".fillRate > 0) then -0.01 else");
                    modelicaWriter.WriteLine("                                         0;");
                    modelicaWriter.WriteLine("    mix" + mix.theId + ".armVelocity = if (mix" + mix.theId + "CTRL.instruction[1] > 0) then");
                    modelicaWriter.WriteLine("                       (if(mix" + mix.theId + "CTRL.armVelocity > 0 and mix" + mix.theId + ".altitude < (mix" + mix.theId + "CTRL.positions[integer(mix" + mix.theId + "CTRL.instruction[1])]-0.01)) then mix" + mix.theId + "CTRL.armVelocity else ");
                    modelicaWriter.WriteLine("                       if (mix" + mix.theId + "CTRL.armVelocity < 0 and mix" + mix.theId + ".altitude > (mix" + mix.theId + "CTRL.positions[integer(mix" + mix.theId + "CTRL.instruction[1])]+0.01)) then mix" + mix.theId + "CTRL.armVelocity else ");
                    modelicaWriter.WriteLine("                       0) else 0;");
                    modelicaWriter.WriteLine("    connect(mix" + plant.AllStations[i].theId + ".con, control.mix" + plant.AllStations[i].theId + "Con);");
                    modelicaWriter.WriteLine("    connect(mix" + plant.AllStations[i].theId + ".alt, control.mix" + plant.AllStations[i].theId + "Alt);");
                    modelicaWriter.WriteLine("    connect(mix" + plant.AllStations[i].theId + ".rot, control.mix" + plant.AllStations[i].theId + "RRate);");
                    modelicaWriter.WriteLine("    connect(mix" + plant.AllStations[i].theId + ".fillState, control.mix" + plant.AllStations[i].theId + "Sensor);");
                    modelicaWriter.WriteLine("    connect(mix" + plant.AllStations[i].theId + ".rat, control.mix" + plant.AllStations[i].theId + "FRate);");
                }
                else if (stat.isStorageStation())
                {
                    Datastructure.Model.Stations.StorageStation two = (Datastructure.Model.Stations.StorageStation)stat;
                    modelicaWriter.WriteLine("    two" + two.theId + ".armVelocity[1] = if (two" + two.theId + "CTRL.instruction[1] > 0) then");
                    modelicaWriter.WriteLine("                          (if(two" + two.theId + "CTRL.armVelocity[1] > 0 and two" + two.theId + ".altitude < (two" + two.theId + "CTRL.positions[integer(two" + two.theId + "CTRL.instruction[1])]-0.01)) then two" + two.theId + "CTRL.armVelocity[1] else ");
                    modelicaWriter.WriteLine("                          if (two" + two.theId + "CTRL.armVelocity[1] < 0 and two" + two.theId + ".altitude > (two" + two.theId + "CTRL.positions[integer(two" + two.theId + "CTRL.instruction[1])]+0.01)) then two" + two.theId + "CTRL.armVelocity[1] else ");
                    modelicaWriter.WriteLine("                          0) else 0;");
                    modelicaWriter.WriteLine("    two" + two.theId + ".armVelocity[2] = if (two" + two.theId + "CTRL.instruction[2] > 0) then");
                    modelicaWriter.WriteLine("                          (if(two" + two.theId + ".traverse < (two" + two.theId + "CTRL.holdPositions[integer(two" + two.theId + "CTRL.instruction[2])]-0.01) and two" + two.theId + "CTRL.armVelocity[2] > 0) then two" + two.theId + "CTRL.armVelocity[2] else ");
                    modelicaWriter.WriteLine("                          if (two" + two.theId + ".traverse > (two" + two.theId + "CTRL.holdPositions[integer(two" + two.theId + "CTRL.instruction[2])]+0.01) and two" + two.theId + "CTRL.armVelocity[2] < 0) then two" + two.theId + "CTRL.armVelocity[2] else ");
                    modelicaWriter.WriteLine("                          0) else 0;");
                    modelicaWriter.WriteLine("    connect(two" + plant.AllStations[i].theId + ".con[1], control.two" + plant.AllStations[i].theId + "Con1);");
                    modelicaWriter.WriteLine("    connect(two" + plant.AllStations[i].theId + ".con[2], control.two" + plant.AllStations[i].theId + "Con2);");
                    modelicaWriter.WriteLine("    connect(two" + plant.AllStations[i].theId + ".con[3], control.two" + plant.AllStations[i].theId + "Con3);");
                    modelicaWriter.WriteLine("    connect(two" + plant.AllStations[i].theId + ".con[4], control.two" + plant.AllStations[i].theId + "Con4);");
                    modelicaWriter.WriteLine("    connect(two" + plant.AllStations[i].theId + ".con[5], control.two" + plant.AllStations[i].theId + "Con5);");
                    modelicaWriter.WriteLine("    connect(two" + plant.AllStations[i].theId + ".con[6], control.two" + plant.AllStations[i].theId + "Con6);");
                    modelicaWriter.WriteLine("    connect(two" + plant.AllStations[i].theId + ".con[7], control.two" + plant.AllStations[i].theId + "Con7);");
                    modelicaWriter.WriteLine("    connect(two" + plant.AllStations[i].theId + ".alt, control.two" + plant.AllStations[i].theId + "Alt);");
                    modelicaWriter.WriteLine("    connect(two" + plant.AllStations[i].theId + ".tra, control.two" + plant.AllStations[i].theId + "Tra);");
                }
                else if
                          (stat.isChargingStation())
                {
                    modelicaWriter.WriteLine("    der(bat" + plant.AllStations[i].theId + ".loadRate) = if (bat" + plant.AllStations[i].theId + "CTRL.instruction > 0 and bat" + plant.AllStations[i].theId + ".loadRate <= bat" + plant.AllStations[i].theId + "CTRL.loadRate) then 0.01 else ");
                    modelicaWriter.WriteLine("                                                          if (bat" + plant.AllStations[i].theId + "CTRL.instruction <= 0 and bat" + plant.AllStations[i].theId + ".loadRate >= 0) then -0.01 else 0;");
                    modelicaWriter.WriteLine("    connect(bat" + plant.AllStations[i].theId + ".rat, control.bat" + plant.AllStations[i].theId + "LRate);");
                }
            }
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine();
            /**
             * mixing stations fillStateSensor
             */
            for (int i = 0; i < plant.AllStations.Count; i++)
            {
                if (plant.AllStations[i].isMixingStation())
                {
                    modelicaWriter.WriteLine("     der(mix" + plant.AllStations[i].theId + ".fillStateSensor) = ");
                    for (int j = 0; j < plant.AllVessels.Count; j++)
                    {
                        modelicaWriter.WriteLine("         if(con" + plant.AllVessels[j].theId + ".id == mix" + plant.AllStations[i].theId + ".containerID) then ");
                        modelicaWriter.WriteLine("             (if (mix" + plant.AllStations[i].theId + ".altitude < (mix" + plant.AllStations[i].theId + "CTRL.positions[5] - con" + plant.AllVessels[j].theId + ".curFillStateWater) and mix" + plant.AllStations[i].theId + ".fillStateSensor <= mix" + plant.AllStations[i].theId + "CTRL.fillStateStages[1]) then 0 else ");
                        modelicaWriter.WriteLine("             if (mix" + plant.AllStations[i].theId + ".altitude < (mix" + plant.AllStations[i].theId + "CTRL.positions[5] - con" + plant.AllVessels[j].theId + ".curFillStateWater) and mix" + plant.AllStations[i].theId + ".fillStateSensor >= mix" + plant.AllStations[i].theId + "CTRL.fillStateStages[1]) then -5 else ");
                        modelicaWriter.WriteLine("             if (mix" + plant.AllStations[i].theId + ".altitude > (mix" + plant.AllStations[i].theId + "CTRL.positions[5] - con" + plant.AllVessels[j].theId + ".curFillStateWater) and mix" + plant.AllStations[i].theId + ".altitude < (mix" + plant.AllStations[i].theId + "CTRL.positions[5] - con" + plant.AllVessels[j].theId + ".curFillStateHard) and mix" + plant.AllStations[i].theId + ".fillStateSensor <= mix" + plant.AllStations[i].theId + "CTRL.fillStateStages[2]) then 5 else ");
                        modelicaWriter.WriteLine("             if(mix" + plant.AllStations[i].theId + ".fillStateSensor <= mix" + plant.AllStations[i].theId + "CTRL.fillStateStages[3] and mix" + plant.AllStations[i].theId + ".altitude < (mix" + plant.AllStations[i].theId + "CTRL.positions[5] - con" + plant.AllVessels[j].theId + ".curFillStateHard)) then 5 else 0) ");
                        modelicaWriter.WriteLine("         else");
                    }
                    modelicaWriter.WriteLine("     if(mix" + plant.AllStations[i].theId + ".fillStateSensor > 0) then -5 else 0;");
                }
            }
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine();
            /**
             * the containers, connect
             */
            for (int i = 0; i < plant.AllVessels.Count; i++)
            {
                modelicaWriter.WriteLine("    connect(con" + plant.AllVessels[i].theId + ".fillHard, control.con" + plant.AllVessels[i].theId + "HRate);");
                modelicaWriter.WriteLine("    connect(con" + plant.AllVessels[i].theId + ".fillWater, control.con" + plant.AllVessels[i].theId + "WRate);");
                modelicaWriter.WriteLine("    for i in 1:5 loop");
                modelicaWriter.WriteLine("      for j in 1:4 loop");
                modelicaWriter.WriteLine("        connect(con" + plant.AllVessels[i].theId + ".layerVol[i,j], control.con" + plant.AllVessels[i].theId + "LayerVol[i,j]);");
                modelicaWriter.WriteLine("      end for;");
                modelicaWriter.WriteLine("    end for;");
            }
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine();
            /**
             * the stations CTRL
             */
            int y = 0;
            for (int i = 0; i < plant.AllStations.Count; i++)
            {
                if (plant.AllStations[i].isStorageStation())
                {
                    modelicaWriter.WriteLine("    two" + plant.AllStations[i].theId + "CTRL.instruction[1] = control.digitalOutputs[" + (2 * plant.AllAGVs.Count + y * 5 + 1) + "];");
                    modelicaWriter.WriteLine("    two" + plant.AllStations[i].theId + "CTRL.instruction[2] = control.digitalOutputs[" + (2 * plant.AllAGVs.Count + y * 5 + 2) + "];");
                    modelicaWriter.WriteLine("    two" + plant.AllStations[i].theId + "CTRL.instruction[3] = control.digitalOutputs[" + (2 * plant.AllAGVs.Count + y * 5 + 3) + "];");
                    modelicaWriter.WriteLine("    two" + plant.AllStations[i].theId + "CTRL.armVelocity[1] = control.digitalOutputs[" + (2 * plant.AllAGVs.Count + y * 5 + 4) + "];");
                    modelicaWriter.WriteLine("    two" + plant.AllStations[i].theId + "CTRL.armVelocity[2] = control.digitalOutputs[" + (2 * plant.AllAGVs.Count + y * 5 + 5) + "];");
                    y++;
                }
            }
            y = 0;
            for (int i = 0; i < plant.AllStations.Count; i++)
            {
                if (plant.AllStations[i].isMixingStation())
                {
                    modelicaWriter.WriteLine("    mix" + plant.AllStations[i].theId + "CTRL.instruction[1] = control.digitalOutputs[" + (2 * plant.AllAGVs.Count + plant.getStorageStationCount() * 5 + y * 5 + 1) + "];");
                    modelicaWriter.WriteLine("    mix" + plant.AllStations[i].theId + "CTRL.instruction[2] = control.digitalOutputs[" + (2 * plant.AllAGVs.Count + plant.getStorageStationCount() * 5 + y * 5 + 2) + "];");
                    modelicaWriter.WriteLine("    mix" + plant.AllStations[i].theId + "CTRL.instruction[3] = control.digitalOutputs[" + (2 * plant.AllAGVs.Count + plant.getStorageStationCount() * 5 + y * 5 + 3) + "];");
                    modelicaWriter.WriteLine("    mix" + plant.AllStations[i].theId + "CTRL.instruction[4] = control.digitalOutputs[" + (2 * plant.AllAGVs.Count + plant.getStorageStationCount() * 5 + y * 5 + 4) + "];");
                    modelicaWriter.WriteLine("    mix" + plant.AllStations[i].theId + "CTRL.armVelocity = control.digitalOutputs[" + (2 * plant.AllAGVs.Count + plant.getStorageStationCount() * 5 + y * 5 + 5) + "];");
                    y++;
                }
            }
            y = 0;
            for (int i = 0; i < plant.AllStations.Count; i++)
            {
                if (plant.AllStations[i].isFillingStation())
                {
                    modelicaWriter.WriteLine("    col" + plant.AllStations[i].theId + "CTRL.instruction[1] = control.digitalOutputs[" + (2 * plant.AllAGVs.Count + plant.getStorageStationCount() * 5 + plant.getMixStatCount() * 5 + y * 2 + 1) + "];");
                    modelicaWriter.WriteLine("    col" + plant.AllStations[i].theId + "CTRL.instruction[2] = control.digitalOutputs[" + (2 * plant.AllAGVs.Count + plant.getStorageStationCount() * 5 + plant.getMixStatCount() * 5 + y * 2 + 2) + "];");
                    y++;
                }
            }
            y = 0;
            for (int i = 0; i < plant.AllStations.Count; i++)
            {
                if (plant.AllStations[i].isChargingStation())
                {
                    modelicaWriter.WriteLine("    bat" + plant.AllStations[i].theId + "CTRL.instruction = control.digitalOutputs[" + (2 * plant.AllAGVs.Count + plant.getStorageStationCount() * 5 + plant.getMixStatCount() * 5 + plant.getColorStatCount() * 2 + y + 1) + "];");
                    y++;
                }
            }
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine();
            #region plant_algorithm;
            modelicaWriter.WriteLine("  algorithm");
            /**
             * Container exchange! Cause of algorithm, very slow...
             */
            modelicaWriter.WriteLine("    if(control.sampled) then");
            for (int i = 0; i < plant.AllAGVs.Count; i++)
            {
                int first = 0;
                modelicaWriter.WriteLine("    if (");
                for (int j = 0; j < plant.AllStations.Count; j++)
                {
                    if (plant.AllStations[j].isFillingStation())
                    {
                        //nothing to do here
                    }
                    else if (plant.AllStations[j].isMixingStation())
                    {
                        if (first == 0)
                        {
                            modelicaWriter.WriteLine("      robot" + plant.AllAGVs[i].Id + ".posX >= (mix" + plant.AllStations[j].theId + ".dockingX - 5) and robot" + plant.AllAGVs[i].Id + ".posX <= (mix" + plant.AllStations[j].theId + ".dockingX + 5)");
                            first += 1;
                        }
                        else
                        {
                            modelicaWriter.WriteLine("    elseif (robot" + plant.AllAGVs[i].Id + ".posX >= (mix" + plant.AllStations[j].theId + ".dockingX - 5) and robot" + plant.AllAGVs[i].Id + ".posX <= (mix" + plant.AllStations[j].theId + ".dockingX + 5)");
                        }
                        modelicaWriter.WriteLine("      and robot" + plant.AllAGVs[i].Id + ".posY >= (mix" + plant.AllStations[j].theId + ".dockingY - 5) and robot" + plant.AllAGVs[i].Id + ".posY <= (mix" + plant.AllStations[j].theId + ".dockingY + 5)");
                        modelicaWriter.WriteLine("      and (if(mix" + plant.AllStations[j].theId + ".dockingRot < 4 or mix" + plant.AllStations[j].theId + ".dockingRot > 357) then (if (robot" + plant.AllAGVs[i].Id + ".rot >= 355 or robot" + plant.AllAGVs[i].Id + ".rot <= 5) then true else false)");
                        modelicaWriter.WriteLine("           else (if(robot" + plant.AllAGVs[i].Id + ".rot >= (mix" + plant.AllStations[j].theId + ".dockingRot - 5) and robot" + plant.AllAGVs[i].Id + ".rot <= (mix" + plant.AllStations[j].theId + ".dockingRot +5)) then true else false))");
                        modelicaWriter.WriteLine("      and mix" + plant.AllStations[j].theId + ".altitude <= mix" + plant.AllStations[j].theId + "CTRL.positions[1] +0.05) then");
                        modelicaWriter.WriteLine("        if(mix" + plant.AllStations[j].theId + "CTRL.instruction[4] >= 0.5 and robot" + plant.AllAGVs[i].Id + ".container > 0) then");
                        modelicaWriter.WriteLine("          robot" + plant.AllAGVs[i].Id + "ConTmp := robot" + plant.AllAGVs[i].Id + ".container;");
                        modelicaWriter.WriteLine("            robot" + plant.AllAGVs[i].Id + ".container :=0;");
                        modelicaWriter.WriteLine("        elseif (mix" + plant.AllStations[j].theId + "CTRL.instruction[4] <= -0.5 and robot" + plant.AllAGVs[i].Id + ".container == 0) then");
                        modelicaWriter.WriteLine("          robot" + plant.AllAGVs[i].Id + ".container := mix" + plant.AllStations[j].theId + ".containerID;");
                        modelicaWriter.WriteLine("        end if;");
                    }
                    else if (plant.AllStations[j].isStorageStation())
                    {
                        if (first == 0)
                        {
                            modelicaWriter.WriteLine("      robot" + plant.AllAGVs[i].Id + ".posX >= (two" + plant.AllStations[j].theId + ".dockingX - 5) and robot" + plant.AllAGVs[i].Id + ".posX <= (two" + plant.AllStations[j].theId + ".dockingX + 5)");
                            first += 1;
                        }
                        else
                        {
                            modelicaWriter.WriteLine("    elseif (robot" + plant.AllAGVs[i].Id + ".posX >= (two" + plant.AllStations[j].theId + ".dockingX - 5) and robot" + plant.AllAGVs[i].Id + ".posX <= (two" + plant.AllStations[j].theId + ".dockingX + 5)");
                        }
                        modelicaWriter.WriteLine("      and robot" + plant.AllAGVs[i].Id + ".posY >= (two" + plant.AllStations[j].theId + ".dockingY - 5) and robot" + plant.AllAGVs[i].Id + ".posY <= (two" + plant.AllStations[j].theId + ".dockingY + 5)");
                        modelicaWriter.WriteLine("      and (if(two" + plant.AllStations[j].theId + ".dockingRot < 4 or two" + plant.AllStations[j].theId + ".dockingRot > 357) then (if (robot" + plant.AllAGVs[i].Id + ".rot >= 355 or robot" + plant.AllAGVs[i].Id + ".rot <= 5) then true else false)");
                        modelicaWriter.WriteLine("           else (if(robot" + plant.AllAGVs[i].Id + ".rot >= (two" + plant.AllStations[j].theId + ".dockingRot - 5) and robot" + plant.AllAGVs[i].Id + ".rot <= (two" + plant.AllStations[j].theId + ".dockingRot +5)) then true else false))");
                        modelicaWriter.WriteLine("      and two" + plant.AllStations[j].theId + ".altitude <= two" + plant.AllStations[j].theId + "CTRL.positions[1] +0.05) then");
                        modelicaWriter.WriteLine("        if( two" + plant.AllStations[j].theId + "CTRL.instruction[3] >= 0.5 and robot" + plant.AllAGVs[i].Id + ".container > 0) then");
                        modelicaWriter.WriteLine("          robot" + plant.AllAGVs[i].Id + "ConTmp := robot" + plant.AllAGVs[i].Id + ".container;");
                        modelicaWriter.WriteLine("          robot" + plant.AllAGVs[i].Id + ".container :=0;");
                        modelicaWriter.WriteLine("        elseif (two" + plant.AllStations[j].theId + "CTRL.instruction[3] <= -0.5 and robot" + plant.AllAGVs[i].Id + ".container == 0) then");
                        modelicaWriter.WriteLine("          robot" + plant.AllAGVs[i].Id + ".container := two" + plant.AllStations[j].theId + ".containerID[1];");
                        modelicaWriter.WriteLine("        end if;");
                    }
                }
                modelicaWriter.WriteLine("    end if;");
            }
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine();
            for (int i = 0; i < plant.AllStations.Count; i++)
            {
                if (plant.AllStations[i].isFillingStation())
                {
                    int first = 0;
                    modelicaWriter.WriteLine("    if (");
                    for (int j = 0; j < plant.AllAGVs.Count; j++)
                    {
                        if (first == 0)
                        {
                            modelicaWriter.WriteLine("      robot" + plant.AllAGVs[j].Id + ".posX >= (col" + plant.AllStations[i].theId + ".dockingX - 5) and robot" + plant.AllAGVs[j].Id + ".posX <= (col" + plant.AllStations[i].theId + ".dockingX + 5)");
                            first += 1;
                        }
                        else
                        {
                            modelicaWriter.WriteLine("    elseif (robot" + plant.AllAGVs[j].Id + ".posX >= (col" + plant.AllStations[i].theId + ".dockingX - 5) and robot" + plant.AllAGVs[j].Id + ".posX <= (col" + plant.AllStations[i].theId + ".dockingX + 5)");
                        }
                        modelicaWriter.WriteLine("      and robot" + plant.AllAGVs[j].Id + ".posY >= (col" + plant.AllStations[i].theId + ".dockingY - 5) and robot" + plant.AllAGVs[j].Id + ".posY <= (col" + plant.AllStations[i].theId + ".dockingY + 5)");
                        modelicaWriter.WriteLine("      and (if(col" + plant.AllStations[i].theId + ".dockingRot < 4 or col" + plant.AllStations[i].theId + ".dockingRot > 357) then (if (robot" + plant.AllAGVs[j].Id + ".rot >= 355 or robot" + plant.AllAGVs[j].Id + ".rot <= 5) then true else false)");
                        modelicaWriter.WriteLine("           else (if(robot" + plant.AllAGVs[j].Id + ".rot >= (col" + plant.AllStations[i].theId + ".dockingRot - 5) and robot" + plant.AllAGVs[j].Id + ".rot <= (col" + plant.AllStations[i].theId + ".dockingRot +5)) then true else false))");
                        modelicaWriter.WriteLine("      ) then");
                        modelicaWriter.WriteLine("        if(col" + plant.AllStations[i].theId + ".containerID ==0) then");
                        modelicaWriter.WriteLine("          col" + plant.AllStations[i].theId + ".containerID := robot" + plant.AllAGVs[j].Id + ".container;");
                        modelicaWriter.WriteLine("        end if;");
                    }
                    modelicaWriter.WriteLine("    else");
                    modelicaWriter.WriteLine("      col" + plant.AllStations[i].theId + ".containerID := 0;");
                    modelicaWriter.WriteLine("    end if;");
                }
                else if (plant.AllStations[i].isStorageStation())
                {
                    int first = 0;
                    modelicaWriter.WriteLine("    if (");
                    for (int j = 0; j < plant.AllAGVs.Count; j++)
                    {
                        if (first == 0)
                        {
                            modelicaWriter.WriteLine("      robot" + plant.AllAGVs[j].Id + ".posX >= (two" + plant.AllStations[i].theId + ".dockingX - 5) and robot" + plant.AllAGVs[j].Id + ".posX <= (two" + plant.AllStations[i].theId + ".dockingX + 5)");
                            first += 1;
                        }
                        else
                        {
                            modelicaWriter.WriteLine("    elseif (robot" + plant.AllAGVs[j].Id + ".posX >= (two" + plant.AllStations[i].theId + ".dockingX - 5) and robot" + plant.AllAGVs[j].Id + ".posX <= (two" + plant.AllStations[i].theId + ".dockingX + 5)");
                        }
                        modelicaWriter.WriteLine("      and robot" + plant.AllAGVs[j].Id + ".posY >= (two" + plant.AllStations[i].theId + ".dockingY - 5) and robot" + plant.AllAGVs[j].Id + ".posY <= (two" + plant.AllStations[i].theId + ".dockingY + 5)");
                        modelicaWriter.WriteLine("      and (if(two" + plant.AllStations[i].theId + ".dockingRot < 4 or two" + plant.AllStations[i].theId + ".dockingRot > 357) then (if (robot" + plant.AllAGVs[j].Id + ".rot >= 355 or robot" + plant.AllAGVs[j].Id + ".rot <= 5) then true else false)");
                        modelicaWriter.WriteLine("           else (if(robot" + plant.AllAGVs[j].Id + ".rot >= (two" + plant.AllStations[i].theId + ".dockingRot - 5) and robot" + plant.AllAGVs[j].Id + ".rot <= (two" + plant.AllStations[i].theId + ".dockingRot +5)) then true else false))");
                        modelicaWriter.WriteLine("      and two" + plant.AllStations[i].theId + ".altitude <= two" + plant.AllStations[i].theId + "CTRL.positions[1] +0.05) then");
                        modelicaWriter.WriteLine("        if( two" + plant.AllStations[i].theId + "CTRL.instruction[3] >= 0.5 and two" + plant.AllStations[i].theId + ".containerID[1] == 0) then");
                        modelicaWriter.WriteLine("          two" + plant.AllStations[i].theId + ".containerID[1] := robot" + plant.AllAGVs[j].Id + "ConTmp;");
                        modelicaWriter.WriteLine("        elseif (two" + plant.AllStations[i].theId + "CTRL.instruction[3] <= -0.5 and two" + plant.AllStations[i].theId + ".containerID[1] > 0) then");
                        modelicaWriter.WriteLine("          two" + plant.AllStations[i].theId + ".containerID[1] := 0;");
                        modelicaWriter.WriteLine("        end if;");
                    }
                    modelicaWriter.WriteLine("    end if;");
                }
                else if (plant.AllStations[i].isMixingStation())
                {
                    int first = 0;
                    modelicaWriter.WriteLine("    if (");
                    for (int j = 0; j < plant.AllAGVs.Count; j++)
                    {
                        if (first == 0)
                        {
                            modelicaWriter.WriteLine("      robot" + plant.AllAGVs[j].Id + ".posX >= (mix" + plant.AllStations[i].theId + ".dockingX - 5) and robot" + plant.AllAGVs[j].Id + ".posX <= (mix" + plant.AllStations[i].theId + ".dockingX + 5)");
                            first += 1;
                        }
                        else
                        {
                            modelicaWriter.WriteLine("    elseif (robot" + plant.AllAGVs[j].Id + ".posX >= (mix" + plant.AllStations[i].theId + ".dockingX - 5) and robot" + plant.AllAGVs[j].Id + ".posX <= (mix" + plant.AllStations[i].theId + ".dockingX + 5)");
                        }
                        modelicaWriter.WriteLine("      and robot" + plant.AllAGVs[j].Id + ".posY >= (mix" + plant.AllStations[i].theId + ".dockingY - 5) and robot" + plant.AllAGVs[j].Id + ".posY <= (mix" + plant.AllStations[i].theId + ".dockingY + 5)");
                        modelicaWriter.WriteLine("      and (if(mix" + plant.AllStations[i].theId + ".dockingRot < 4 or mix" + plant.AllStations[i].theId + ".dockingRot > 357) then (if (robot" + plant.AllAGVs[j].Id + ".rot >= 355 or robot" + plant.AllAGVs[j].Id + ".rot <= 5) then true else false)");
                        modelicaWriter.WriteLine("           else (if(robot" + plant.AllAGVs[j].Id + ".rot >= (mix" + plant.AllStations[i].theId + ".dockingRot - 5) and robot" + plant.AllAGVs[j].Id + ".rot <= (mix" + plant.AllStations[i].theId + ".dockingRot +5)) then true else false))");
                        modelicaWriter.WriteLine("      and mix" + plant.AllStations[i].theId + ".altitude <= mix" + plant.AllStations[i].theId + "CTRL.positions[1] +0.05) then");
                        modelicaWriter.WriteLine("        if(mix" + plant.AllStations[i].theId + "CTRL.instruction[4] >= 0.5 and mix" + plant.AllStations[i].theId + ".containerID == 0) then");
                        modelicaWriter.WriteLine("          mix" + plant.AllStations[i].theId + ".containerID := robot" + plant.AllAGVs[j].Id + "ConTmp;");
                        modelicaWriter.WriteLine("        elseif (mix" + plant.AllStations[i].theId + "CTRL.instruction[4] <= -0.5 and mix" + plant.AllStations[i].theId + ".containerID > 0) then");
                        modelicaWriter.WriteLine("          mix" + plant.AllStations[i].theId + ".containerID := 0;");
                        modelicaWriter.WriteLine("        end if;");
                    }
                    modelicaWriter.WriteLine("    end if;");
                }
            }
            modelicaWriter.WriteLine();
            for (int i = 0; i < plant.AllStations.Count; i++)
            {
                if (plant.AllStations[i].isStorageStation())
                {
                    modelicaWriter.WriteLine("    if (two" + plant.AllStations[i].theId + ".altitude >= two" + plant.AllStations[i].theId + "CTRL.positions[3] -0.05) then");
                    modelicaWriter.WriteLine("      if (two" + plant.AllStations[i].theId + ".containerID[1] > 0 and two" + plant.AllStations[i].theId + "CTRL.instruction[3] <= -0.5 and (two" + plant.AllStations[i].theId + ".traverse > two" + plant.AllStations[i].theId + "CTRL.holdPositions[1]+0.05 or two" + plant.AllStations[i].theId + ".traverse < two" + plant.AllStations[i].theId + "CTRL.holdPositions[1]-0.05)) then");
                    modelicaWriter.WriteLine("        if (two" + plant.AllStations[i].theId + ".traverse >= two" + plant.AllStations[i].theId + "CTRL.holdPositions[2]-0.1 and two" + plant.AllStations[i].theId + ".traverse <= two" + plant.AllStations[i].theId + "CTRL.holdPositions[2]+0.1 and two" + plant.AllStations[i].theId + ".containerID[2] <= 0.1) then");
                    modelicaWriter.WriteLine("          two" + plant.AllStations[i].theId + ".containerID[2] := two" + plant.AllStations[i].theId + ".containerID[1];");
                    modelicaWriter.WriteLine("          two" + plant.AllStations[i].theId + ".containerID[1] := 0;");
                    modelicaWriter.WriteLine("        elseif (two" + plant.AllStations[i].theId + ".traverse >= two" + plant.AllStations[i].theId + "CTRL.holdPositions[3]-0.1 and two" + plant.AllStations[i].theId + ".traverse <= two" + plant.AllStations[i].theId + "CTRL.holdPositions[3]+0.1 and two" + plant.AllStations[i].theId + ".containerID[3] <= 0.1) then");
                    modelicaWriter.WriteLine("          two" + plant.AllStations[i].theId + ".containerID[3] := two" + plant.AllStations[i].theId + ".containerID[1];");
                    modelicaWriter.WriteLine("          two" + plant.AllStations[i].theId + ".containerID[1] := 0;");
                    modelicaWriter.WriteLine("        elseif (two" + plant.AllStations[i].theId + ".traverse >= two" + plant.AllStations[i].theId + "CTRL.holdPositions[4]-0.1 and two" + plant.AllStations[i].theId + ".traverse <= two" + plant.AllStations[i].theId + "CTRL.holdPositions[4]+0.1 and two" + plant.AllStations[i].theId + ".containerID[4] <= 0.1) then");
                    modelicaWriter.WriteLine("          two" + plant.AllStations[i].theId + ".containerID[4] := two" + plant.AllStations[i].theId + ".containerID[1];");
                    modelicaWriter.WriteLine("          two" + plant.AllStations[i].theId + ".containerID[1] := 0;");
                    modelicaWriter.WriteLine("        elseif (two" + plant.AllStations[i].theId + ".traverse >= two" + plant.AllStations[i].theId + "CTRL.holdPositions[5]-0.1 and two" + plant.AllStations[i].theId + ".traverse <= two" + plant.AllStations[i].theId + "CTRL.holdPositions[5]+0.1 and two" + plant.AllStations[i].theId + ".containerID[5] <= 0.1) then");
                    modelicaWriter.WriteLine("          two" + plant.AllStations[i].theId + ".containerID[5] := two" + plant.AllStations[i].theId + ".containerID[1];");
                    modelicaWriter.WriteLine("          two" + plant.AllStations[i].theId + ".containerID[1] := 0;");
                    modelicaWriter.WriteLine("        elseif (two" + plant.AllStations[i].theId + ".traverse >= two" + plant.AllStations[i].theId + "CTRL.holdPositions[6]-0.1 and two" + plant.AllStations[i].theId + ".traverse <= two" + plant.AllStations[i].theId + "CTRL.holdPositions[6]+0.1 and two" + plant.AllStations[i].theId + ".containerID[6] <= 0.1) then");
                    modelicaWriter.WriteLine("          two" + plant.AllStations[i].theId + ".containerID[6] := two" + plant.AllStations[i].theId + ".containerID[1];");
                    modelicaWriter.WriteLine("          two" + plant.AllStations[i].theId + ".containerID[1] := 0;");
                    modelicaWriter.WriteLine("        elseif (two" + plant.AllStations[i].theId + ".traverse >= two" + plant.AllStations[i].theId + "CTRL.holdPositions[7]-0.1 and two" + plant.AllStations[i].theId + ".traverse <= two" + plant.AllStations[i].theId + "CTRL.holdPositions[7]+0.1 and two" + plant.AllStations[i].theId + ".containerID[7] <= 0.1) then");
                    modelicaWriter.WriteLine("          two" + plant.AllStations[i].theId + ".containerID[7] := two" + plant.AllStations[i].theId + ".containerID[1];");
                    modelicaWriter.WriteLine("          two" + plant.AllStations[i].theId + ".containerID[1] := 0;");
                    modelicaWriter.WriteLine("        end if;");
                    modelicaWriter.WriteLine("      elseif (two" + plant.AllStations[i].theId + ".containerID[1] < 0.1 and two" + plant.AllStations[i].theId + "CTRL.instruction[3] >= 0.5 and (two" + plant.AllStations[i].theId + ".traverse > two" + plant.AllStations[i].theId + "CTRL.holdPositions[1]+0.05 or two" + plant.AllStations[i].theId + ".traverse < two" + plant.AllStations[i].theId + "CTRL.holdPositions[1]-0.05) then");
                    modelicaWriter.WriteLine("        if (two" + plant.AllStations[i].theId + ".traverse >= two" + plant.AllStations[i].theId + "CTRL.holdPositions[2]-0.1 and two" + plant.AllStations[i].theId + ".traverse <= two" + plant.AllStations[i].theId + "CTRL.holdPositions[2]+0.1 and two" + plant.AllStations[i].theId + ".containerID[2] > 0) then");
                    modelicaWriter.WriteLine("          two" + plant.AllStations[i].theId + ".containerID[1] := two" + plant.AllStations[i].theId + ".containerID[2];");
                    modelicaWriter.WriteLine("          two" + plant.AllStations[i].theId + ".containerID[2] := 0;");
                    modelicaWriter.WriteLine("        elseif (two" + plant.AllStations[i].theId + ".traverse >= two" + plant.AllStations[i].theId + "CTRL.holdPositions[3]-0.1 and two" + plant.AllStations[i].theId + ".traverse <= two" + plant.AllStations[i].theId + "CTRL.holdPositions[3]+0.1 and two" + plant.AllStations[i].theId + ".containerID[3] > 0) then");
                    modelicaWriter.WriteLine("          two" + plant.AllStations[i].theId + ".containerID[1] := two" + plant.AllStations[i].theId + ".containerID[3];");
                    modelicaWriter.WriteLine("          two" + plant.AllStations[i].theId + ".containerID[3] := 0;");
                    modelicaWriter.WriteLine("        elseif (two" + plant.AllStations[i].theId + ".traverse >= two" + plant.AllStations[i].theId + "CTRL.holdPositions[4]-0.1 and two" + plant.AllStations[i].theId + ".traverse <= two" + plant.AllStations[i].theId + "CTRL.holdPositions[4]+0.1 and two" + plant.AllStations[i].theId + ".containerID[4] > 0) then");
                    modelicaWriter.WriteLine("          two" + plant.AllStations[i].theId + ".containerID[1] := two" + plant.AllStations[i].theId + ".containerID[4];");
                    modelicaWriter.WriteLine("          two" + plant.AllStations[i].theId + ".containerID[4] := 0;");
                    modelicaWriter.WriteLine("        elseif (two" + plant.AllStations[i].theId + ".traverse >= two" + plant.AllStations[i].theId + "CTRL.holdPositions[5]-0.1 and two" + plant.AllStations[i].theId + ".traverse <= two" + plant.AllStations[i].theId + "CTRL.holdPositions[5]+0.1 and two" + plant.AllStations[i].theId + ".containerID[5] > 0) then");
                    modelicaWriter.WriteLine("          two" + plant.AllStations[i].theId + ".containerID[1] := two" + plant.AllStations[i].theId + ".containerID[5];");
                    modelicaWriter.WriteLine("          two" + plant.AllStations[i].theId + ".containerID[5] := 0;");
                    modelicaWriter.WriteLine("        elseif (two" + plant.AllStations[i].theId + ".traverse >= two" + plant.AllStations[i].theId + "CTRL.holdPositions[6]-0.1 and two" + plant.AllStations[i].theId + ".traverse <= two" + plant.AllStations[i].theId + "CTRL.holdPositions[6]+0.1 and two" + plant.AllStations[i].theId + ".containerID[6] > 0) then");
                    modelicaWriter.WriteLine("          two" + plant.AllStations[i].theId + ".containerID[1] := two" + plant.AllStations[i].theId + ".containerID[6];");
                    modelicaWriter.WriteLine("          two" + plant.AllStations[i].theId + ".containerID[6] := 0;");
                    modelicaWriter.WriteLine("        elseif (two" + plant.AllStations[i].theId + ".traverse >= two" + plant.AllStations[i].theId + "CTRL.holdPositions[7]-0.1 and two" + plant.AllStations[i].theId + ".traverse <= two" + plant.AllStations[i].theId + "CTRL.holdPositions[7]+0.1 and two" + plant.AllStations[i].theId + ".containerID[7] > 0) then");
                    modelicaWriter.WriteLine("          two" + plant.AllStations[i].theId + ".containerID[1] := two" + plant.AllStations[i].theId + ".containerID[7];");
                    modelicaWriter.WriteLine("          two" + plant.AllStations[i].theId + ".containerID[7] := 0;");
                    modelicaWriter.WriteLine("        end if;");
                    modelicaWriter.WriteLine("      end if;");
                    modelicaWriter.WriteLine("    end if;");
                }
            }
            modelicaWriter.WriteLine("    end if;");
            #endregion;
            modelicaWriter.WriteLine("  end Arena;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine();
            #region annotation;
            if (!oldversion)
            {
                modelicaWriter.WriteLine("annotation(uses(Modelica(version = \"3.0.1\"), Socket(version = \"1\")), experiment(");
                modelicaWriter.WriteLine("StopTime = 240));");
            }
            else
            {
                modelicaWriter.WriteLine("annotation(uses(Socket(version = \"1\"), Modelica(version=\"2.2.1\")), version=\"1\");");
            }
            #endregion;
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("  package Control \"control with the socket\"");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("    connector CSharpOutput = output Modelica.Blocks.Interfaces.RealOutput;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("  class ExternalControl");
            modelicaWriter.WriteLine("    \"wrapping model to connect the actual control unit to the simulation using TCP sockets\"");
            modelicaWriter.WriteLine("    Modelica.Blocks.Interfaces.RealInput digitalInputs[" + (plant.AllAGVs.Count * 5 + plant.getStorageStationCount() * 9 + plant.getMixStatCount() * 5 + plant.getColorStatCount() * 3 + plant.getBattStatCount() + plant.AllVessels.Count * 22 + 1) + "];");
            modelicaWriter.WriteLine("    Modelica.Blocks.Interfaces.RealOutput digitalOutputs[" + (2 * plant.AllAGVs.Count + plant.getBattStatCount() + plant.getStorageStationCount() * 5 + plant.getColorStatCount() * 2 + plant.getMixStatCount() * 5) + "];");
            modelicaWriter.WriteLine("    " + plant.theName + ".Communication.Socket socket(IPAddress=IPAddress, port=port);");
            modelicaWriter.WriteLine("    parameter String IPAddress=\"127.0.0.1\" \"IP-Address of Server\";");
            modelicaWriter.WriteLine("    parameter Integer port=12345 \"Port to connect to\";");
            modelicaWriter.WriteLine("    parameter Modelica.SIunits.Time samplingRate=0.1;");
            modelicaWriter.WriteLine();
            for (int i = 0; i < plant.AllAGVs.Count; i++)
            {
                Datastructure.Model.AGV.AGV robo = plant.AllAGVs[i];
                modelicaWriter.WriteLine("    CSharpOutput robot" + robo.Id + "X;");
                modelicaWriter.WriteLine("    CSharpOutput robot" + robo.Id + "Y;");
                modelicaWriter.WriteLine("    CSharpOutput robot" + robo.Id + "Rot;");
                modelicaWriter.WriteLine("    CSharpOutput robot" + robo.Id + "Con;");
                modelicaWriter.WriteLine("    CSharpOutput robot" + robo.Id + "Bat;");
            }
            for (int i = 0; i < plant.AllStations.Count; i++)
            {
                if (plant.AllStations[i].isStorageStation())
                {
                    modelicaWriter.WriteLine("    CSharpOutput two" + plant.AllStations[i].theId + "Con1;");
                    modelicaWriter.WriteLine("    CSharpOutput two" + plant.AllStations[i].theId + "Con2;");
                    modelicaWriter.WriteLine("    CSharpOutput two" + plant.AllStations[i].theId + "Con3;");
                    modelicaWriter.WriteLine("    CSharpOutput two" + plant.AllStations[i].theId + "Con4;");
                    modelicaWriter.WriteLine("    CSharpOutput two" + plant.AllStations[i].theId + "Con5;");
                    modelicaWriter.WriteLine("    CSharpOutput two" + plant.AllStations[i].theId + "Con6;");
                    modelicaWriter.WriteLine("    CSharpOutput two" + plant.AllStations[i].theId + "Con7;");
                    modelicaWriter.WriteLine("    CSharpOutput two" + plant.AllStations[i].theId + "Alt;");
                    modelicaWriter.WriteLine("    CSharpOutput two" + plant.AllStations[i].theId + "Tra;");
                }
                else if (plant.AllStations[i].isMixingStation())
                {
                    modelicaWriter.WriteLine("    CSharpOutput mix" + plant.AllStations[i].theId + "Con;");
                    modelicaWriter.WriteLine("    CSharpOutput mix" + plant.AllStations[i].theId + "RRate;");
                    modelicaWriter.WriteLine("    CSharpOutput mix" + plant.AllStations[i].theId + "Sensor;");
                    modelicaWriter.WriteLine("    CSharpOutput mix" + plant.AllStations[i].theId + "Alt;");
                    modelicaWriter.WriteLine("    CSharpOutput mix" + plant.AllStations[i].theId + "FRate;");
                }
                else if (plant.AllStations[i].isFillingStation())
                {
                    modelicaWriter.WriteLine("    CSharpOutput col" + plant.AllStations[i].theId + "Con;");
                    modelicaWriter.WriteLine("    CSharpOutput col" + plant.AllStations[i].theId + "FRate_1;");
                    modelicaWriter.WriteLine("    CSharpOutput col" + plant.AllStations[i].theId + "FRate_2;");
                }
                else if (plant.AllStations[i].isChargingStation())
                {
                    modelicaWriter.WriteLine("    CSharpOutput bat" + plant.AllStations[i].theId + "LRate;");
                }
            }
            for (int i = 0; i < plant.AllVessels.Count; i++)
            {
                modelicaWriter.WriteLine("    CSharpOutput con" + plant.AllVessels[i].theId + "HRate;");
                modelicaWriter.WriteLine("    CSharpOutput con" + plant.AllVessels[i].theId + "WRate;");
                modelicaWriter.WriteLine("    CSharpOutput[5,4] con" + plant.AllVessels[i].theId + "LayerVol;");
            }
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("    Boolean sampled;");
            modelicaWriter.WriteLine("  equation");
            for (int i = 0; i < plant.AllAGVs.Count; i++)
            {
                modelicaWriter.WriteLine("    digitalInputs[" + (i * 5 + 1) + "] = robot" + plant.AllAGVs[i].Id + "X;");
                modelicaWriter.WriteLine("    digitalInputs[" + (i * 5 + 2) + "] = robot" + plant.AllAGVs[i].Id + "Y;");
                modelicaWriter.WriteLine("    digitalInputs[" + (i * 5 + 3) + "] = robot" + plant.AllAGVs[i].Id + "Rot;");
                modelicaWriter.WriteLine("    digitalInputs[" + (i * 5 + 4) + "] = robot" + plant.AllAGVs[i].Id + "Con;");
                modelicaWriter.WriteLine("    digitalInputs[" + (i * 5 + 5) + "] = robot" + plant.AllAGVs[i].Id + "Bat;");
            }
            int x = 0;
            for (int j = 0; j < plant.AllStations.Count; j++)
            {
                if (plant.AllStations[j].isStorageStation())
                {
                    modelicaWriter.WriteLine("    digitalInputs[" + (plant.AllAGVs.Count * 5 + x * 9 + 1) + "] = two" + plant.AllStations[j].theId + "Con1;");
                    modelicaWriter.WriteLine("    digitalInputs[" + (plant.AllAGVs.Count * 5 + x * 9 + 2) + "] = two" + plant.AllStations[j].theId + "Con2;");
                    modelicaWriter.WriteLine("    digitalInputs[" + (plant.AllAGVs.Count * 5 + x * 9 + 3) + "] = two" + plant.AllStations[j].theId + "Con3;");
                    modelicaWriter.WriteLine("    digitalInputs[" + (plant.AllAGVs.Count * 5 + x * 9 + 4) + "] = two" + plant.AllStations[j].theId + "Con4;");
                    modelicaWriter.WriteLine("    digitalInputs[" + (plant.AllAGVs.Count * 5 + x * 9 + 5) + "] = two" + plant.AllStations[j].theId + "Con5;");
                    modelicaWriter.WriteLine("    digitalInputs[" + (plant.AllAGVs.Count * 5 + x * 9 + 6) + "] = two" + plant.AllStations[j].theId + "Con6;");
                    modelicaWriter.WriteLine("    digitalInputs[" + (plant.AllAGVs.Count * 5 + x * 9 + 7) + "] = two" + plant.AllStations[j].theId + "Con7;");
                    modelicaWriter.WriteLine("    digitalInputs[" + (plant.AllAGVs.Count * 5 + x * 9 + 8) + "] = two" + plant.AllStations[j].theId + "Alt;");
                    modelicaWriter.WriteLine("    digitalInputs[" + (plant.AllAGVs.Count * 5 + x * 9 + 9) + "] = two" + plant.AllStations[j].theId + "Tra;");
                    x++;
                }
            }
            x = 0;
            for (int j = 0; j < plant.AllStations.Count; j++)
            {
                if (plant.AllStations[j].isMixingStation())
                {
                    modelicaWriter.WriteLine("    digitalInputs[" + (plant.AllAGVs.Count * 5 + 9 * plant.getStorageStationCount() + x * 5 + 1) + "] = mix" + plant.AllStations[j].theId + "Con;");
                    modelicaWriter.WriteLine("    digitalInputs[" + (plant.AllAGVs.Count * 5 + 9 * plant.getStorageStationCount() + x * 5 + 2) + "] = mix" + plant.AllStations[j].theId + "RRate;");
                    modelicaWriter.WriteLine("    digitalInputs[" + (plant.AllAGVs.Count * 5 + 9 * plant.getStorageStationCount() + x * 5 + 3) + "] = mix" + plant.AllStations[j].theId + "Sensor;");
                    modelicaWriter.WriteLine("    digitalInputs[" + (plant.AllAGVs.Count * 5 + 9 * plant.getStorageStationCount() + x * 5 + 4) + "] = mix" + plant.AllStations[j].theId + "Alt;");
                    modelicaWriter.WriteLine("    digitalInputs[" + (plant.AllAGVs.Count * 5 + 9 * plant.getStorageStationCount() + x * 5 + 5) + "] = mix" + plant.AllStations[j].theId + "FRate;");
                    x++;
                }
            }
            x = 0;
            for (int j = 0; j < plant.AllStations.Count; j++)
            {
                if (plant.AllStations[j].isFillingStation())
                {
                    modelicaWriter.WriteLine("    digitalInputs[" + (plant.AllAGVs.Count * 5 + 9 * plant.getStorageStationCount() + plant.getMixStatCount() * 5 + x * 3 + 1) + "] = col" + plant.AllStations[j].theId + "Con;");
                    modelicaWriter.WriteLine("    digitalInputs[" + (plant.AllAGVs.Count * 5 + 9 * plant.getStorageStationCount() + plant.getMixStatCount() * 5 + x * 3 + 2) + "] = col" + plant.AllStations[j].theId + "FRate_1;");
                    modelicaWriter.WriteLine("    digitalInputs[" + (plant.AllAGVs.Count * 5 + 9 * plant.getStorageStationCount() + plant.getMixStatCount() * 5 + x * 3 + 3) + "] = col" + plant.AllStations[j].theId + "FRate_2;");
                    x++;
                }
            }
            x = 0;
            for (int j = 0; j < plant.AllStations.Count; j++)
            {
                if (plant.AllStations[j].isChargingStation())
                {
                    modelicaWriter.WriteLine("    digitalInputs[" + (plant.AllAGVs.Count * 5 + 9 * plant.getStorageStationCount() + plant.getMixStatCount() * 5 + plant.getColorStatCount() * 3 + x + 1) + "] = bat" + plant.AllStations[j].theId + "LRate;");
                    x++;
                }
            }
            for (int i = 0; i < plant.AllVessels.Count; i++)
            {
                modelicaWriter.WriteLine("    digitalInputs[" + (plant.AllAGVs.Count * 5 + 9 * plant.getStorageStationCount() + plant.getMixStatCount() * 5 + plant.getColorStatCount() * 3 + plant.getBattStatCount() + i * 22 + 1) + "] = con" + plant.AllVessels[i].theId + "HRate;");
                modelicaWriter.WriteLine("    digitalInputs[" + (plant.AllAGVs.Count * 5 + 9 * plant.getStorageStationCount() + plant.getMixStatCount() * 5 + plant.getColorStatCount() * 3 + plant.getBattStatCount() + i * 22 + 2) + "] = con" + plant.AllVessels[i].theId + "WRate;");
                int tmp = 3;
                for (int j = 0; j < 5; j++)
                {
                    for (int k = 0; k < 4; k++)
                    {
                        modelicaWriter.WriteLine("    digitalInputs[" + (plant.AllAGVs.Count * 5 + 9 * plant.getStorageStationCount() + plant.getMixStatCount() * 5 + plant.getColorStatCount() * 3 + plant.getBattStatCount() + i * 22 + tmp) + "] = con" + plant.AllVessels[i].theId + "LayerVol[" + (j + 1) + "," + (k + 1) + "];");
                        tmp++;
                    }
                }
            }
            modelicaWriter.WriteLine("    digitalInputs[" + (plant.AllAGVs.Count * 5 + plant.getStorageStationCount() * 9 + plant.getMixStatCount() * 5 + plant.getColorStatCount() * 3 + plant.AllVessels.Count * 22 + plant.getBattStatCount() + 1) + "] = time;");
            modelicaWriter.WriteLine("  algorithm");
            modelicaWriter.WriteLine("    sampled:=sample(0, samplingRate);");
            modelicaWriter.WriteLine("    when (sampled and time >0) then");
            modelicaWriter.WriteLine("      sendInputSignalVector(digitalInputs);");
            modelicaWriter.WriteLine("      digitalOutputs:=receiveOutputSignalVector();");
            modelicaWriter.WriteLine("    end when;");
            modelicaWriter.WriteLine("  end ExternalControl;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("  function sendInputSignalVector");
            modelicaWriter.WriteLine("    \"transform a vector of real signals to a string with comma seperated values and send the string\"");
            modelicaWriter.WriteLine("    input Real inputSignals[" + (plant.AllAGVs.Count * 5 + plant.getStorageStationCount() * 9 + plant.getMixStatCount() * 5 + plant.getColorStatCount() * 3 + plant.AllVessels.Count * 22 + plant.getBattStatCount() + 1) + "];");
            modelicaWriter.WriteLine("  protected");
            modelicaWriter.WriteLine("    String message;");
            modelicaWriter.WriteLine("    Integer roundUp;");
            modelicaWriter.WriteLine("    Real rounded;");
            modelicaWriter.WriteLine("  algorithm");
            modelicaWriter.WriteLine("    message:=\"\";");
            modelicaWriter.WriteLine("    for i in 1:" + (plant.AllAGVs.Count * 5 + plant.getStorageStationCount() * 9 + plant.getMixStatCount() * 5 + plant.getColorStatCount() * 3 + plant.AllVessels.Count * 22 + plant.getBattStatCount() + 1) + " loop");
            modelicaWriter.WriteLine("      roundUp := integer(inputSignals[i] * 100);");
            modelicaWriter.WriteLine("      rounded := roundUp/100;");
            modelicaWriter.WriteLine("      if (i==1) then");
            modelicaWriter.WriteLine("        message:=String(rounded);");
            modelicaWriter.WriteLine("      else");
            modelicaWriter.WriteLine("        message:=message + \",\" + String(rounded);");
            modelicaWriter.WriteLine("      end if;");
            modelicaWriter.WriteLine("    end for;");
            modelicaWriter.WriteLine("    message:=message+\"\\n\";");
            modelicaWriter.WriteLine("    " + plant.theName + ".Communication.sendMessage(message);");
            modelicaWriter.WriteLine("  end sendInputSignalVector;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("  function receiveOutputSignalVector");
            modelicaWriter.WriteLine("    \"receive a sring and transform it to a vector of signals\"");
            modelicaWriter.WriteLine("    output Real outputSignals[" + (2 * plant.AllAGVs.Count + plant.getBattStatCount() + plant.getStorageStationCount() * 5 + plant.getColorStatCount() * 2 + plant.getMixStatCount() * 5) + "];");
            modelicaWriter.WriteLine("  protected");
            modelicaWriter.WriteLine("    String message;");
            modelicaWriter.WriteLine("    Integer responseLength;");
            modelicaWriter.WriteLine("    Boolean found;");
            modelicaWriter.WriteLine("    Integer oldStartPosition;");
            modelicaWriter.WriteLine("    Integer newStartPosition;");
            modelicaWriter.WriteLine("    Integer index;");
            modelicaWriter.WriteLine("  algorithm");
            modelicaWriter.WriteLine("    message:=  " + plant.theName + ".Communication.receiveMessage();");
            modelicaWriter.WriteLine("    responseLength:=Modelica.Utilities.Strings.length(message);");
            modelicaWriter.WriteLine("    found:=true;");
            modelicaWriter.WriteLine("    oldStartPosition:=1;");
            modelicaWriter.WriteLine("    index:=1;");
            modelicaWriter.WriteLine("    if (responseLength > 0) then");
            modelicaWriter.WriteLine("      while found loop");
            modelicaWriter.WriteLine("        found:=false;");
            modelicaWriter.WriteLine("        newStartPosition:=  Modelica.Utilities.Strings.find(");
            modelicaWriter.WriteLine("          message,");
            modelicaWriter.WriteLine("          \",\",");
            modelicaWriter.WriteLine("          oldStartPosition);");
            modelicaWriter.WriteLine("        if newStartPosition>0 and index<" + (2 * plant.AllAGVs.Count + plant.getBattStatCount() + plant.getStorageStationCount() * 5 + plant.getColorStatCount() * 2 + plant.getMixStatCount() * 5 + 1) + " then");
            modelicaWriter.WriteLine("          outputSignals[index]:=  Modelica.Utilities.Strings.scanReal( Modelica.Utilities.Strings.substring(");
            modelicaWriter.WriteLine("            message,");
            modelicaWriter.WriteLine("            oldStartPosition,");
            modelicaWriter.WriteLine("            newStartPosition-1));");
            modelicaWriter.WriteLine("          index:=index + 1;");
            modelicaWriter.WriteLine("          found:=true;");
            modelicaWriter.WriteLine("          oldStartPosition:=newStartPosition + 1;");
            modelicaWriter.WriteLine("        end if;");
            modelicaWriter.WriteLine("        if index<=" + (2 * plant.AllAGVs.Count + plant.getBattStatCount() + plant.getStorageStationCount() * 5 + plant.getColorStatCount() * 2 + plant.getMixStatCount() * 5) + " then");
            modelicaWriter.WriteLine("          outputSignals[index]:=Modelica.Utilities.Strings.scanReal( Modelica.Utilities.Strings.substring(");
            modelicaWriter.WriteLine("            message,");
            modelicaWriter.WriteLine("            oldStartPosition,");
            modelicaWriter.WriteLine("            responseLength));");
            modelicaWriter.WriteLine("        end if;");
            modelicaWriter.WriteLine("      end while;");
            modelicaWriter.WriteLine("    end if;");
            modelicaWriter.WriteLine("    if (index < " + (2 * plant.AllAGVs.Count + plant.getBattStatCount() + plant.getStorageStationCount() * 5 + plant.getColorStatCount() * 2 + plant.getMixStatCount() * 5) + ") then");
            modelicaWriter.WriteLine("      for i in index:" + (2 * plant.AllAGVs.Count + plant.getBattStatCount() + plant.getStorageStationCount() * 5 + plant.getColorStatCount() * 2 + plant.getMixStatCount() * 5) + " loop");
            modelicaWriter.WriteLine("        outputSignals[i]:=0;");
            modelicaWriter.WriteLine("      end for;");
            modelicaWriter.WriteLine("    end if;");
            modelicaWriter.WriteLine("  end receiveOutputSignalVector;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("  end Control;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("  package Communication");
            modelicaWriter.WriteLine("    function sendMessage \"send a message through the socket\"");
            modelicaWriter.WriteLine("      annotation(Include=\"#include <Socket.h>\", Library={\"SocketModelica\",\"wsock32\"});");
            modelicaWriter.WriteLine("      input String message;");
            modelicaWriter.WriteLine("      external \"C\" sendMessage(message);");
            modelicaWriter.WriteLine("    end sendMessage;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("    function receiveMessage \"receive a message through the socket\"");
            modelicaWriter.WriteLine("      annotation(Include=\"#include <Socket.h>\", Library={\"SocketModelica\",\"wsock32\"});");
            modelicaWriter.WriteLine("      output String message;");
            modelicaWriter.WriteLine("      external \"C\" message = ");
            modelicaWriter.WriteLine("        receiveMessage();");
            modelicaWriter.WriteLine("    end receiveMessage;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("    function createSocket \"Create the TCP socket\"");
            modelicaWriter.WriteLine("      annotation(Include=\"#include <Socket.h>\", Library={\"SocketModelica\",\"wsock32\"});");
            modelicaWriter.WriteLine("      input String IPAddress;");
            modelicaWriter.WriteLine("      input Integer port;");
            modelicaWriter.WriteLine("      external \"C\" createSocket(IPAddress,port);");
            modelicaWriter.WriteLine("    end createSocket;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("    function startUP \"Initialize the Windows Socket Library\" ");
            modelicaWriter.WriteLine("      annotation(Include=\"#include <Socket.h>\", Library={\"SocketModelica\",\"wsock32\"});");
            modelicaWriter.WriteLine("      external \"C\" startUP();");
            modelicaWriter.WriteLine("    end startUP;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("    model Socket");
            modelicaWriter.WriteLine("      \"Model to create a TCP socket to specified IPAddress and port\" ");
            modelicaWriter.WriteLine("      parameter String IPAddress=\"127.0.0.1\" \"IP-Address of Server\";");
            modelicaWriter.WriteLine("      parameter Integer port=12345 \"Port to connect to\";");
            modelicaWriter.WriteLine("    algorithm ");
            modelicaWriter.WriteLine("      if (initial()) then");
            modelicaWriter.WriteLine("        startUP();");
            modelicaWriter.WriteLine("        createSocket(IPAddress,port);");
            modelicaWriter.WriteLine("      end if;");
            modelicaWriter.WriteLine("    end Socket;");
            modelicaWriter.WriteLine("  end Communication;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("end " + plant.theName + ";");
            modelicaWriter.WriteLine();


            #region oldModelicaModel;
            /**
            modelicaWriter.WriteLine("package PipelessPlant_v2 \"the pipeless plant model\""); 
            modelicaWriter.WriteLine("package ModelClasses \"the model classes of the pipeless plant\"");
            modelicaWriter.WriteLine("package UserTypes");
            modelicaWriter.WriteLine("\"different datatypes, which are used in the model classes\""); 
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("record Position");
            modelicaWriter.WriteLine("type Coordinate = Real(min=0, max=999);");
            modelicaWriter.WriteLine("Coordinate x;");
            modelicaWriter.WriteLine("Coordinate y;");
            modelicaWriter.WriteLine("end Position;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("record Color");
            modelicaWriter.WriteLine("type ColorParameter = Integer(min=0, max=255);");
            modelicaWriter.WriteLine("ColorParameter red;");
            modelicaWriter.WriteLine("ColorParameter green;");
            modelicaWriter.WriteLine("ColorParameter blue;");
            modelicaWriter.WriteLine("end Color;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("record Size");
            modelicaWriter.WriteLine("type SizeParameter = Integer(min=0, max=999);");
            modelicaWriter.WriteLine("SizeParameter length;");
            modelicaWriter.WriteLine("SizeParameter width;");
            modelicaWriter.WriteLine("end Size;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("record TimeStamp");
            modelicaWriter.WriteLine("type Hours = Integer(min=0, max=23);");
            modelicaWriter.WriteLine("type Minutes = Integer(min=0, max=59);");
            modelicaWriter.WriteLine("type Seconds = Integer(min=0, max=59);");
            modelicaWriter.WriteLine("Hours hours;");
            modelicaWriter.WriteLine("Minutes minutes;");
            modelicaWriter.WriteLine("Seconds seconds;");
            modelicaWriter.WriteLine("end TimeStamp;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("end UserTypes;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("record Ingredient");
            modelicaWriter.WriteLine("parameter String description;");
            modelicaWriter.WriteLine("parameter String name;");
            modelicaWriter.WriteLine("parameter Integer ingredientID(min=0);");
            modelicaWriter.WriteLine("parameter UserTypes.Color color;");
            modelicaWriter.WriteLine("parameter Integer timeTicksForProcessing(min=0);");
            modelicaWriter.WriteLine("end Ingredient;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("record RecipeIngredient");
            modelicaWriter.WriteLine("parameter Integer ingredients_IngredientID;");
            modelicaWriter.WriteLine("parameter Integer timeTicksForProcessing(min=0);");
            modelicaWriter.WriteLine("Boolean finished;");
            modelicaWriter.WriteLine("Integer priority(min=0);");
            modelicaWriter.WriteLine("UserTypes.TimeStamp startTime;");
            modelicaWriter.WriteLine("UserTypes.TimeStamp deadLine;");
            modelicaWriter.WriteLine("Integer order(min=0);");
            modelicaWriter.WriteLine("end RecipeIngredient;");
            modelicaWriter.WriteLine();    
            modelicaWriter.WriteLine("record Recipe");
            modelicaWriter.WriteLine("parameter Integer recipeID(min=0);");
            modelicaWriter.WriteLine("parameter String name;");
            modelicaWriter.WriteLine("parameter String description;");
            modelicaWriter.WriteLine("parameter Integer[:] ingredients;");
            modelicaWriter.WriteLine("Integer priority(min=0);");
            modelicaWriter.WriteLine("UserTypes.TimeStamp startTime;");
            modelicaWriter.WriteLine("UserTypes.TimeStamp deadLine;");
            modelicaWriter.WriteLine("end Recipe;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("class Robot");
            modelicaWriter.WriteLine("constant Real zuBogenmass = Modelica.Constants.pi / 180;");
            modelicaWriter.WriteLine("parameter Integer robotID(min=0);");
            modelicaWriter.WriteLine("parameter String landmark;");
            modelicaWriter.WriteLine("parameter UserTypes.Size size;");
            modelicaWriter.WriteLine("parameter Real batteryLossRate=0.000005;");
            modelicaWriter.WriteLine("parameter Boolean broken;");
            modelicaWriter.WriteLine("UserTypes.Position cur_Position;");
            modelicaWriter.WriteLine("Real rotation(unit=\"degrees\", min=0, max=359);");
            modelicaWriter.WriteLine("Real batteryState(min=0, max=1);");
            modelicaWriter.WriteLine("Boolean isFree;");
            modelicaWriter.WriteLine("output Integer cur_load_RecipeID;");
            modelicaWriter.WriteLine("output Real cur_Velocity;");
            modelicaWriter.WriteLine("output Real cur_rotVelocity;");
            modelicaWriter.WriteLine("equation");
            modelicaWriter.WriteLine("cur_load_RecipeID = 4;");
            modelicaWriter.WriteLine("cur_Velocity = 15;");
            modelicaWriter.WriteLine("cur_rotVelocity = 0;");
            modelicaWriter.WriteLine("algorithm");
            modelicaWriter.WriteLine("cur_Position.x :=stepForwardX(cur_Position.x, rotation, cur_Velocity, timeTicks);");
            modelicaWriter.WriteLine("cur_Position.y :=stepForwardY(cur_Position.y, rotation, cur_Velocity, timeTicks);");
            modelicaWriter.WriteLine("rotation := mod(rotation + cur_rotVelocity * 1, 360);");
            modelicaWriter.WriteLine("batteryState := if (batteryState - batteryLossRate * (cur_Velocity * 1)) < 0 then 0 else batteryState - batteryLossRate * (cur_Velocity * 1);");
            modelicaWriter.WriteLine("if cur_load_RecipeID < 0 then");
            modelicaWriter.WriteLine("isFree := true;");
            modelicaWriter.WriteLine("else");
            modelicaWriter.WriteLine("isFree := false;");
            modelicaWriter.WriteLine("end if;");
            modelicaWriter.WriteLine("end Robot;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("class Station");
            modelicaWriter.WriteLine("parameter String name;");
            modelicaWriter.WriteLine("parameter String description;");
            modelicaWriter.WriteLine("parameter UserTypes.Size size;");
            modelicaWriter.WriteLine("parameter UserTypes.Position position;");
            modelicaWriter.WriteLine("parameter Real rotation(unit=\"degrees\", min=0, max=359);");
            modelicaWriter.WriteLine("parameter Integer stationID(min=0);");
            modelicaWriter.WriteLine("parameter Boolean broken;");
            modelicaWriter.WriteLine("Boolean isDockFree;");
            modelicaWriter.WriteLine("Boolean isContainerFree;");
            modelicaWriter.WriteLine("output Integer cur_Container_RecipeID;");
            modelicaWriter.WriteLine("output Integer cur_Robot_RobotID;");
            modelicaWriter.WriteLine("equation ");
            modelicaWriter.WriteLine("cur_Container_RecipeID = 5;");
            modelicaWriter.WriteLine("cur_Robot_RobotID = 5;");
            modelicaWriter.WriteLine("algorithm ");
            modelicaWriter.WriteLine("if cur_Container_RecipeID < 0 then");
            modelicaWriter.WriteLine("isContainerFree :=true;");
            modelicaWriter.WriteLine("else");
            modelicaWriter.WriteLine("isContainerFree :=false;");
            modelicaWriter.WriteLine("end if;");
            modelicaWriter.WriteLine("if cur_Robot_RobotID < 0 then");
            modelicaWriter.WriteLine("isDockFree :=true;");
            modelicaWriter.WriteLine("else");
            modelicaWriter.WriteLine("isDockFree :=false;");
            modelicaWriter.WriteLine("end if;");
            modelicaWriter.WriteLine("end Station;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("record BatteryStation");
            modelicaWriter.WriteLine("extends Station;");
            modelicaWriter.WriteLine("end BatteryStation;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("record IOStation");
            modelicaWriter.WriteLine("extends Station;");
            modelicaWriter.WriteLine("end IOStation;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("record ColorStation");
            modelicaWriter.WriteLine("extends Station;");
            modelicaWriter.WriteLine("parameter Ingredient canWorkOn;");
            modelicaWriter.WriteLine("end ColorStation;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("record Job");
            modelicaWriter.WriteLine("parameter Integer jobID(min=0);");
            modelicaWriter.WriteLine("end Job;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("class Arena");
            modelicaWriter.WriteLine("constant Real zuBogenmass = Modelica.Constants.pi / 180;");
            modelicaWriter.WriteLine("parameter Real scaling(min=0);");
            modelicaWriter.WriteLine("parameter UserTypes.TimeStamp startTime;");
            Model.Roboter robo;
            for(int i = 0; i < ObserverModule.getInstance().getAktuelleArena().AllAGVs.Count; i++)
            {
                robo = ObserverModule.getInstance().getAktuelleArena().AllAGVs[i];
                if (robo.getAktuellenAuftrag() != null)
                {
                    modelicaWriter.WriteLine("ModelClasses.Robot robot" + robo.theId + "(");
                    modelicaWriter.WriteLine("robotID(start = " + robo.theId + "),");
                    modelicaWriter.WriteLine("landmark(start = \"" + robo.theName + "\"),");
                    modelicaWriter.WriteLine("size(length(start = " + robo.theSize.theLength + "), width(start = " + robo.theSize.theWidth + ")),");
                    modelicaWriter.WriteLine("cur_Position(x(start = " + robo.theCurPosition.X + "), y(start = " + robo.theCurPosition.Y + ")),");
                    modelicaWriter.WriteLine("rotation(start = " + robo.theRotation + "),");
                    modelicaWriter.WriteLine("batteryState(start = " + robo.getAkkuKapazitaet() + "),");
                    modelicaWriter.WriteLine("broken(start = " + (robo.getNichtDefekt() ? "false" : "true") + "),");
                    modelicaWriter.WriteLine("isFree(start = " + (robo.isFrei() ? "true" : "false") + "),");
                    modelicaWriter.WriteLine("cur_load_RecipeID(start = " + robo.getAktuellenAuftrag().theId + "),");
                    modelicaWriter.WriteLine("cur_Velocity(start = 15),");
                    modelicaWriter.WriteLine("cur_rotVelocity(start=0));");
                }
                else
                {
                    modelicaWriter.WriteLine("ModelClasses.Robot robot" + robo.theId + "(");
                    modelicaWriter.WriteLine("robotID(start = " + robo.theId + "),");
                    modelicaWriter.WriteLine("landmark(start = \"" + robo.theName + "\"),");
                    modelicaWriter.WriteLine("size(length(start = " + robo.theSize.theLength + "), width(start = " + robo.theSize.theWidth + ")),");
                    modelicaWriter.WriteLine("cur_Position(x(start = " + robo.theCurPosition.X + "), y(start = " + robo.theCurPosition.Y + ")),");
                    modelicaWriter.WriteLine("rotation(start = " + robo.theRotation + "),");
                    modelicaWriter.WriteLine("batteryState(start = " + robo.getAkkuKapazitaet() + "),");
                    modelicaWriter.WriteLine("broken(start = " + (robo.getNichtDefekt() ? "false" : "true") + "),");
                    modelicaWriter.WriteLine("isFree(start = " + (robo.isFrei() ? "true" : "false") + "),");
                    modelicaWriter.WriteLine("cur_load_RecipeID(start = -1),");
                    modelicaWriter.WriteLine("cur_Velocity(start = 0),");
                    modelicaWriter.WriteLine("cur_rotVelocity(start=0));");
                }
            }
            /**
            modelicaWriter.WriteLine("parameter Robot["+ObserverModule.getInstance().getAktuelleArena().AllAGVs.Count+"] theRobots = {");
            for (int i = 0; i < ObserverModule.getInstance().getAktuelleArena().AllAGVs.Count - 1; i++)
            {
                robo = ObserverModule.getInstance().getAktuelleArena().AllAGVs[i];
                if (robo.getAktuellenAuftrag() != null)
                {
                    modelicaWriter.WriteLine("ModelClasses.Robot(" + robo.theId + ", \"" + robo.theName + "\", ModelClasses.UserTypes.Size(" + robo.theSize.theWidth + ", " + robo.theSize.theLength + "), ModelClasses.UserTypes.Position(" + robo.theCurPosition.X + "," + robo.theCurPosition.Y + "), " + robo.theRotation + ", " + robo.getAkkuKapazitaet() + ", " + (robo.getNichtDefekt()? "false" : "true") + ", " + (robo.isFrei()?"true":"false") + ", "+robo.getAktuellenAuftrag().theId+"),");
                }
                else
                {
                    modelicaWriter.WriteLine("ModelClasses.Robot(" + robo.theId + ", \"" + robo.theName + "\", ModelClasses.UserTypes.Size(" + robo.theSize.theWidth + ", " + robo.theSize.theLength + "), ModelClasses.UserTypes.Position(" + robo.theCurPosition.X + "," + robo.theCurPosition.Y + "), " + robo.theRotation + ", " + robo.getAkkuKapazitaet() + ", " + (robo.getNichtDefekt() ? "false" : "true") + ", " + (robo.isFrei() ? "true" : "false") + ", 0),");
                }
            }
            robo = ObserverModule.getInstance().getAktuelleArena().AllAGVs[ObserverModule.getInstance().getAktuelleArena().AllAGVs.Count-1];
            if (robo.getAktuellenAuftrag() != null)
            {
                modelicaWriter.WriteLine("ModelClasses.Robot(" + robo.theId + ", \"" + robo.theName + "\", ModelClasses.UserTypes.Size(" + robo.theSize.theWidth + ", " + robo.theSize.theLength + "), ModelClasses.UserTypes.Position(" + robo.theCurPosition.X + "," + robo.theCurPosition.Y + "), " + robo.theRotation + ", " + robo.getAkkuKapazitaet() + ", " + (robo.getNichtDefekt() ? "false" : "true") + ", " + (robo.isFrei() ? "true" : "false") + ", " + robo.getAktuellenAuftrag().theId + ")};");
            }
            else
            {
                modelicaWriter.WriteLine("ModelClasses.Robot(" + robo.theId + ", \"" + robo.theName + "\", ModelClasses.UserTypes.Size(" + robo.theSize.theWidth + ", " + robo.theSize.theLength + "), ModelClasses.UserTypes.Position(" + robo.theCurPosition.X + "," + robo.theCurPosition.Y + "), " + robo.theRotation + ", " + robo.getAkkuKapazitaet() + ", " + (robo.getNichtDefekt() ? "false" : "true") + ", " + (robo.isFrei() ? "true" : "false") + ", 0)};");
            }
            Model.Station stat;
            for (int i = 0; i < ObserverModule.getInstance().getAktuelleArena().AllStations.Count; i++)
            {
                stat = ObserverModule.getInstance().getAktuelleArena().AllStations[i];
                modelicaWriter.WriteLine("ModelClasses.Station station" + stat.theId + "(");
                modelicaWriter.WriteLine("name(start=\"" + stat.theName + "\"),");
                modelicaWriter.WriteLine("description(start=\"" + stat.getBeschreibung() + "\"),");
                modelicaWriter.WriteLine("size(width(start=" + stat.theSize.theWidth + "), length(start=" + stat.theSize.theLength + ")),");
                modelicaWriter.WriteLine("position(x(start=" + stat.thePosition.X + "), y(start=" + stat.thePosition.Y + ")),");
                modelicaWriter.WriteLine("rotation(start=" + stat.theRotation + "),");
                modelicaWriter.WriteLine("stationID(start=" + stat.theId + "),");
                modelicaWriter.WriteLine("broken(start=" + (stat.getNichtDefekt() ? "false" : "true") + "),");
                modelicaWriter.WriteLine("isDockFree(start=" + (stat.isDockBusy() ? "false" : "true") + "),");
                modelicaWriter.WriteLine("isContainerFree(start=" + (stat.getContainerBusy() ? "false" : "true") + "),");
                modelicaWriter.WriteLine("cur_Container_RecipeID(start=0),");
                modelicaWriter.WriteLine("cur_Robot_RobotID(start=0));");
            }
            /**
            modelicaWriter.WriteLine("parameter Station["+ObserverModule.getInstance().getAktuelleArena().AllStations.Count+"] theStations = {");
            for (int i = 0; i < ObserverModule.getInstance().getAktuelleArena().AllStations.Count-1; i++)
            {
                stat = ObserverModule.getInstance().getAktuelleArena().AllStations[i];
                modelicaWriter.WriteLine("ModelClasses.Station(\""+stat.theName+"\", \""+stat.getBeschreibung()+"\", ModelClasses.UserTypes.Size("+stat.theSize.theWidth+","+stat.theSize.theLength+"), ModelClasses.UserTypes.Position("+stat.thePosition.X+","+stat.thePosition.Y+"), "+stat.theRotation+", "+stat.theId+", "+(stat.getNichtDefekt()? "false" : "true") +", "+(stat.isDockBusy()? "false" : "true") +", "+(stat.getContainerBusy()? "false" : "true") +", 0, 0),");
            }
            stat = ObserverModule.getInstance().getAktuelleArena().AllStations[ObserverModule.getInstance().getAktuelleArena().AllStations.Count-1];
            modelicaWriter.WriteLine("ModelClasses.Station(\"" + stat.theName + "\",\"" + stat.getBeschreibung() + "\", ModelClasses.UserTypes.Size(" + stat.theSize.theWidth + "," + stat.theSize.theLength + "), ModelClasses.UserTypes.Position(" + stat.thePosition.X + "," + stat.thePosition.Y + "), " + stat.theRotation + ", " + stat.theId + ", " + (stat.getNichtDefekt() ? "false" : "true") + ", " + (stat.isDockBusy() ? "false" : "true") + ", " + (stat.getContainerBusy() ? "false" : "true") + ", 0, 0)};");
            
            modelicaWriter.WriteLine("Recipe["+ObserverModule.getInstance().getAktuelleArena().AllRecipes.Count+"] theRecipes = {");
            for (int i = 0; i < ObserverModule.getInstance().getAktuelleArena().AllRecipes.Count-1; i++)
            {
                Model.Recipe reci = ObserverModule.getInstance().getAktuelleArena().AllRecipes[i];
                modelicaWriter.WriteLine("ModelClasses.Recipe(1, \"bla\", \"blub\", {1,2,3}, 0, ModelClasses.UserTypes.TimeStamp(0, 0, 0), ModelClasses.UserTypes.TimeStamp(0, 0, 0)),");
            }
            Model.Recipe recipe = ObserverModule.getInstance().getAktuelleArena().AllRecipes[ObserverModule.getInstance().getAktuelleArena().AllRecipes.Count-1];
            modelicaWriter.WriteLine("ModelClasses.Recipe(1, \"bla\", \"blub\", {1,2,3}, 0, ModelClasses.UserTypes.TimeStamp(0, 0, 0), ModelClasses.UserTypes.TimeStamp(0, 0, 0))};");
            modelicaWriter.WriteLine("end Arena;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("function stepForwardX \"one step of the robot\"");
            modelicaWriter.WriteLine("input Real x;");
            modelicaWriter.WriteLine("input Real rot;");
            modelicaWriter.WriteLine("input Real vel;");
            modelicaWriter.WriteLine("input Real timeTicks;");
            modelicaWriter.WriteLine("output Real newX;");
            modelicaWriter.WriteLine("protected ");
            modelicaWriter.WriteLine("constant Real zuBogenmass = Modelica.Constants.pi / 180;");
            modelicaWriter.WriteLine("algorithm");
            modelicaWriter.WriteLine("if vel== 0 then");
            modelicaWriter.WriteLine("newX := x;");
            modelicaWriter.WriteLine("else");
            modelicaWriter.WriteLine("for i in 1:timeTicks loop");
            modelicaWriter.WriteLine("newX := Modelica.Math.cos(rot * zuBogenmass) * vel + (if i == 1 then x else newX);");
            modelicaWriter.WriteLine("end for;");
            modelicaWriter.WriteLine("end if;");
            modelicaWriter.WriteLine("end stepForwardX;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("function stepForwardY \"one step of the robot\"");
            modelicaWriter.WriteLine("input Real y;");
            modelicaWriter.WriteLine("input Real rot;");
            modelicaWriter.WriteLine("input Real vel;");
            modelicaWriter.WriteLine("input Real timeTicks;");
            modelicaWriter.WriteLine("output Real newY;");
            modelicaWriter.WriteLine("protected");
            modelicaWriter.WriteLine("constant Real zuBogenmass = Modelica.Constants.pi / 180;");
            modelicaWriter.WriteLine("algorithm");
            modelicaWriter.WriteLine("if vel== 0 then");
            modelicaWriter.WriteLine("newY := y;");
            modelicaWriter.WriteLine("else");
            modelicaWriter.WriteLine("for i in 1:timeTicks loop");
            modelicaWriter.WriteLine("newY := Modelica.Math.sin(rot * zuBogenmass) * vel + (if i == 1 then y else newY);");
            modelicaWriter.WriteLine("end for;");
            modelicaWriter.WriteLine("end if;");
            modelicaWriter.WriteLine("end stepForwardY;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("end ModelClasses;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("class currentPlant");
            modelicaWriter.WriteLine("output Integer x;");
            modelicaWriter.WriteLine("input Integer y;");
            modelicaWriter.WriteLine("ModelClasses.Arena cur_arena(scaling=3, startTime(hours = 10,minutes = 0, seconds = 0));");
            modelicaWriter.WriteLine("equation");
            modelicaWriter.WriteLine("x=1;");
            modelicaWriter.WriteLine("end currentPlant;");
            modelicaWriter.WriteLine();
            modelicaWriter.WriteLine("annotation (uses(Modelica(version=\"2.2.1\")));");
            modelicaWriter.WriteLine("end PipelessPlant_v2;");*/
            #endregion;

            modelicaWriter.Close();
            if (oldversion)
            {
                GUI.PCSMainWindow.getInstance().postStatusMessage("Written Modelica 2.2 Model to File " + filename + "_modelica2.mo");
            }
            else
            {
                GUI.PCSMainWindow.getInstance().postStatusMessage("Written Modelica 3.2 Model to File " + filename + ".mo");
            }
        }

        public void saveModelToFile(Stream selectedFile)
        {
            TextWriter modelWriter = new StreamWriter(selectedFile);
            Datastructure.Model.Plant arena = ObserverModule.getInstance().getCurrentPlant();
            modelWriter.WriteLine("<plant>");
            modelWriter.WriteLine("  <name>" + arena.theName + "</name>");
            modelWriter.WriteLine("  <ipadress>" + arena.theIpAdress + "</ipadress>");
            modelWriter.WriteLine("  <port>" + arena.thePort + "</port>");
            modelWriter.WriteLine("  <samplingrate>" + arena.theSamplingRate + "</samplingrate>");
            modelWriter.WriteLine("  <scaling>" + arena.theScaling + "</scaling>");
            if (arena.theSize != null)
            {
                modelWriter.WriteLine("  <length>" + arena.theSize.Height + "</length>");
                modelWriter.WriteLine("  <width>" + arena.theSize.Width + "</width>");
            }
            modelWriter.WriteLine("  <simtime>" + arena.theCurSimTime + "</simtime>");
            modelWriter.WriteLine("  <batloadrate>" + arena.BatStationLoadRate + "</batloadrate>");
            modelWriter.WriteLine("  <colfillrate>" + arena.ColFillRate + "</colfillrate>");
            modelWriter.WriteLine("  <curingtimepervolume>" + arena.VesselCuringTimePerVolume + "</curingtimepervolume>");
            modelWriter.WriteLine("  <mixingtimepervolume>" + arena.VesselMixingTimePerVolume + "</mixingtimepervolume>");
            modelWriter.WriteLine("  <mixfillrate>" + arena.MixFillRate + "</mixfillrate>");
            modelWriter.WriteLine("  <robotbatterycapacity>" + arena.AGVBatteryCapacity + "</robotbatterycapacity>");
            modelWriter.WriteLine("  <robotbatterylossrate>" + arena.AGVBatteryLossRate + "</robotbatterylossrate>");
            modelWriter.WriteLine();
            modelWriter.WriteLine("  <selectedcolors>");
            Datastructure.Model.MyColors myColors = Datastructure.Model.MyColors.getInstance();
            for (int i = 0; i < arena.ChoosenColorIDs.Length; i++)
            {
                modelWriter.WriteLine("    <color>");
                modelWriter.WriteLine("      <id>"+ arena.ChoosenColorIDs[i]+"</id>");
                modelWriter.WriteLine("      <name>" + myColors.getName(arena.ChoosenColorIDs[i]) + "</name>");
                modelWriter.WriteLine("      <red>" + myColors.getColor(arena.ChoosenColorIDs[i]).R + "</red>");
                modelWriter.WriteLine("      <green>" + myColors.getColor(arena.ChoosenColorIDs[i]).G + "</green>");
                modelWriter.WriteLine("      <blue>" + myColors.getColor(arena.ChoosenColorIDs[i]).B + "</blue>");
                modelWriter.WriteLine("    </color>");
            }
            modelWriter.WriteLine("  </selectedcolors>");
            modelWriter.WriteLine();
            modelWriter.WriteLine("  <allcontainers>");
            for (int i = 0; i < arena.AllVessels.Count; i++)
            {
                modelWriter.WriteLine("    <container>");
                modelWriter.WriteLine("      <id>"+arena.AllVessels[i].theId+"</id>");
                modelWriter.WriteLine("      <fillhard>" + arena.AllVessels[i].theCurFillHard + "</fillhard>");
                modelWriter.WriteLine("      <fillwater>" + arena.AllVessels[i].theCurFillWater + "</fillwater>");
                modelWriter.WriteLine("      <maxvolume>" + arena.AllVessels[i].theMaxVolume + "</maxvolume>");
                modelWriter.WriteLine("      <alllayers>");
                modelWriter.WriteLine("        <layer>");
                for (int j = 0; j < arena.AllVessels[i].theLayers.Length; j++)
                {
                    modelWriter.WriteLine("          <id>" + j + "</id>");
                    modelWriter.WriteLine("          <colorname>"+arena.AllVessels[i].theLayers[j].theColorName+"</colorname>");
                    modelWriter.WriteLine("          <allingredients>");
                    for (int k = 0; k < arena.AllVessels[i].theLayers[j].theIngredients.Count; k++)
                    {
                        modelWriter.WriteLine("            <ingredient>");
                        modelWriter.WriteLine("              <colorid>"+arena.AllVessels[i].theLayers[j].theIngredients[k].theColorID+"</colorid>");
                        modelWriter.WriteLine("              <volume>" + arena.AllVessels[i].theLayers[j].theIngredients[k].theCurVolume + "</volume>");
                        modelWriter.WriteLine("              <filltime>" + arena.AllVessels[i].theLayers[j].theIngredients[k].theFillTime + "</filltime>");
                        modelWriter.WriteLine("              <mixfilltime>" + arena.AllVessels[i].theLayers[j].theIngredients[k].theMixFillTime + "</mixfilltime>");
                        modelWriter.WriteLine("              <mixtime>" + arena.AllVessels[i].theLayers[j].theIngredients[k].theMixTime + "</mixtime>");
                        modelWriter.WriteLine("              <name>" + arena.AllVessels[i].theLayers[j].theIngredients[k].theName + "</name>");
                        modelWriter.WriteLine("            </ingredient>");
                    }
                    modelWriter.WriteLine("          </allingredients>");
                }
                modelWriter.WriteLine("        </layer>");
                modelWriter.WriteLine("      </alllayers>");
                modelWriter.WriteLine("    </container>");
            }
            modelWriter.WriteLine("  </allcontainers>");
            modelWriter.WriteLine();
            modelWriter.WriteLine("  <allrobots>");
            for (int i = 0; i < arena.AllAGVs.Count; i++)
            {
                modelWriter.WriteLine("    <robot>");
                modelWriter.WriteLine("      <id>" + arena.AllAGVs[i].Id + "</id>");
                modelWriter.WriteLine("      <batteryload>" + arena.AllAGVs[i].theBatteryLoad + "</batteryload>");
                modelWriter.WriteLine("      <name>" + arena.AllAGVs[i].Id + "</name>");
                modelWriter.WriteLine("      <rotation>" + arena.AllAGVs[i].theRotation + "</rotation>");
                modelWriter.WriteLine("      <positionx>" + arena.AllAGVs[i].theCurPosition.X + "</positionx>");
                modelWriter.WriteLine("      <positiony>" + arena.AllAGVs[i].theCurPosition.Y + "</positiony>");
                modelWriter.WriteLine("      <length>" + arena.AllAGVs[i].Diameter + "</length>");
                modelWriter.WriteLine("      <width>" + arena.AllAGVs[i].Diameter + "</width>");
                if (arena.AllAGVs[i].theVessel != null)
                {
                    modelWriter.WriteLine("      <containerid>" + arena.AllAGVs[i].theVessel.theId + "</containerid>");
                }
                else
                {
                    modelWriter.WriteLine("      <containerid>-1</containerid>");
                }
                modelWriter.WriteLine("    </robot>");
            }
            modelWriter.WriteLine("  </allrobots>");
            modelWriter.WriteLine();
            modelWriter.WriteLine("  <allstations>");
            for (int i = 0; i < arena.AllStations.Count; i++)
            {
                modelWriter.WriteLine("    <station>");
                modelWriter.WriteLine("      <id>" + arena.AllStations[i].theId + "</id>");
                modelWriter.WriteLine("      <dockingrotation>" + arena.AllStations[i].theDockingRot + "</dockingrotation>");
                modelWriter.WriteLine("      <dockingx>" + arena.AllStations[i].theDockingPos.X + "</dockingx>");
                modelWriter.WriteLine("      <dockingy>" + arena.AllStations[i].theDockingPos.Y + "</dockingy>");
                modelWriter.WriteLine("      <rotation>" + arena.AllStations[i].theRotation + "</rotation>");
                modelWriter.WriteLine("      <positionx>" + arena.AllStations[i].thePosition.X + "</positionx>");
                modelWriter.WriteLine("      <positiony>" + arena.AllStations[i].thePosition.Y + "</positiony>");
                modelWriter.WriteLine("      <name>" + arena.AllStations[i].theName + "</name>");
                modelWriter.WriteLine("      <length>" + arena.AllStations[i].theSize.Height + "</length>");
                modelWriter.WriteLine("      <width>" + arena.AllStations[i].theSize.Width + "</width>");              
                if (arena.AllStations[i].isChargingStation())
                {
                    modelWriter.WriteLine("      <type>bat</type>");
                    Datastructure.Model.Stations.ChargingStation bat = (Datastructure.Model.Stations.ChargingStation)arena.AllStations[i];
                    modelWriter.WriteLine("      <loadrate>" + bat.theLoadRate + "</loadrate>");
                }
                else if (arena.AllStations[i].isFillingStation())
                {
                    modelWriter.WriteLine("      <type>col</type>");
                    Datastructure.Model.Stations.FillingStation col = (Datastructure.Model.Stations.FillingStation)arena.AllStations[i];
                    //modelWriter.WriteLine("      <altitude>" + col.theAltitude + "</altitude>");
                    //modelWriter.WriteLine("      <armvelocity>" + col.theArmVelocity + "</armvelocity>");
                    //modelWriter.WriteLine("      <armpositions>");
                    //for (int j = 0; j < col.theArmpositions.Length; j++)
                    {
                    //    modelWriter.WriteLine("        <position>"+col.theArmpositions[j]+"</position>");
                    }
                    //modelWriter.WriteLine("      </armpositions>");
                    if (col.theCurContainer != null)
                    {
                        modelWriter.WriteLine("      <containerid>" + col.theCurContainer.theId + "</containerid>");
                    }
                    else
                    {
                        modelWriter.WriteLine("      <containerid>-1</containerid>");
                    }
                    modelWriter.WriteLine("      <fillrate>" + col.theFillRate_1 + "</fillrate>");
                    modelWriter.WriteLine("      <colorid>" + col.theColorID + "</colorid>");
                }
                else if (arena.AllStations[i].isMixingStation())
                {
                    modelWriter.WriteLine("      <type>mix</type>");
                    Datastructure.Model.Stations.MixingStation mix = (Datastructure.Model.Stations.MixingStation)arena.AllStations[i];
                    modelWriter.WriteLine("      <altitude>" + mix.theAltitude + "</altitude>");
                    modelWriter.WriteLine("      <armvelocity>" + mix.theArmVelocity + "</armvelocity>");
                    modelWriter.WriteLine("      <armpositions>");
                    for (int j = 0; j < mix.theArmpositions.Length; j++)
                    {
                        modelWriter.WriteLine("        <position>" + mix.theArmpositions[j] + "</position>");
                    }
                    modelWriter.WriteLine("      </armpositions>");
                    if (mix.theCurrentVessel != null)
                    {
                        modelWriter.WriteLine("      <containerid>" + mix.theCurrentVessel.theId + "</containerid>");
                    }
                    else
                    {
                        modelWriter.WriteLine("      <containerid>-1</containerid>");
                    }
                    modelWriter.WriteLine("      <fillrate>" + mix.theFillRate + "</fillrate>");
                    modelWriter.WriteLine("      <rotating>" + mix.theRotating + "</rotating>");
                    modelWriter.WriteLine("      <sensor>" + mix.theSensor + "</sensor>");
                }
                else if (arena.AllStations[i].isStorageStation())
                {
                    modelWriter.WriteLine("      <type>two</type>");
                    Datastructure.Model.Stations.StorageStation two = (Datastructure.Model.Stations.StorageStation)arena.AllStations[i];
                    modelWriter.WriteLine("      <altitude>" + two.theAltitude + "</altitude>");
                    modelWriter.WriteLine("      <varmvelocity>" + two.theArmVelocity[0] + "</varmvelocity>");
                    modelWriter.WriteLine("      <varmpositions>");
                    for (int j = 0; j < two.theVArmPositions.Length; j++)
                    {
                        modelWriter.WriteLine("        <position>" + two.theVArmPositions[j] + "</position>");
                    }
                    modelWriter.WriteLine("      </varmpositions>");
                    modelWriter.WriteLine("      <traverse>" + two.theTraverse + "</traverse>");
                    modelWriter.WriteLine("      <harmvelocity>" + two.theArmVelocity[1] + "</harmvelocity>");
                    modelWriter.WriteLine("      <harmpositions>");
                    for (int j = 0; j < two.theHArmPositions.Length; j++)
                    {
                        modelWriter.WriteLine("        <position>" + two.theHArmPositions[j] + "</position>");
                    }
                    modelWriter.WriteLine("      </harmpositions>");
                    modelWriter.WriteLine("      <containers>");
                    for (int j = 0; j < two.theVessels.Length; j++)
                    {
                        if (two.theVessels[j] != null)
                        {
                            modelWriter.WriteLine("        <containerid>" + two.theVessels[j].theId + "</containerid>");
                        }
                        else
                        {
                            modelWriter.WriteLine("        <containerid>-1</containerid>");
                        }
                    }
                    modelWriter.WriteLine("      </containers>");
                }
                modelWriter.WriteLine("    </station>");
            }
            modelWriter.WriteLine("  </allstations>");
            modelWriter.WriteLine();
            modelWriter.WriteLine("  <allrecipes>");
            for (int i = 0; i < arena.AllRecipes.Count; i++)
            {
                modelWriter.WriteLine("    <recipe>");
                modelWriter.WriteLine("      <id>"+arena.AllRecipes[i].theId+"</id>");
                modelWriter.WriteLine("      <name>" + arena.AllRecipes[i].theName + "</name>");
                modelWriter.WriteLine("      <description>" + arena.AllRecipes[i].theDescription + "</description>");
                modelWriter.WriteLine("      <alllayers>");
                for (int j = 0; j < arena.AllRecipes[i].theLayers.Count; j++)
                {
                    modelWriter.WriteLine("        <layer>");
                    modelWriter.WriteLine("          <id>"+j+"</id>");
                    modelWriter.WriteLine("          <colorname>" + arena.AllRecipes[i].theLayers[j].theColorName + "</colorname>");
                    modelWriter.WriteLine("          <allingredients>");
                    for (int k = 0; k < arena.AllRecipes[i].theLayers[j].theIngredients.Count; k++)
                    {
                        modelWriter.WriteLine("            <ingredient>");
                        modelWriter.WriteLine("              <colorid>"+arena.AllRecipes[i].theLayers[j].theIngredients[k].theColorID+"</colorid>");
                        modelWriter.WriteLine("              <volume>" + arena.AllRecipes[i].theLayers[j].theIngredients[k].theCurVolume + "</volume>");
                        modelWriter.WriteLine("              <filltime>" + arena.AllRecipes[i].theLayers[j].theIngredients[k].theFillTime + "</filltime>");
                        modelWriter.WriteLine("              <mixfilltime>" + arena.AllRecipes[i].theLayers[j].theIngredients[k].theMixFillTime + "</mixfilltime>");
                        modelWriter.WriteLine("              <mixtime>" + arena.AllRecipes[i].theLayers[j].theIngredients[k].theMixTime + "</mixtime>");
                        modelWriter.WriteLine("              <name>" + arena.AllRecipes[i].theLayers[j].theIngredients[k].theName + "</name>");
                        modelWriter.WriteLine("            </ingredient>");
                    }
                    modelWriter.WriteLine("          </allingredients>");
                    modelWriter.WriteLine("        </layer>");
                }
                modelWriter.WriteLine("      </alllayers>");
                modelWriter.WriteLine("    </recipe>");
            }
            modelWriter.WriteLine("  </allrecipes>");
            modelWriter.WriteLine("</plant>");
            modelWriter.Flush();
            selectedFile.Flush();
            modelWriter.Close();
            //selectedFile.Close();
        }
        public void loadModelFromFile(String filename)
        {
            throw new NotImplementedException();
        }
    }
}
