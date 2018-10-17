package PGWS1011 "the pipeless plant representation"


  connector CSharpInput = input Modelica.Blocks.Interfaces.RealInput;


  model Robot "the robot going to walk"
    parameter String name;
    parameter Integer id;
    parameter Real length;
    parameter Real width;
    parameter Real batteryCapacity=1;

    Real speed "speed in cm per second";
    Real rotation;
    Real realRotation(start = 0);
    Real rotationSpeed "speed of the rotation in cm per second";
    Real positionX;
    Real positionY;
    Integer container;
    Real battery;
    Real nonDiscContainer(start=0);

    CSharpInput posX;
    CSharpInput posY;
    CSharpInput rot;
    CSharpInput con;
    CSharpInput bat;
  equation
    der(positionX) = speed * Modelica.Math.cos(Modelica.SIunits.Conversions.from_deg(realRotation));
    der(positionY) = speed * Modelica.Math.sin(Modelica.SIunits.Conversions.from_deg(realRotation));
    der(rotation) = rotationSpeed;
    realRotation = mod(rotation,360);
    der(nonDiscContainer) = if (container > nonDiscContainer and (container-nonDiscContainer) > 0.05) then 5 else if (container < nonDiscContainer and (nonDiscContainer-container) > 0.05) then -5 else 0;

    posX = positionX;
    posY = positionY;
    bat = battery;
    rot = realRotation;
    con = nonDiscContainer;
  end Robot;



   partial model AbstractStation
     parameter Real dockingX;
     parameter Real dockingY;
     parameter Real dockingRot;
     parameter Integer id;
     parameter String name;
     parameter Real positionX;
     parameter Real positionY;
     parameter Real rotation;
     parameter Real length;
     parameter Real width;
   end AbstractStation;


   model BatteryStation
     extends AbstractStation;
     Real loadRate;

     CSharpInput rat;
   equation
     rat = loadRate;
   end BatteryStation;


   model BatteryStationController 
     parameter Real loadRate = 0.02;

     Real instruction;
   end BatteryStationController;


   partial model OneArmStation
     extends AbstractStation;
     Real altitude;
     Real armVelocity;
   equation
     der(altitude) = armVelocity;
   end OneArmStation;


   model TwoArmStation 
     extends AbstractStation;
     Real[2] armVelocity;
     Integer[7] containerID;
     Real altitude;
     Real traverse;
     Real[7] nonDiscContainerID;

     CSharpInput alt;
     CSharpInput tra;
     CSharpInput[7] con;
   equation 
     der(altitude) = armVelocity[1];
     der(traverse) = armVelocity[2];
     for i in 1 : 7 loop
       der(nonDiscContainerID[i]) = if(containerID[i] > nonDiscContainerID[i] and (containerID[i]-nonDiscContainerID[i]) > 0.05) then 5 else if(containerID[i] < nonDiscContainerID[i] and (nonDiscContainerID[i]-containerID[i]) > 0.05) then -5 else 0;
     end for;


     for i in 1 : 7 loop
       con[i] = nonDiscContainerID[i];
     end for;
     alt = altitude;
     tra = traverse;
   end TwoArmStation;


   model TwoArmStationController 
     parameter Real[5] positions;
     parameter Real[7] holdPositions = {0, -33, -23, -13, 13, 23, 33};

     Real[3] instruction;
     Real[2] armVelocity;
   end TwoArmStationController;


   model ColorStation 
     extends AbstractStation;
     parameter Integer colorID_1;
     parameter Integer colorID_2;

     Real fillRate_1;
     Real fillRate_2;
     Integer containerID;
     Real nonDiscContainerID(start=0);

     CSharpInput rat_1;
     CSharpInput rat_2;
     CSharpInput con;
   equation 
     der(nonDiscContainerID) = if(containerID > nonDiscContainerID and (containerID-nonDiscContainerID) >0.05) then 5 else if(containerID < nonDiscContainerID and (nonDiscContainerID-containerID) > 0.05) then -5 else 0;

     con = nonDiscContainerID;
     rat_1 = fillRate_1;
     rat_2 = fillRate_2;
   end ColorStation;


   model ColorStationController 
     parameter Real fillRate_1;
     parameter Real fillRate_2;

     Real[2] instruction;
   end ColorStationController;


   model MixingStation 
     extends AbstractStation;
     Real fillRate;
     Integer containerID;
     Real altitude;
     Real armVelocity;
     Real rotating;
     Real fillStateSensor;
     Real nonDiscContainerID(start=0);

     CSharpInput alt;
     CSharpInput rot;
     CSharpInput fillState;
     CSharpInput con;
     CSharpInput rat;
   equation 
     der(altitude) = armVelocity;
     der(nonDiscContainerID) = if(containerID > nonDiscContainerID and (containerID-nonDiscContainerID) > 0.05) then 5 else if(containerID < nonDiscContainerID and (nonDiscContainerID-containerID) > 0.05) then -5 else 0;

     con = nonDiscContainerID;
     alt = altitude;
     rot = rotating;
     fillState = fillStateSensor;
     rat = fillRate;
   end MixingStation;


   model MixingStationController 
     parameter Real[5] positions;
     parameter Real[3] fillStateStages = {0, 5, 10};
     parameter Real fillRate;

     Real[4] instruction;
     Real armVelocity;
   end MixingStationController;


   model Container 
     parameter Integer id;
     parameter Real maxVolume;
     parameter Real curingTimePerVolume = 3;
     parameter Real mixingTimePerVolume = 1;

     Real[5,4] volume;
     Real[5] mixStartTime;
     Real[5] curLayerVolume;
     Real[5] curLayerWater;
     Real[5] cementVolume;
     Boolean[5] mixed;
     Real[5] hard(start={0,0,0,0,0});
     Integer curWorkingLayer;
     Real curFillStateHard;
     Real curFillStateWater;

     CSharpInput fillHard;
     CSharpInput fillWater;
     CSharpInput[5,4] layerVol;
   equation 
     curFillStateWater =  curLayerVolume[1] + curLayerVolume[2] + curLayerVolume[3] + curLayerVolume[4] + curLayerVolume[5];
     curFillStateHard = (if (hard[1] >= 1) then curLayerVolume[1] else 0) + (if (hard[2] >= 1) then curLayerVolume[2] else 0)
                      + (if (hard[3] >= 1) then curLayerVolume[3] else 0) + (if (hard[4] >= 1) then curLayerVolume[4] else 0)
                      + (if (hard[5] >= 1) then curLayerVolume[5] else 0);
     curWorkingLayer = if (hard[5] >= 1) then 6 else if (hard[4] >= 1) then 5 else if (hard[3] >= 1) then 4 else 
                       if (hard[2] >= 1) then 3 else if (hard[1] >= 1) then 2 else 1;
     for i in 1:5 loop
       curLayerVolume[i] = volume[i,1] + volume[i,2] + volume[i,3] + volume[i,4]  + cementVolume[i];
       curLayerWater[i] = volume[i,1] + volume[i,2] + volume[i,3] + volume[i,4];
       if (cementVolume[i] > 0 and hard[i] <= 1 and curLayerWater[i] > 0) then
         der(hard[i]) = 1 / (curingTimePerVolume * curLayerVolume[i]);
       else
         der(hard[i]) = 0;
       end if;
       if (mixStartTime[i] > (mixingTimePerVolume * curLayerVolume[i])) then
         mixed[i] = true;
       else
         mixed[i] = false;
       end if;
       for j in 1:4 loop
         layerVol[i,j] = volume[i,j];
       end for;
     end for;

     fillHard = curFillStateHard;
     fillWater = curFillStateWater;
   end Container;




  model Arena "the pipelessPlant"
    parameter Real batteryLossRate = -0.001;
    parameter Real arenaWidth = 200;
    parameter Real arenaLength = 200;

    PGWS1011.Control.ExternalControl control;

    Robot robot0(name="0", id=0, speed(start=0), rotation(start=180), rotationSpeed(start=0), positionX(start=1300), positionY(start=50), length=33, width=33, container(start=0), battery(start=1));
    Integer robot0ConTmp(start=0);
    Robot robot1(name="1", id=1, speed(start=0), rotation(start=180), rotationSpeed(start=0), positionX(start=1300), positionY(start=100), length=33, width=33, container(start=0), battery(start=1));
    Integer robot1ConTmp(start=0);
    Robot robot2(name="2", id=2, speed(start=0), rotation(start=180), rotationSpeed(start=0), positionX(start=1300), positionY(start=150), length=33, width=33, container(start=0), battery(start=1));
    Integer robot2ConTmp(start=0);
    Robot robot3(name="3", id=3, speed(start=0), rotation(start=180), rotationSpeed(start=0), positionX(start=1300), positionY(start=200), length=33, width=33, container(start=0), battery(start=1));
    Integer robot3ConTmp(start=0);
    Robot robot4(name="4", id=4, speed(start=0), rotation(start=180), rotationSpeed(start=0), positionX(start=1300), positionY(start=250), length=33, width=33, container(start=0), battery(start=1));
    Integer robot4ConTmp(start=0);

    Container con1(id=1, maxVolume=300);
    Container con2(id=2, maxVolume=300);
    Container con3(id=3, maxVolume=300);
    Container con4(id=4, maxVolume=300);
    Container con5(id=5, maxVolume=300);
    Container con6(id=6, maxVolume=300);

    ColorStation col1(dockingX=54.5, dockingY=150, dockingRot=180, id=1, name="Yellow and Black", positionX=33, positionY=150, rotation=0, length=70.71, width=70.71, colorID_1=1, colorID_2=2, containerID(start=0));
    ColorStationController col1CTRL(fillRate_1=0.5, fillRate_2=0.5);
    ColorStation col3(dockingX=345.5, dockingY=150, dockingRot=0, id=3, name="Red and Blue", positionX=367, positionY=150, rotation=180, length=70.71, width=70.71, colorID_1=3, colorID_2=4, containerID(start=0));
    ColorStationController col3CTRL(fillRate_1=0.5, fillRate_2=0.5);
    BatteryStation bat4(dockingX=87.2027957955108, dockingY=204.797204204489, dockingRot=135, id=4, name="load1", positionX=72, positionY=220, rotation=315, length=45, width=45);
    BatteryStationController bat4CTRL;
    BatteryStation bat7(dockingX=312.797204204489, dockingY=204.797204204489, dockingRot=45, id=7, name="load2", positionX=328, positionY=220, rotation=225, length=45, width=45);
    BatteryStationController bat7CTRL;
    MixingStation mix5(dockingX=200, dockingY=31.5, dockingRot=270, id=5, name="mix1", positionX=200, positionY=10, rotation=90, length=70.71, width=70.71, containerID(start=0));
    MixingStationController mix5CTRL(positions={0,5,10,20,25}, fillRate=0.25);
    TwoArmStation two6(dockingX=170, dockingY=298, dockingRot=90, id=6, name="two1", positionX=170, positionY=319,5, rotation=270, length=150, width=50, containerID(start ={0,3,4,2,5,1,6}));
    TwoArmStationController two6CTRL(positions={0,5,10,15,35});
  equation
    for i in 1:5 loop
      der(con1.cementVolume[i]) = 
        if (mix5.containerID == con1.id and mix5.fillRate > 0 and i == con1.curWorkingLayer) then mix5.fillRate else
        0;
      der(con1.mixStartTime[i]) = 
        if  (mix5.containerID == con1.id and mix5.rotating > 0) then 0.5 else 
        0;
      der(con2.cementVolume[i]) = 
        if (mix5.containerID == con2.id and mix5.fillRate > 0 and i == con2.curWorkingLayer) then mix5.fillRate else
        0;
      der(con2.mixStartTime[i]) = 
        if  (mix5.containerID == con2.id and mix5.rotating > 0) then 0.5 else 
        0;
      der(con3.cementVolume[i]) = 
        if (mix5.containerID == con3.id and mix5.fillRate > 0 and i == con3.curWorkingLayer) then mix5.fillRate else
        0;
      der(con3.mixStartTime[i]) = 
        if  (mix5.containerID == con3.id and mix5.rotating > 0) then 0.5 else 
        0;
      der(con4.cementVolume[i]) = 
        if (mix5.containerID == con4.id and mix5.fillRate > 0 and i == con4.curWorkingLayer) then mix5.fillRate else
        0;
      der(con4.mixStartTime[i]) = 
        if  (mix5.containerID == con4.id and mix5.rotating > 0) then 0.5 else 
        0;
      der(con5.cementVolume[i]) = 
        if (mix5.containerID == con5.id and mix5.fillRate > 0 and i == con5.curWorkingLayer) then mix5.fillRate else
        0;
      der(con5.mixStartTime[i]) = 
        if  (mix5.containerID == con5.id and mix5.rotating > 0) then 0.5 else 
        0;
      der(con6.cementVolume[i]) = 
        if (mix5.containerID == con6.id and mix5.fillRate > 0 and i == con6.curWorkingLayer) then mix5.fillRate else
        0;
      der(con6.mixStartTime[i]) = 
        if  (mix5.containerID == con6.id and mix5.rotating > 0) then 0.5 else 
        0;
      for j in 1:4 loop
        der(con1.volume[i,j]) = 
            if (col1.containerID == con1.id and j ==col1.colorID_1 and col1.fillRate_1 > 0 and i == con1.curWorkingLayer) then col1.fillRate_1 else 
              if (col1.containerID == con1.id and j ==col1.colorID_2 and col1.fillRate_2 > 0 and i == con1.curWorkingLayer) then col1.fillRate_2 else 
            if (col3.containerID == con1.id and j ==col3.colorID_1 and col3.fillRate_1 > 0 and i == con1.curWorkingLayer) then col3.fillRate_1 else 
              if (col3.containerID == con1.id and j ==col3.colorID_2 and col3.fillRate_2 > 0 and i == con1.curWorkingLayer) then col3.fillRate_2 else 
              0;
        der(con2.volume[i,j]) = 
            if (col1.containerID == con2.id and j ==col1.colorID_1 and col1.fillRate_1 > 0 and i == con2.curWorkingLayer) then col1.fillRate_1 else 
              if (col1.containerID == con2.id and j ==col1.colorID_2 and col1.fillRate_2 > 0 and i == con2.curWorkingLayer) then col1.fillRate_2 else 
            if (col3.containerID == con2.id and j ==col3.colorID_1 and col3.fillRate_1 > 0 and i == con2.curWorkingLayer) then col3.fillRate_1 else 
              if (col3.containerID == con2.id and j ==col3.colorID_2 and col3.fillRate_2 > 0 and i == con2.curWorkingLayer) then col3.fillRate_2 else 
              0;
        der(con3.volume[i,j]) = 
            if (col1.containerID == con3.id and j ==col1.colorID_1 and col1.fillRate_1 > 0 and i == con3.curWorkingLayer) then col1.fillRate_1 else 
              if (col1.containerID == con3.id and j ==col1.colorID_2 and col1.fillRate_2 > 0 and i == con3.curWorkingLayer) then col1.fillRate_2 else 
            if (col3.containerID == con3.id and j ==col3.colorID_1 and col3.fillRate_1 > 0 and i == con3.curWorkingLayer) then col3.fillRate_1 else 
              if (col3.containerID == con3.id and j ==col3.colorID_2 and col3.fillRate_2 > 0 and i == con3.curWorkingLayer) then col3.fillRate_2 else 
              0;
        der(con4.volume[i,j]) = 
            if (col1.containerID == con4.id and j ==col1.colorID_1 and col1.fillRate_1 > 0 and i == con4.curWorkingLayer) then col1.fillRate_1 else 
              if (col1.containerID == con4.id and j ==col1.colorID_2 and col1.fillRate_2 > 0 and i == con4.curWorkingLayer) then col1.fillRate_2 else 
            if (col3.containerID == con4.id and j ==col3.colorID_1 and col3.fillRate_1 > 0 and i == con4.curWorkingLayer) then col3.fillRate_1 else 
              if (col3.containerID == con4.id and j ==col3.colorID_2 and col3.fillRate_2 > 0 and i == con4.curWorkingLayer) then col3.fillRate_2 else 
              0;
        der(con5.volume[i,j]) = 
            if (col1.containerID == con5.id and j ==col1.colorID_1 and col1.fillRate_1 > 0 and i == con5.curWorkingLayer) then col1.fillRate_1 else 
              if (col1.containerID == con5.id and j ==col1.colorID_2 and col1.fillRate_2 > 0 and i == con5.curWorkingLayer) then col1.fillRate_2 else 
            if (col3.containerID == con5.id and j ==col3.colorID_1 and col3.fillRate_1 > 0 and i == con5.curWorkingLayer) then col3.fillRate_1 else 
              if (col3.containerID == con5.id and j ==col3.colorID_2 and col3.fillRate_2 > 0 and i == con5.curWorkingLayer) then col3.fillRate_2 else 
              0;
        der(con6.volume[i,j]) = 
            if (col1.containerID == con6.id and j ==col1.colorID_1 and col1.fillRate_1 > 0 and i == con6.curWorkingLayer) then col1.fillRate_1 else 
              if (col1.containerID == con6.id and j ==col1.colorID_2 and col1.fillRate_2 > 0 and i == con6.curWorkingLayer) then col1.fillRate_2 else 
            if (col3.containerID == con6.id and j ==col3.colorID_1 and col3.fillRate_1 > 0 and i == con6.curWorkingLayer) then col3.fillRate_1 else 
              if (col3.containerID == con6.id and j ==col3.colorID_2 and col3.fillRate_2 > 0 and i == con6.curWorkingLayer) then col3.fillRate_2 else 
              0;
      end for;
    end for;


    robot0.speed = control.digitalOutputs[1];
    robot0.rotationSpeed = control.digitalOutputs[2];
    connect(robot0.posX, control.robot0X);
    connect(robot0.posY, control.robot0Y);
    connect(robot0.rot, control.robot0Rot);
    connect(robot0.con, control.robot0Con);
    connect(robot0.bat, control.robot0Bat);

    robot1.speed = control.digitalOutputs[3];
    robot1.rotationSpeed = control.digitalOutputs[4];
    connect(robot1.posX, control.robot1X);
    connect(robot1.posY, control.robot1Y);
    connect(robot1.rot, control.robot1Rot);
    connect(robot1.con, control.robot1Con);
    connect(robot1.bat, control.robot1Bat);

    robot2.speed = control.digitalOutputs[5];
    robot2.rotationSpeed = control.digitalOutputs[6];
    connect(robot2.posX, control.robot2X);
    connect(robot2.posY, control.robot2Y);
    connect(robot2.rot, control.robot2Rot);
    connect(robot2.con, control.robot2Con);
    connect(robot2.bat, control.robot2Bat);

    robot3.speed = control.digitalOutputs[7];
    robot3.rotationSpeed = control.digitalOutputs[8];
    connect(robot3.posX, control.robot3X);
    connect(robot3.posY, control.robot3Y);
    connect(robot3.rot, control.robot3Rot);
    connect(robot3.con, control.robot3Con);
    connect(robot3.bat, control.robot3Bat);

    robot4.speed = control.digitalOutputs[9];
    robot4.rotationSpeed = control.digitalOutputs[10];
    connect(robot4.posX, control.robot4X);
    connect(robot4.posY, control.robot4Y);
    connect(robot4.rot, control.robot4Rot);
    connect(robot4.con, control.robot4Con);
    connect(robot4.bat, control.robot4Bat);



    der(robot0.battery) = if(
      robot0.posX >= (bat4.dockingX - 2) and robot0.posX <= (bat4.dockingX + 2)
      and robot0.posY >= (bat4.dockingY - 2) and robot0.posY <= (bat4.dockingY + 2)
      and robot0.rot >= (bat4.dockingRot - 5) and robot0.rot <= (bat4.dockingRot +5)
      and bat4CTRL.instruction >= 0.5 and robot0.battery <= robot0.batteryCapacity) then
        bat4.loadRate
    else if(
      robot0.posX >= (bat7.dockingX - 2) and robot0.posX <= (bat7.dockingX + 2)
      and robot0.posY >= (bat7.dockingY - 2) and robot0.posY <= (bat7.dockingY + 2)
      and robot0.rot >= (bat7.dockingRot - 5) and robot0.rot <= (bat7.dockingRot +5)
      and bat7CTRL.instruction >= 0.5 and robot0.battery <= robot0.batteryCapacity) then
        bat7.loadRate
      else if(robot0.battery >= 0) then batteryLossRate else 0;
    der(robot1.battery) = if(
      robot1.posX >= (bat4.dockingX - 2) and robot1.posX <= (bat4.dockingX + 2)
      and robot1.posY >= (bat4.dockingY - 2) and robot1.posY <= (bat4.dockingY + 2)
      and robot1.rot >= (bat4.dockingRot - 5) and robot1.rot <= (bat4.dockingRot +5)
      and bat4CTRL.instruction >= 0.5 and robot1.battery <= robot1.batteryCapacity) then
        bat4.loadRate
    else if(
      robot1.posX >= (bat7.dockingX - 2) and robot1.posX <= (bat7.dockingX + 2)
      and robot1.posY >= (bat7.dockingY - 2) and robot1.posY <= (bat7.dockingY + 2)
      and robot1.rot >= (bat7.dockingRot - 5) and robot1.rot <= (bat7.dockingRot +5)
      and bat7CTRL.instruction >= 0.5 and robot1.battery <= robot1.batteryCapacity) then
        bat7.loadRate
      else if(robot1.battery >= 0) then batteryLossRate else 0;
    der(robot2.battery) = if(
      robot2.posX >= (bat4.dockingX - 2) and robot2.posX <= (bat4.dockingX + 2)
      and robot2.posY >= (bat4.dockingY - 2) and robot2.posY <= (bat4.dockingY + 2)
      and robot2.rot >= (bat4.dockingRot - 5) and robot2.rot <= (bat4.dockingRot +5)
      and bat4CTRL.instruction >= 0.5 and robot2.battery <= robot2.batteryCapacity) then
        bat4.loadRate
    else if(
      robot2.posX >= (bat7.dockingX - 2) and robot2.posX <= (bat7.dockingX + 2)
      and robot2.posY >= (bat7.dockingY - 2) and robot2.posY <= (bat7.dockingY + 2)
      and robot2.rot >= (bat7.dockingRot - 5) and robot2.rot <= (bat7.dockingRot +5)
      and bat7CTRL.instruction >= 0.5 and robot2.battery <= robot2.batteryCapacity) then
        bat7.loadRate
      else if(robot2.battery >= 0) then batteryLossRate else 0;
    der(robot3.battery) = if(
      robot3.posX >= (bat4.dockingX - 2) and robot3.posX <= (bat4.dockingX + 2)
      and robot3.posY >= (bat4.dockingY - 2) and robot3.posY <= (bat4.dockingY + 2)
      and robot3.rot >= (bat4.dockingRot - 5) and robot3.rot <= (bat4.dockingRot +5)
      and bat4CTRL.instruction >= 0.5 and robot3.battery <= robot3.batteryCapacity) then
        bat4.loadRate
    else if(
      robot3.posX >= (bat7.dockingX - 2) and robot3.posX <= (bat7.dockingX + 2)
      and robot3.posY >= (bat7.dockingY - 2) and robot3.posY <= (bat7.dockingY + 2)
      and robot3.rot >= (bat7.dockingRot - 5) and robot3.rot <= (bat7.dockingRot +5)
      and bat7CTRL.instruction >= 0.5 and robot3.battery <= robot3.batteryCapacity) then
        bat7.loadRate
      else if(robot3.battery >= 0) then batteryLossRate else 0;
    der(robot4.battery) = if(
      robot4.posX >= (bat4.dockingX - 2) and robot4.posX <= (bat4.dockingX + 2)
      and robot4.posY >= (bat4.dockingY - 2) and robot4.posY <= (bat4.dockingY + 2)
      and robot4.rot >= (bat4.dockingRot - 5) and robot4.rot <= (bat4.dockingRot +5)
      and bat4CTRL.instruction >= 0.5 and robot4.battery <= robot4.batteryCapacity) then
        bat4.loadRate
    else if(
      robot4.posX >= (bat7.dockingX - 2) and robot4.posX <= (bat7.dockingX + 2)
      and robot4.posY >= (bat7.dockingY - 2) and robot4.posY <= (bat7.dockingY + 2)
      and robot4.rot >= (bat7.dockingRot - 5) and robot4.rot <= (bat7.dockingRot +5)
      and bat7CTRL.instruction >= 0.5 and robot4.battery <= robot4.batteryCapacity) then
        bat7.loadRate
      else if(robot4.battery >= 0) then batteryLossRate else 0;


    der(col1.fillRate_1) = if (col1CTRL.instruction[1] > 0 and col1.fillRate_1 <= col1CTRL.fillRate_1) then 0.5 else 
                                         if (col1CTRL.instruction[1] <= 0 and col1.fillRate_1 >= 0) then -0.5 else
                                         0;
    der(col1.fillRate_2) = if (col1CTRL.instruction[2] > 0 and col1.fillRate_2 <= col1CTRL.fillRate_2) then 0.5 else 
                                         if (col1CTRL.instruction[2] <= 0 and col1.fillRate_2 >= 0) then -0.5 else
                                         0;
    connect(col1.con, control.col1Con);
    connect(col1.rat_1, control.col1FRate_1);
    connect(col1.rat_2, control.col1FRate_2);
    der(col3.fillRate_1) = if (col3CTRL.instruction[1] > 0 and col3.fillRate_1 <= col3CTRL.fillRate_1) then 0.5 else 
                                         if (col3CTRL.instruction[1] <= 0 and col3.fillRate_1 >= 0) then -0.5 else
                                         0;
    der(col3.fillRate_2) = if (col3CTRL.instruction[2] > 0 and col3.fillRate_2 <= col3CTRL.fillRate_2) then 0.5 else 
                                         if (col3CTRL.instruction[2] <= 0 and col3.fillRate_2 >= 0) then -0.5 else
                                         0;
    connect(col3.con, control.col3Con);
    connect(col3.rat_1, control.col3FRate_1);
    connect(col3.rat_2, control.col3FRate_2);
    der(bat4.loadRate) = if (bat4CTRL.instruction > 0 and bat4.loadRate <= bat4CTRL.loadRate) then 0.01 else 
                                                          if (bat4CTRL.instruction <= 0 and bat4.loadRate >= 0) then -0.01 else 0;
    connect(bat4.rat, control.bat4LRate);
    der(bat7.loadRate) = if (bat7CTRL.instruction > 0 and bat7.loadRate <= bat7CTRL.loadRate) then 0.01 else 
                                                          if (bat7CTRL.instruction <= 0 and bat7.loadRate >= 0) then -0.01 else 0;
    connect(bat7.rat, control.bat7LRate);
    der(mix5.rotating) = if (mix5CTRL.instruction[3] > 0 and mix5.rotating <=1 ) then 10 else
                                         if (mix5CTRL.instruction[3] <= 0 and mix5.rotating > 0 ) then -10 else
                                         0;
    der(mix5.fillRate) = if (mix5CTRL.instruction[2] > 0 and mix5.fillRate <= mix5CTRL.fillRate) then 0.01 else 
                                         if (mix5CTRL.instruction[2] <= 0 and mix5.fillRate > 0) then -0.01 else
                                         0;
    mix5.armVelocity = if (mix5CTRL.instruction[1] > 0) then
                       (if(mix5CTRL.armVelocity > 0 and mix5.altitude < (mix5CTRL.positions[integer(mix5CTRL.instruction[1])]-0.01)) then mix5CTRL.armVelocity else 
                       if (mix5CTRL.armVelocity < 0 and mix5.altitude > (mix5CTRL.positions[integer(mix5CTRL.instruction[1])]+0.01)) then mix5CTRL.armVelocity else 
                       0) else 0;
    connect(mix5.con, control.mix5Con);
    connect(mix5.alt, control.mix5Alt);
    connect(mix5.rot, control.mix5RRate);
    connect(mix5.fillState, control.mix5Sensor);
    connect(mix5.rat, control.mix5FRate);
    two6.armVelocity[1] = if (two6CTRL.instruction[1] > 0) then
                          (if(two6CTRL.armVelocity[1] > 0 and two6.altitude < (two6CTRL.positions[integer(two6CTRL.instruction[1])]-0.01)) then two6CTRL.armVelocity[1] else 
                          if (two6CTRL.armVelocity[1] < 0 and two6.altitude > (two6CTRL.positions[integer(two6CTRL.instruction[1])]+0.01)) then two6CTRL.armVelocity[1] else 
                          0) else 0;
    two6.armVelocity[2] = if (two6CTRL.instruction[2] > 0) then
                          (if(two6.traverse < (two6CTRL.holdPositions[integer(two6CTRL.instruction[2])]-0.01) and two6CTRL.armVelocity[2] > 0) then two6CTRL.armVelocity[2] else 
                          if (two6.traverse > (two6CTRL.holdPositions[integer(two6CTRL.instruction[2])]+0.01) and two6CTRL.armVelocity[2] < 0) then two6CTRL.armVelocity[2] else 
                          0) else 0;
    connect(two6.con[1], control.two6Con1);
    connect(two6.con[2], control.two6Con2);
    connect(two6.con[3], control.two6Con3);
    connect(two6.con[4], control.two6Con4);
    connect(two6.con[5], control.two6Con5);
    connect(two6.con[6], control.two6Con6);
    connect(two6.con[7], control.two6Con7);
    connect(two6.alt, control.two6Alt);
    connect(two6.tra, control.two6Tra);


     der(mix5.fillStateSensor) = 
         if(con1.id == mix5.containerID) then 
             (if (mix5.altitude < (mix5CTRL.positions[5] - con1.curFillStateWater) and mix5.fillStateSensor <= mix5CTRL.fillStateStages[1]) then 0 else 
             if (mix5.altitude < (mix5CTRL.positions[5] - con1.curFillStateWater) and mix5.fillStateSensor >= mix5CTRL.fillStateStages[1]) then -5 else 
             if (mix5.altitude > (mix5CTRL.positions[5] - con1.curFillStateWater) and mix5.altitude < (mix5CTRL.positions[5] - con1.curFillStateHard) and mix5.fillStateSensor <= mix5CTRL.fillStateStages[2]) then 5 else 
             if(mix5.fillStateSensor <= mix5CTRL.fillStateStages[3] and mix5.altitude < (mix5CTRL.positions[5] - con1.curFillStateHard)) then 5 else 0) 
         else
         if(con2.id == mix5.containerID) then 
             (if (mix5.altitude < (mix5CTRL.positions[5] - con2.curFillStateWater) and mix5.fillStateSensor <= mix5CTRL.fillStateStages[1]) then 0 else 
             if (mix5.altitude < (mix5CTRL.positions[5] - con2.curFillStateWater) and mix5.fillStateSensor >= mix5CTRL.fillStateStages[1]) then -5 else 
             if (mix5.altitude > (mix5CTRL.positions[5] - con2.curFillStateWater) and mix5.altitude < (mix5CTRL.positions[5] - con2.curFillStateHard) and mix5.fillStateSensor <= mix5CTRL.fillStateStages[2]) then 5 else 
             if(mix5.fillStateSensor <= mix5CTRL.fillStateStages[3] and mix5.altitude < (mix5CTRL.positions[5] - con2.curFillStateHard)) then 5 else 0) 
         else
         if(con3.id == mix5.containerID) then 
             (if (mix5.altitude < (mix5CTRL.positions[5] - con3.curFillStateWater) and mix5.fillStateSensor <= mix5CTRL.fillStateStages[1]) then 0 else 
             if (mix5.altitude < (mix5CTRL.positions[5] - con3.curFillStateWater) and mix5.fillStateSensor >= mix5CTRL.fillStateStages[1]) then -5 else 
             if (mix5.altitude > (mix5CTRL.positions[5] - con3.curFillStateWater) and mix5.altitude < (mix5CTRL.positions[5] - con3.curFillStateHard) and mix5.fillStateSensor <= mix5CTRL.fillStateStages[2]) then 5 else 
             if(mix5.fillStateSensor <= mix5CTRL.fillStateStages[3] and mix5.altitude < (mix5CTRL.positions[5] - con3.curFillStateHard)) then 5 else 0) 
         else
         if(con4.id == mix5.containerID) then 
             (if (mix5.altitude < (mix5CTRL.positions[5] - con4.curFillStateWater) and mix5.fillStateSensor <= mix5CTRL.fillStateStages[1]) then 0 else 
             if (mix5.altitude < (mix5CTRL.positions[5] - con4.curFillStateWater) and mix5.fillStateSensor >= mix5CTRL.fillStateStages[1]) then -5 else 
             if (mix5.altitude > (mix5CTRL.positions[5] - con4.curFillStateWater) and mix5.altitude < (mix5CTRL.positions[5] - con4.curFillStateHard) and mix5.fillStateSensor <= mix5CTRL.fillStateStages[2]) then 5 else 
             if(mix5.fillStateSensor <= mix5CTRL.fillStateStages[3] and mix5.altitude < (mix5CTRL.positions[5] - con4.curFillStateHard)) then 5 else 0) 
         else
         if(con5.id == mix5.containerID) then 
             (if (mix5.altitude < (mix5CTRL.positions[5] - con5.curFillStateWater) and mix5.fillStateSensor <= mix5CTRL.fillStateStages[1]) then 0 else 
             if (mix5.altitude < (mix5CTRL.positions[5] - con5.curFillStateWater) and mix5.fillStateSensor >= mix5CTRL.fillStateStages[1]) then -5 else 
             if (mix5.altitude > (mix5CTRL.positions[5] - con5.curFillStateWater) and mix5.altitude < (mix5CTRL.positions[5] - con5.curFillStateHard) and mix5.fillStateSensor <= mix5CTRL.fillStateStages[2]) then 5 else 
             if(mix5.fillStateSensor <= mix5CTRL.fillStateStages[3] and mix5.altitude < (mix5CTRL.positions[5] - con5.curFillStateHard)) then 5 else 0) 
         else
         if(con6.id == mix5.containerID) then 
             (if (mix5.altitude < (mix5CTRL.positions[5] - con6.curFillStateWater) and mix5.fillStateSensor <= mix5CTRL.fillStateStages[1]) then 0 else 
             if (mix5.altitude < (mix5CTRL.positions[5] - con6.curFillStateWater) and mix5.fillStateSensor >= mix5CTRL.fillStateStages[1]) then -5 else 
             if (mix5.altitude > (mix5CTRL.positions[5] - con6.curFillStateWater) and mix5.altitude < (mix5CTRL.positions[5] - con6.curFillStateHard) and mix5.fillStateSensor <= mix5CTRL.fillStateStages[2]) then 5 else 
             if(mix5.fillStateSensor <= mix5CTRL.fillStateStages[3] and mix5.altitude < (mix5CTRL.positions[5] - con6.curFillStateHard)) then 5 else 0) 
         else
     if(mix5.fillStateSensor > 0) then -5 else 0;


    connect(con1.fillHard, control.con1HRate);
    connect(con1.fillWater, control.con1WRate);
    for i in 1:5 loop
      for j in 1:4 loop
        connect(con1.layerVol[i,j], control.con1LayerVol[i,j]);
      end for;
    end for;
    connect(con2.fillHard, control.con2HRate);
    connect(con2.fillWater, control.con2WRate);
    for i in 1:5 loop
      for j in 1:4 loop
        connect(con2.layerVol[i,j], control.con2LayerVol[i,j]);
      end for;
    end for;
    connect(con3.fillHard, control.con3HRate);
    connect(con3.fillWater, control.con3WRate);
    for i in 1:5 loop
      for j in 1:4 loop
        connect(con3.layerVol[i,j], control.con3LayerVol[i,j]);
      end for;
    end for;
    connect(con4.fillHard, control.con4HRate);
    connect(con4.fillWater, control.con4WRate);
    for i in 1:5 loop
      for j in 1:4 loop
        connect(con4.layerVol[i,j], control.con4LayerVol[i,j]);
      end for;
    end for;
    connect(con5.fillHard, control.con5HRate);
    connect(con5.fillWater, control.con5WRate);
    for i in 1:5 loop
      for j in 1:4 loop
        connect(con5.layerVol[i,j], control.con5LayerVol[i,j]);
      end for;
    end for;
    connect(con6.fillHard, control.con6HRate);
    connect(con6.fillWater, control.con6WRate);
    for i in 1:5 loop
      for j in 1:4 loop
        connect(con6.layerVol[i,j], control.con6LayerVol[i,j]);
      end for;
    end for;


    two6CTRL.instruction[1] = control.digitalOutputs[11];
    two6CTRL.instruction[2] = control.digitalOutputs[12];
    two6CTRL.instruction[3] = control.digitalOutputs[13];
    two6CTRL.armVelocity[1] = control.digitalOutputs[14];
    two6CTRL.armVelocity[2] = control.digitalOutputs[15];
    mix5CTRL.instruction[1] = control.digitalOutputs[16];
    mix5CTRL.instruction[2] = control.digitalOutputs[17];
    mix5CTRL.instruction[3] = control.digitalOutputs[18];
    mix5CTRL.instruction[4] = control.digitalOutputs[19];
    mix5CTRL.armVelocity = control.digitalOutputs[20];
    col1CTRL.instruction[1] = control.digitalOutputs[21];
    col1CTRL.instruction[2] = control.digitalOutputs[22];
    col3CTRL.instruction[1] = control.digitalOutputs[23];
    col3CTRL.instruction[2] = control.digitalOutputs[24];
    bat4CTRL.instruction = control.digitalOutputs[25];
    bat7CTRL.instruction = control.digitalOutputs[26];


  algorithm
    if(control.sampled) then
    if (
      robot0.posX >= (mix5.dockingX - 5) and robot0.posX <= (mix5.dockingX + 5)
      and robot0.posY >= (mix5.dockingY - 5) and robot0.posY <= (mix5.dockingY + 5)
      and (if(mix5.dockingRot < 4 or mix5.dockingRot > 357) then (if (robot0.rot >= 355 or robot0.rot <= 5) then true else false)
           else (if(robot0.rot >= (mix5.dockingRot - 5) and robot0.rot <= (mix5.dockingRot +5)) then true else false))
      and mix5.altitude <= mix5CTRL.positions[1] +0.05) then
        if(mix5CTRL.instruction[4] >= 0.5 and robot0.container > 0) then
          robot0ConTmp := robot0.container;
            robot0.container :=0;
        elseif (mix5CTRL.instruction[4] <= -0.5 and robot0.container == 0) then
          robot0.container := mix5.containerID;
        end if;
    elseif (robot0.posX >= (two6.dockingX - 5) and robot0.posX <= (two6.dockingX + 5)
      and robot0.posY >= (two6.dockingY - 5) and robot0.posY <= (two6.dockingY + 5)
      and (if(two6.dockingRot < 4 or two6.dockingRot > 357) then (if (robot0.rot >= 355 or robot0.rot <= 5) then true else false)
           else (if(robot0.rot >= (two6.dockingRot - 5) and robot0.rot <= (two6.dockingRot +5)) then true else false))
      and two6.altitude <= two6CTRL.positions[1] +0.05) then
        if( two6CTRL.instruction[3] >= 0.5 and robot0.container > 0) then
          robot0ConTmp := robot0.container;
          robot0.container :=0;
        elseif (two6CTRL.instruction[3] <= -0.5 and robot0.container == 0) then
          robot0.container := two6.containerID[1];
        end if;
    end if;
    if (
      robot1.posX >= (mix5.dockingX - 5) and robot1.posX <= (mix5.dockingX + 5)
      and robot1.posY >= (mix5.dockingY - 5) and robot1.posY <= (mix5.dockingY + 5)
      and (if(mix5.dockingRot < 4 or mix5.dockingRot > 357) then (if (robot1.rot >= 355 or robot1.rot <= 5) then true else false)
           else (if(robot1.rot >= (mix5.dockingRot - 5) and robot1.rot <= (mix5.dockingRot +5)) then true else false))
      and mix5.altitude <= mix5CTRL.positions[1] +0.05) then
        if(mix5CTRL.instruction[4] >= 0.5 and robot1.container > 0) then
          robot1ConTmp := robot1.container;
            robot1.container :=0;
        elseif (mix5CTRL.instruction[4] <= -0.5 and robot1.container == 0) then
          robot1.container := mix5.containerID;
        end if;
    elseif (robot1.posX >= (two6.dockingX - 5) and robot1.posX <= (two6.dockingX + 5)
      and robot1.posY >= (two6.dockingY - 5) and robot1.posY <= (two6.dockingY + 5)
      and (if(two6.dockingRot < 4 or two6.dockingRot > 357) then (if (robot1.rot >= 355 or robot1.rot <= 5) then true else false)
           else (if(robot1.rot >= (two6.dockingRot - 5) and robot1.rot <= (two6.dockingRot +5)) then true else false))
      and two6.altitude <= two6CTRL.positions[1] +0.05) then
        if( two6CTRL.instruction[3] >= 0.5 and robot1.container > 0) then
          robot1ConTmp := robot1.container;
          robot1.container :=0;
        elseif (two6CTRL.instruction[3] <= -0.5 and robot1.container == 0) then
          robot1.container := two6.containerID[1];
        end if;
    end if;
    if (
      robot2.posX >= (mix5.dockingX - 5) and robot2.posX <= (mix5.dockingX + 5)
      and robot2.posY >= (mix5.dockingY - 5) and robot2.posY <= (mix5.dockingY + 5)
      and (if(mix5.dockingRot < 4 or mix5.dockingRot > 357) then (if (robot2.rot >= 355 or robot2.rot <= 5) then true else false)
           else (if(robot2.rot >= (mix5.dockingRot - 5) and robot2.rot <= (mix5.dockingRot +5)) then true else false))
      and mix5.altitude <= mix5CTRL.positions[1] +0.05) then
        if(mix5CTRL.instruction[4] >= 0.5 and robot2.container > 0) then
          robot2ConTmp := robot2.container;
            robot2.container :=0;
        elseif (mix5CTRL.instruction[4] <= -0.5 and robot2.container == 0) then
          robot2.container := mix5.containerID;
        end if;
    elseif (robot2.posX >= (two6.dockingX - 5) and robot2.posX <= (two6.dockingX + 5)
      and robot2.posY >= (two6.dockingY - 5) and robot2.posY <= (two6.dockingY + 5)
      and (if(two6.dockingRot < 4 or two6.dockingRot > 357) then (if (robot2.rot >= 355 or robot2.rot <= 5) then true else false)
           else (if(robot2.rot >= (two6.dockingRot - 5) and robot2.rot <= (two6.dockingRot +5)) then true else false))
      and two6.altitude <= two6CTRL.positions[1] +0.05) then
        if( two6CTRL.instruction[3] >= 0.5 and robot2.container > 0) then
          robot2ConTmp := robot2.container;
          robot2.container :=0;
        elseif (two6CTRL.instruction[3] <= -0.5 and robot2.container == 0) then
          robot2.container := two6.containerID[1];
        end if;
    end if;
    if (
      robot3.posX >= (mix5.dockingX - 5) and robot3.posX <= (mix5.dockingX + 5)
      and robot3.posY >= (mix5.dockingY - 5) and robot3.posY <= (mix5.dockingY + 5)
      and (if(mix5.dockingRot < 4 or mix5.dockingRot > 357) then (if (robot3.rot >= 355 or robot3.rot <= 5) then true else false)
           else (if(robot3.rot >= (mix5.dockingRot - 5) and robot3.rot <= (mix5.dockingRot +5)) then true else false))
      and mix5.altitude <= mix5CTRL.positions[1] +0.05) then
        if(mix5CTRL.instruction[4] >= 0.5 and robot3.container > 0) then
          robot3ConTmp := robot3.container;
            robot3.container :=0;
        elseif (mix5CTRL.instruction[4] <= -0.5 and robot3.container == 0) then
          robot3.container := mix5.containerID;
        end if;
    elseif (robot3.posX >= (two6.dockingX - 5) and robot3.posX <= (two6.dockingX + 5)
      and robot3.posY >= (two6.dockingY - 5) and robot3.posY <= (two6.dockingY + 5)
      and (if(two6.dockingRot < 4 or two6.dockingRot > 357) then (if (robot3.rot >= 355 or robot3.rot <= 5) then true else false)
           else (if(robot3.rot >= (two6.dockingRot - 5) and robot3.rot <= (two6.dockingRot +5)) then true else false))
      and two6.altitude <= two6CTRL.positions[1] +0.05) then
        if( two6CTRL.instruction[3] >= 0.5 and robot3.container > 0) then
          robot3ConTmp := robot3.container;
          robot3.container :=0;
        elseif (two6CTRL.instruction[3] <= -0.5 and robot3.container == 0) then
          robot3.container := two6.containerID[1];
        end if;
    end if;
    if (
      robot4.posX >= (mix5.dockingX - 5) and robot4.posX <= (mix5.dockingX + 5)
      and robot4.posY >= (mix5.dockingY - 5) and robot4.posY <= (mix5.dockingY + 5)
      and (if(mix5.dockingRot < 4 or mix5.dockingRot > 357) then (if (robot4.rot >= 355 or robot4.rot <= 5) then true else false)
           else (if(robot4.rot >= (mix5.dockingRot - 5) and robot4.rot <= (mix5.dockingRot +5)) then true else false))
      and mix5.altitude <= mix5CTRL.positions[1] +0.05) then
        if(mix5CTRL.instruction[4] >= 0.5 and robot4.container > 0) then
          robot4ConTmp := robot4.container;
            robot4.container :=0;
        elseif (mix5CTRL.instruction[4] <= -0.5 and robot4.container == 0) then
          robot4.container := mix5.containerID;
        end if;
    elseif (robot4.posX >= (two6.dockingX - 5) and robot4.posX <= (two6.dockingX + 5)
      and robot4.posY >= (two6.dockingY - 5) and robot4.posY <= (two6.dockingY + 5)
      and (if(two6.dockingRot < 4 or two6.dockingRot > 357) then (if (robot4.rot >= 355 or robot4.rot <= 5) then true else false)
           else (if(robot4.rot >= (two6.dockingRot - 5) and robot4.rot <= (two6.dockingRot +5)) then true else false))
      and two6.altitude <= two6CTRL.positions[1] +0.05) then
        if( two6CTRL.instruction[3] >= 0.5 and robot4.container > 0) then
          robot4ConTmp := robot4.container;
          robot4.container :=0;
        elseif (two6CTRL.instruction[3] <= -0.5 and robot4.container == 0) then
          robot4.container := two6.containerID[1];
        end if;
    end if;


    if (
      robot0.posX >= (col1.dockingX - 5) and robot0.posX <= (col1.dockingX + 5)
      and robot0.posY >= (col1.dockingY - 5) and robot0.posY <= (col1.dockingY + 5)
      and (if(col1.dockingRot < 4 or col1.dockingRot > 357) then (if (robot0.rot >= 355 or robot0.rot <= 5) then true else false)
           else (if(robot0.rot >= (col1.dockingRot - 5) and robot0.rot <= (col1.dockingRot +5)) then true else false))
      ) then
        if(col1.containerID ==0) then
          col1.containerID := robot0.container;
        end if;
    elseif (robot1.posX >= (col1.dockingX - 5) and robot1.posX <= (col1.dockingX + 5)
      and robot1.posY >= (col1.dockingY - 5) and robot1.posY <= (col1.dockingY + 5)
      and (if(col1.dockingRot < 4 or col1.dockingRot > 357) then (if (robot1.rot >= 355 or robot1.rot <= 5) then true else false)
           else (if(robot1.rot >= (col1.dockingRot - 5) and robot1.rot <= (col1.dockingRot +5)) then true else false))
      ) then
        if(col1.containerID ==0) then
          col1.containerID := robot1.container;
        end if;
    elseif (robot2.posX >= (col1.dockingX - 5) and robot2.posX <= (col1.dockingX + 5)
      and robot2.posY >= (col1.dockingY - 5) and robot2.posY <= (col1.dockingY + 5)
      and (if(col1.dockingRot < 4 or col1.dockingRot > 357) then (if (robot2.rot >= 355 or robot2.rot <= 5) then true else false)
           else (if(robot2.rot >= (col1.dockingRot - 5) and robot2.rot <= (col1.dockingRot +5)) then true else false))
      ) then
        if(col1.containerID ==0) then
          col1.containerID := robot2.container;
        end if;
    elseif (robot3.posX >= (col1.dockingX - 5) and robot3.posX <= (col1.dockingX + 5)
      and robot3.posY >= (col1.dockingY - 5) and robot3.posY <= (col1.dockingY + 5)
      and (if(col1.dockingRot < 4 or col1.dockingRot > 357) then (if (robot3.rot >= 355 or robot3.rot <= 5) then true else false)
           else (if(robot3.rot >= (col1.dockingRot - 5) and robot3.rot <= (col1.dockingRot +5)) then true else false))
      ) then
        if(col1.containerID ==0) then
          col1.containerID := robot3.container;
        end if;
    elseif (robot4.posX >= (col1.dockingX - 5) and robot4.posX <= (col1.dockingX + 5)
      and robot4.posY >= (col1.dockingY - 5) and robot4.posY <= (col1.dockingY + 5)
      and (if(col1.dockingRot < 4 or col1.dockingRot > 357) then (if (robot4.rot >= 355 or robot4.rot <= 5) then true else false)
           else (if(robot4.rot >= (col1.dockingRot - 5) and robot4.rot <= (col1.dockingRot +5)) then true else false))
      ) then
        if(col1.containerID ==0) then
          col1.containerID := robot4.container;
        end if;
    else
      col1.containerID := 0;
    end if;
    if (
      robot0.posX >= (col3.dockingX - 5) and robot0.posX <= (col3.dockingX + 5)
      and robot0.posY >= (col3.dockingY - 5) and robot0.posY <= (col3.dockingY + 5)
      and (if(col3.dockingRot < 4 or col3.dockingRot > 357) then (if (robot0.rot >= 355 or robot0.rot <= 5) then true else false)
           else (if(robot0.rot >= (col3.dockingRot - 5) and robot0.rot <= (col3.dockingRot +5)) then true else false))
      ) then
        if(col3.containerID ==0) then
          col3.containerID := robot0.container;
        end if;
    elseif (robot1.posX >= (col3.dockingX - 5) and robot1.posX <= (col3.dockingX + 5)
      and robot1.posY >= (col3.dockingY - 5) and robot1.posY <= (col3.dockingY + 5)
      and (if(col3.dockingRot < 4 or col3.dockingRot > 357) then (if (robot1.rot >= 355 or robot1.rot <= 5) then true else false)
           else (if(robot1.rot >= (col3.dockingRot - 5) and robot1.rot <= (col3.dockingRot +5)) then true else false))
      ) then
        if(col3.containerID ==0) then
          col3.containerID := robot1.container;
        end if;
    elseif (robot2.posX >= (col3.dockingX - 5) and robot2.posX <= (col3.dockingX + 5)
      and robot2.posY >= (col3.dockingY - 5) and robot2.posY <= (col3.dockingY + 5)
      and (if(col3.dockingRot < 4 or col3.dockingRot > 357) then (if (robot2.rot >= 355 or robot2.rot <= 5) then true else false)
           else (if(robot2.rot >= (col3.dockingRot - 5) and robot2.rot <= (col3.dockingRot +5)) then true else false))
      ) then
        if(col3.containerID ==0) then
          col3.containerID := robot2.container;
        end if;
    elseif (robot3.posX >= (col3.dockingX - 5) and robot3.posX <= (col3.dockingX + 5)
      and robot3.posY >= (col3.dockingY - 5) and robot3.posY <= (col3.dockingY + 5)
      and (if(col3.dockingRot < 4 or col3.dockingRot > 357) then (if (robot3.rot >= 355 or robot3.rot <= 5) then true else false)
           else (if(robot3.rot >= (col3.dockingRot - 5) and robot3.rot <= (col3.dockingRot +5)) then true else false))
      ) then
        if(col3.containerID ==0) then
          col3.containerID := robot3.container;
        end if;
    elseif (robot4.posX >= (col3.dockingX - 5) and robot4.posX <= (col3.dockingX + 5)
      and robot4.posY >= (col3.dockingY - 5) and robot4.posY <= (col3.dockingY + 5)
      and (if(col3.dockingRot < 4 or col3.dockingRot > 357) then (if (robot4.rot >= 355 or robot4.rot <= 5) then true else false)
           else (if(robot4.rot >= (col3.dockingRot - 5) and robot4.rot <= (col3.dockingRot +5)) then true else false))
      ) then
        if(col3.containerID ==0) then
          col3.containerID := robot4.container;
        end if;
    else
      col3.containerID := 0;
    end if;
    if (
      robot0.posX >= (mix5.dockingX - 5) and robot0.posX <= (mix5.dockingX + 5)
      and robot0.posY >= (mix5.dockingY - 5) and robot0.posY <= (mix5.dockingY + 5)
      and (if(mix5.dockingRot < 4 or mix5.dockingRot > 357) then (if (robot0.rot >= 355 or robot0.rot <= 5) then true else false)
           else (if(robot0.rot >= (mix5.dockingRot - 5) and robot0.rot <= (mix5.dockingRot +5)) then true else false))
      and mix5.altitude <= mix5CTRL.positions[1] +0.05) then
        if(mix5CTRL.instruction[4] >= 0.5 and mix5.containerID == 0) then
          mix5.containerID := robot0ConTmp;
        elseif (mix5CTRL.instruction[4] <= -0.5 and mix5.containerID > 0) then
          mix5.containerID := 0;
        end if;
    elseif (robot1.posX >= (mix5.dockingX - 5) and robot1.posX <= (mix5.dockingX + 5)
      and robot1.posY >= (mix5.dockingY - 5) and robot1.posY <= (mix5.dockingY + 5)
      and (if(mix5.dockingRot < 4 or mix5.dockingRot > 357) then (if (robot1.rot >= 355 or robot1.rot <= 5) then true else false)
           else (if(robot1.rot >= (mix5.dockingRot - 5) and robot1.rot <= (mix5.dockingRot +5)) then true else false))
      and mix5.altitude <= mix5CTRL.positions[1] +0.05) then
        if(mix5CTRL.instruction[4] >= 0.5 and mix5.containerID == 0) then
          mix5.containerID := robot1ConTmp;
        elseif (mix5CTRL.instruction[4] <= -0.5 and mix5.containerID > 0) then
          mix5.containerID := 0;
        end if;
    elseif (robot2.posX >= (mix5.dockingX - 5) and robot2.posX <= (mix5.dockingX + 5)
      and robot2.posY >= (mix5.dockingY - 5) and robot2.posY <= (mix5.dockingY + 5)
      and (if(mix5.dockingRot < 4 or mix5.dockingRot > 357) then (if (robot2.rot >= 355 or robot2.rot <= 5) then true else false)
           else (if(robot2.rot >= (mix5.dockingRot - 5) and robot2.rot <= (mix5.dockingRot +5)) then true else false))
      and mix5.altitude <= mix5CTRL.positions[1] +0.05) then
        if(mix5CTRL.instruction[4] >= 0.5 and mix5.containerID == 0) then
          mix5.containerID := robot2ConTmp;
        elseif (mix5CTRL.instruction[4] <= -0.5 and mix5.containerID > 0) then
          mix5.containerID := 0;
        end if;
    elseif (robot3.posX >= (mix5.dockingX - 5) and robot3.posX <= (mix5.dockingX + 5)
      and robot3.posY >= (mix5.dockingY - 5) and robot3.posY <= (mix5.dockingY + 5)
      and (if(mix5.dockingRot < 4 or mix5.dockingRot > 357) then (if (robot3.rot >= 355 or robot3.rot <= 5) then true else false)
           else (if(robot3.rot >= (mix5.dockingRot - 5) and robot3.rot <= (mix5.dockingRot +5)) then true else false))
      and mix5.altitude <= mix5CTRL.positions[1] +0.05) then
        if(mix5CTRL.instruction[4] >= 0.5 and mix5.containerID == 0) then
          mix5.containerID := robot3ConTmp;
        elseif (mix5CTRL.instruction[4] <= -0.5 and mix5.containerID > 0) then
          mix5.containerID := 0;
        end if;
    elseif (robot4.posX >= (mix5.dockingX - 5) and robot4.posX <= (mix5.dockingX + 5)
      and robot4.posY >= (mix5.dockingY - 5) and robot4.posY <= (mix5.dockingY + 5)
      and (if(mix5.dockingRot < 4 or mix5.dockingRot > 357) then (if (robot4.rot >= 355 or robot4.rot <= 5) then true else false)
           else (if(robot4.rot >= (mix5.dockingRot - 5) and robot4.rot <= (mix5.dockingRot +5)) then true else false))
      and mix5.altitude <= mix5CTRL.positions[1] +0.05) then
        if(mix5CTRL.instruction[4] >= 0.5 and mix5.containerID == 0) then
          mix5.containerID := robot4ConTmp;
        elseif (mix5CTRL.instruction[4] <= -0.5 and mix5.containerID > 0) then
          mix5.containerID := 0;
        end if;
    end if;
    if (
      robot0.posX >= (two6.dockingX - 5) and robot0.posX <= (two6.dockingX + 5)
      and robot0.posY >= (two6.dockingY - 5) and robot0.posY <= (two6.dockingY + 5)
      and (if(two6.dockingRot < 4 or two6.dockingRot > 357) then (if (robot0.rot >= 355 or robot0.rot <= 5) then true else false)
           else (if(robot0.rot >= (two6.dockingRot - 5) and robot0.rot <= (two6.dockingRot +5)) then true else false))
      and two6.altitude <= two6CTRL.positions[1] +0.05) then
        if( two6CTRL.instruction[3] >= 0.5 and two6.containerID[1] == 0) then
          two6.containerID[1] := robot0ConTmp;
        elseif (two6CTRL.instruction[3] <= -0.5 and two6.containerID[1] > 0) then
          two6.containerID[1] := 0;
        end if;
    elseif (robot1.posX >= (two6.dockingX - 5) and robot1.posX <= (two6.dockingX + 5)
      and robot1.posY >= (two6.dockingY - 5) and robot1.posY <= (two6.dockingY + 5)
      and (if(two6.dockingRot < 4 or two6.dockingRot > 357) then (if (robot1.rot >= 355 or robot1.rot <= 5) then true else false)
           else (if(robot1.rot >= (two6.dockingRot - 5) and robot1.rot <= (two6.dockingRot +5)) then true else false))
      and two6.altitude <= two6CTRL.positions[1] +0.05) then
        if( two6CTRL.instruction[3] >= 0.5 and two6.containerID[1] == 0) then
          two6.containerID[1] := robot1ConTmp;
        elseif (two6CTRL.instruction[3] <= -0.5 and two6.containerID[1] > 0) then
          two6.containerID[1] := 0;
        end if;
    elseif (robot2.posX >= (two6.dockingX - 5) and robot2.posX <= (two6.dockingX + 5)
      and robot2.posY >= (two6.dockingY - 5) and robot2.posY <= (two6.dockingY + 5)
      and (if(two6.dockingRot < 4 or two6.dockingRot > 357) then (if (robot2.rot >= 355 or robot2.rot <= 5) then true else false)
           else (if(robot2.rot >= (two6.dockingRot - 5) and robot2.rot <= (two6.dockingRot +5)) then true else false))
      and two6.altitude <= two6CTRL.positions[1] +0.05) then
        if( two6CTRL.instruction[3] >= 0.5 and two6.containerID[1] == 0) then
          two6.containerID[1] := robot2ConTmp;
        elseif (two6CTRL.instruction[3] <= -0.5 and two6.containerID[1] > 0) then
          two6.containerID[1] := 0;
        end if;
    elseif (robot3.posX >= (two6.dockingX - 5) and robot3.posX <= (two6.dockingX + 5)
      and robot3.posY >= (two6.dockingY - 5) and robot3.posY <= (two6.dockingY + 5)
      and (if(two6.dockingRot < 4 or two6.dockingRot > 357) then (if (robot3.rot >= 355 or robot3.rot <= 5) then true else false)
           else (if(robot3.rot >= (two6.dockingRot - 5) and robot3.rot <= (two6.dockingRot +5)) then true else false))
      and two6.altitude <= two6CTRL.positions[1] +0.05) then
        if( two6CTRL.instruction[3] >= 0.5 and two6.containerID[1] == 0) then
          two6.containerID[1] := robot3ConTmp;
        elseif (two6CTRL.instruction[3] <= -0.5 and two6.containerID[1] > 0) then
          two6.containerID[1] := 0;
        end if;
    elseif (robot4.posX >= (two6.dockingX - 5) and robot4.posX <= (two6.dockingX + 5)
      and robot4.posY >= (two6.dockingY - 5) and robot4.posY <= (two6.dockingY + 5)
      and (if(two6.dockingRot < 4 or two6.dockingRot > 357) then (if (robot4.rot >= 355 or robot4.rot <= 5) then true else false)
           else (if(robot4.rot >= (two6.dockingRot - 5) and robot4.rot <= (two6.dockingRot +5)) then true else false))
      and two6.altitude <= two6CTRL.positions[1] +0.05) then
        if( two6CTRL.instruction[3] >= 0.5 and two6.containerID[1] == 0) then
          two6.containerID[1] := robot4ConTmp;
        elseif (two6CTRL.instruction[3] <= -0.5 and two6.containerID[1] > 0) then
          two6.containerID[1] := 0;
        end if;
    end if;

    if (two6.altitude >= two6CTRL.positions[3] -0.05) then
      if (two6.containerID[1] > 0 and two6CTRL.instruction[3] <= -0.5 and (two6.traverse > two6CTRL.holdPositions[1]+0.05 or two6.traverse < two6CTRL.holdPositions[1]-0.05)) then
        if (two6.traverse >= two6CTRL.holdPositions[2]-0.1 and two6.traverse <= two6CTRL.holdPositions[2]+0.1 and two6.containerID[2] <= 0.1) then
          two6.containerID[2] := two6.containerID[1];
          two6.containerID[1] := 0;
        elseif (two6.traverse >= two6CTRL.holdPositions[3]-0.1 and two6.traverse <= two6CTRL.holdPositions[3]+0.1 and two6.containerID[3] <= 0.1) then
          two6.containerID[3] := two6.containerID[1];
          two6.containerID[1] := 0;
        elseif (two6.traverse >= two6CTRL.holdPositions[4]-0.1 and two6.traverse <= two6CTRL.holdPositions[4]+0.1 and two6.containerID[4] <= 0.1) then
          two6.containerID[4] := two6.containerID[1];
          two6.containerID[1] := 0;
        elseif (two6.traverse >= two6CTRL.holdPositions[5]-0.1 and two6.traverse <= two6CTRL.holdPositions[5]+0.1 and two6.containerID[5] <= 0.1) then
          two6.containerID[5] := two6.containerID[1];
          two6.containerID[1] := 0;
        elseif (two6.traverse >= two6CTRL.holdPositions[6]-0.1 and two6.traverse <= two6CTRL.holdPositions[6]+0.1 and two6.containerID[6] <= 0.1) then
          two6.containerID[6] := two6.containerID[1];
          two6.containerID[1] := 0;
        elseif (two6.traverse >= two6CTRL.holdPositions[7]-0.1 and two6.traverse <= two6CTRL.holdPositions[7]+0.1 and two6.containerID[7] <= 0.1) then
          two6.containerID[7] := two6.containerID[1];
          two6.containerID[1] := 0;
        end if;
      elseif (two6.containerID[1] < 0.1 and two6CTRL.instruction[3] >= 0.5 and (two6.traverse > two6CTRL.holdPositions[1]+0.05 or two6.traverse < two6CTRL.holdPositions[1]-0.05) then
        if (two6.traverse >= two6CTRL.holdPositions[2]-0.1 and two6.traverse <= two6CTRL.holdPositions[2]+0.1 and two6.containerID[2] > 0) then
          two6.containerID[1] := two6.containerID[2];
          two6.containerID[2] := 0;
        elseif (two6.traverse >= two6CTRL.holdPositions[3]-0.1 and two6.traverse <= two6CTRL.holdPositions[3]+0.1 and two6.containerID[3] > 0) then
          two6.containerID[1] := two6.containerID[3];
          two6.containerID[3] := 0;
        elseif (two6.traverse >= two6CTRL.holdPositions[4]-0.1 and two6.traverse <= two6CTRL.holdPositions[4]+0.1 and two6.containerID[4] > 0) then
          two6.containerID[1] := two6.containerID[4];
          two6.containerID[4] := 0;
        elseif (two6.traverse >= two6CTRL.holdPositions[5]-0.1 and two6.traverse <= two6CTRL.holdPositions[5]+0.1 and two6.containerID[5] > 0) then
          two6.containerID[1] := two6.containerID[5];
          two6.containerID[5] := 0;
        elseif (two6.traverse >= two6CTRL.holdPositions[6]-0.1 and two6.traverse <= two6CTRL.holdPositions[6]+0.1 and two6.containerID[6] > 0) then
          two6.containerID[1] := two6.containerID[6];
          two6.containerID[6] := 0;
        elseif (two6.traverse >= two6CTRL.holdPositions[7]-0.1 and two6.traverse <= two6CTRL.holdPositions[7]+0.1 and two6.containerID[7] > 0) then
          two6.containerID[1] := two6.containerID[7];
          two6.containerID[7] := 0;
        end if;
      end if;
    end if;
    end if;
  end Arena;



annotation(uses(Modelica(version = "3.0.1"), Socket(version = "1")), experiment(
StopTime = 240));

  package Control "control with the socket"

    connector CSharpOutput = output Modelica.Blocks.Interfaces.RealOutput;

  class ExternalControl
    "wrapping model to connect the actual control unit to the simulation using TCP sockets"
    Modelica.Blocks.Interfaces.RealInput digitalInputs[180];
    Modelica.Blocks.Interfaces.RealOutput digitalOutputs[26];
    PGWS1011.Communication.Socket socket(IPAddress=IPAddress, port=port);
    parameter String IPAddress="127.0.0.1" "IP-Address of Server";
    parameter Integer port=12345 "Port to connect to";
    parameter Modelica.SIunits.Time samplingRate=0.1;

    CSharpOutput robot0X;
    CSharpOutput robot0Y;
    CSharpOutput robot0Rot;
    CSharpOutput robot0Con;
    CSharpOutput robot0Bat;
    CSharpOutput robot1X;
    CSharpOutput robot1Y;
    CSharpOutput robot1Rot;
    CSharpOutput robot1Con;
    CSharpOutput robot1Bat;
    CSharpOutput robot2X;
    CSharpOutput robot2Y;
    CSharpOutput robot2Rot;
    CSharpOutput robot2Con;
    CSharpOutput robot2Bat;
    CSharpOutput robot3X;
    CSharpOutput robot3Y;
    CSharpOutput robot3Rot;
    CSharpOutput robot3Con;
    CSharpOutput robot3Bat;
    CSharpOutput robot4X;
    CSharpOutput robot4Y;
    CSharpOutput robot4Rot;
    CSharpOutput robot4Con;
    CSharpOutput robot4Bat;
    CSharpOutput col1Con;
    CSharpOutput col1FRate_1;
    CSharpOutput col1FRate_2;
    CSharpOutput col3Con;
    CSharpOutput col3FRate_1;
    CSharpOutput col3FRate_2;
    CSharpOutput bat4LRate;
    CSharpOutput bat7LRate;
    CSharpOutput mix5Con;
    CSharpOutput mix5RRate;
    CSharpOutput mix5Sensor;
    CSharpOutput mix5Alt;
    CSharpOutput mix5FRate;
    CSharpOutput two6Con1;
    CSharpOutput two6Con2;
    CSharpOutput two6Con3;
    CSharpOutput two6Con4;
    CSharpOutput two6Con5;
    CSharpOutput two6Con6;
    CSharpOutput two6Con7;
    CSharpOutput two6Alt;
    CSharpOutput two6Tra;
    CSharpOutput con1HRate;
    CSharpOutput con1WRate;
    CSharpOutput[5,4] con1LayerVol;
    CSharpOutput con2HRate;
    CSharpOutput con2WRate;
    CSharpOutput[5,4] con2LayerVol;
    CSharpOutput con3HRate;
    CSharpOutput con3WRate;
    CSharpOutput[5,4] con3LayerVol;
    CSharpOutput con4HRate;
    CSharpOutput con4WRate;
    CSharpOutput[5,4] con4LayerVol;
    CSharpOutput con5HRate;
    CSharpOutput con5WRate;
    CSharpOutput[5,4] con5LayerVol;
    CSharpOutput con6HRate;
    CSharpOutput con6WRate;
    CSharpOutput[5,4] con6LayerVol;

    Boolean sampled;
  equation
    digitalInputs[1] = robot0X;
    digitalInputs[2] = robot0Y;
    digitalInputs[3] = robot0Rot;
    digitalInputs[4] = robot0Con;
    digitalInputs[5] = robot0Bat;
    digitalInputs[6] = robot1X;
    digitalInputs[7] = robot1Y;
    digitalInputs[8] = robot1Rot;
    digitalInputs[9] = robot1Con;
    digitalInputs[10] = robot1Bat;
    digitalInputs[11] = robot2X;
    digitalInputs[12] = robot2Y;
    digitalInputs[13] = robot2Rot;
    digitalInputs[14] = robot2Con;
    digitalInputs[15] = robot2Bat;
    digitalInputs[16] = robot3X;
    digitalInputs[17] = robot3Y;
    digitalInputs[18] = robot3Rot;
    digitalInputs[19] = robot3Con;
    digitalInputs[20] = robot3Bat;
    digitalInputs[21] = robot4X;
    digitalInputs[22] = robot4Y;
    digitalInputs[23] = robot4Rot;
    digitalInputs[24] = robot4Con;
    digitalInputs[25] = robot4Bat;
    digitalInputs[26] = two6Con1;
    digitalInputs[27] = two6Con2;
    digitalInputs[28] = two6Con3;
    digitalInputs[29] = two6Con4;
    digitalInputs[30] = two6Con5;
    digitalInputs[31] = two6Con6;
    digitalInputs[32] = two6Con7;
    digitalInputs[33] = two6Alt;
    digitalInputs[34] = two6Tra;
    digitalInputs[35] = mix5Con;
    digitalInputs[36] = mix5RRate;
    digitalInputs[37] = mix5Sensor;
    digitalInputs[38] = mix5Alt;
    digitalInputs[39] = mix5FRate;
    digitalInputs[40] = col1Con;
    digitalInputs[41] = col1FRate_1;
    digitalInputs[42] = col1FRate_2;
    digitalInputs[43] = col3Con;
    digitalInputs[44] = col3FRate_1;
    digitalInputs[45] = col3FRate_2;
    digitalInputs[46] = bat4LRate;
    digitalInputs[47] = bat7LRate;
    digitalInputs[48] = con1HRate;
    digitalInputs[49] = con1WRate;
    digitalInputs[50] = con1LayerVol[1,1];
    digitalInputs[51] = con1LayerVol[1,2];
    digitalInputs[52] = con1LayerVol[1,3];
    digitalInputs[53] = con1LayerVol[1,4];
    digitalInputs[54] = con1LayerVol[2,1];
    digitalInputs[55] = con1LayerVol[2,2];
    digitalInputs[56] = con1LayerVol[2,3];
    digitalInputs[57] = con1LayerVol[2,4];
    digitalInputs[58] = con1LayerVol[3,1];
    digitalInputs[59] = con1LayerVol[3,2];
    digitalInputs[60] = con1LayerVol[3,3];
    digitalInputs[61] = con1LayerVol[3,4];
    digitalInputs[62] = con1LayerVol[4,1];
    digitalInputs[63] = con1LayerVol[4,2];
    digitalInputs[64] = con1LayerVol[4,3];
    digitalInputs[65] = con1LayerVol[4,4];
    digitalInputs[66] = con1LayerVol[5,1];
    digitalInputs[67] = con1LayerVol[5,2];
    digitalInputs[68] = con1LayerVol[5,3];
    digitalInputs[69] = con1LayerVol[5,4];
    digitalInputs[70] = con2HRate;
    digitalInputs[71] = con2WRate;
    digitalInputs[72] = con2LayerVol[1,1];
    digitalInputs[73] = con2LayerVol[1,2];
    digitalInputs[74] = con2LayerVol[1,3];
    digitalInputs[75] = con2LayerVol[1,4];
    digitalInputs[76] = con2LayerVol[2,1];
    digitalInputs[77] = con2LayerVol[2,2];
    digitalInputs[78] = con2LayerVol[2,3];
    digitalInputs[79] = con2LayerVol[2,4];
    digitalInputs[80] = con2LayerVol[3,1];
    digitalInputs[81] = con2LayerVol[3,2];
    digitalInputs[82] = con2LayerVol[3,3];
    digitalInputs[83] = con2LayerVol[3,4];
    digitalInputs[84] = con2LayerVol[4,1];
    digitalInputs[85] = con2LayerVol[4,2];
    digitalInputs[86] = con2LayerVol[4,3];
    digitalInputs[87] = con2LayerVol[4,4];
    digitalInputs[88] = con2LayerVol[5,1];
    digitalInputs[89] = con2LayerVol[5,2];
    digitalInputs[90] = con2LayerVol[5,3];
    digitalInputs[91] = con2LayerVol[5,4];
    digitalInputs[92] = con3HRate;
    digitalInputs[93] = con3WRate;
    digitalInputs[94] = con3LayerVol[1,1];
    digitalInputs[95] = con3LayerVol[1,2];
    digitalInputs[96] = con3LayerVol[1,3];
    digitalInputs[97] = con3LayerVol[1,4];
    digitalInputs[98] = con3LayerVol[2,1];
    digitalInputs[99] = con3LayerVol[2,2];
    digitalInputs[100] = con3LayerVol[2,3];
    digitalInputs[101] = con3LayerVol[2,4];
    digitalInputs[102] = con3LayerVol[3,1];
    digitalInputs[103] = con3LayerVol[3,2];
    digitalInputs[104] = con3LayerVol[3,3];
    digitalInputs[105] = con3LayerVol[3,4];
    digitalInputs[106] = con3LayerVol[4,1];
    digitalInputs[107] = con3LayerVol[4,2];
    digitalInputs[108] = con3LayerVol[4,3];
    digitalInputs[109] = con3LayerVol[4,4];
    digitalInputs[110] = con3LayerVol[5,1];
    digitalInputs[111] = con3LayerVol[5,2];
    digitalInputs[112] = con3LayerVol[5,3];
    digitalInputs[113] = con3LayerVol[5,4];
    digitalInputs[114] = con4HRate;
    digitalInputs[115] = con4WRate;
    digitalInputs[116] = con4LayerVol[1,1];
    digitalInputs[117] = con4LayerVol[1,2];
    digitalInputs[118] = con4LayerVol[1,3];
    digitalInputs[119] = con4LayerVol[1,4];
    digitalInputs[120] = con4LayerVol[2,1];
    digitalInputs[121] = con4LayerVol[2,2];
    digitalInputs[122] = con4LayerVol[2,3];
    digitalInputs[123] = con4LayerVol[2,4];
    digitalInputs[124] = con4LayerVol[3,1];
    digitalInputs[125] = con4LayerVol[3,2];
    digitalInputs[126] = con4LayerVol[3,3];
    digitalInputs[127] = con4LayerVol[3,4];
    digitalInputs[128] = con4LayerVol[4,1];
    digitalInputs[129] = con4LayerVol[4,2];
    digitalInputs[130] = con4LayerVol[4,3];
    digitalInputs[131] = con4LayerVol[4,4];
    digitalInputs[132] = con4LayerVol[5,1];
    digitalInputs[133] = con4LayerVol[5,2];
    digitalInputs[134] = con4LayerVol[5,3];
    digitalInputs[135] = con4LayerVol[5,4];
    digitalInputs[136] = con5HRate;
    digitalInputs[137] = con5WRate;
    digitalInputs[138] = con5LayerVol[1,1];
    digitalInputs[139] = con5LayerVol[1,2];
    digitalInputs[140] = con5LayerVol[1,3];
    digitalInputs[141] = con5LayerVol[1,4];
    digitalInputs[142] = con5LayerVol[2,1];
    digitalInputs[143] = con5LayerVol[2,2];
    digitalInputs[144] = con5LayerVol[2,3];
    digitalInputs[145] = con5LayerVol[2,4];
    digitalInputs[146] = con5LayerVol[3,1];
    digitalInputs[147] = con5LayerVol[3,2];
    digitalInputs[148] = con5LayerVol[3,3];
    digitalInputs[149] = con5LayerVol[3,4];
    digitalInputs[150] = con5LayerVol[4,1];
    digitalInputs[151] = con5LayerVol[4,2];
    digitalInputs[152] = con5LayerVol[4,3];
    digitalInputs[153] = con5LayerVol[4,4];
    digitalInputs[154] = con5LayerVol[5,1];
    digitalInputs[155] = con5LayerVol[5,2];
    digitalInputs[156] = con5LayerVol[5,3];
    digitalInputs[157] = con5LayerVol[5,4];
    digitalInputs[158] = con6HRate;
    digitalInputs[159] = con6WRate;
    digitalInputs[160] = con6LayerVol[1,1];
    digitalInputs[161] = con6LayerVol[1,2];
    digitalInputs[162] = con6LayerVol[1,3];
    digitalInputs[163] = con6LayerVol[1,4];
    digitalInputs[164] = con6LayerVol[2,1];
    digitalInputs[165] = con6LayerVol[2,2];
    digitalInputs[166] = con6LayerVol[2,3];
    digitalInputs[167] = con6LayerVol[2,4];
    digitalInputs[168] = con6LayerVol[3,1];
    digitalInputs[169] = con6LayerVol[3,2];
    digitalInputs[170] = con6LayerVol[3,3];
    digitalInputs[171] = con6LayerVol[3,4];
    digitalInputs[172] = con6LayerVol[4,1];
    digitalInputs[173] = con6LayerVol[4,2];
    digitalInputs[174] = con6LayerVol[4,3];
    digitalInputs[175] = con6LayerVol[4,4];
    digitalInputs[176] = con6LayerVol[5,1];
    digitalInputs[177] = con6LayerVol[5,2];
    digitalInputs[178] = con6LayerVol[5,3];
    digitalInputs[179] = con6LayerVol[5,4];
    digitalInputs[180] = time;
  algorithm
    sampled:=sample(0, samplingRate);
    when (sampled and time >0) then
      sendInputSignalVector(digitalInputs);
      digitalOutputs:=receiveOutputSignalVector();
    end when;
  end ExternalControl;

  function sendInputSignalVector
    "transform a vector of real signals to a string with comma seperated values and send the string"
    input Real inputSignals[180];
  protected
    String message;
    Integer roundUp;
    Real rounded;
  algorithm
    message:="";
    for i in 1:180 loop
      roundUp := integer(inputSignals[i] * 100);
      rounded := roundUp/100;
      if (i==1) then
        message:=String(rounded);
      else
        message:=message + "," + String(rounded);
      end if;
    end for;
    message:=message+"\n";
    PGWS1011.Communication.sendMessage(message);
  end sendInputSignalVector;

  function receiveOutputSignalVector
    "receive a sring and transform it to a vector of signals"
    output Real outputSignals[26];
  protected
    String message;
    Integer responseLength;
    Boolean found;
    Integer oldStartPosition;
    Integer newStartPosition;
    Integer index;
  algorithm
    message:=  PGWS1011.Communication.receiveMessage();
    responseLength:=Modelica.Utilities.Strings.length(message);
    found:=true;
    oldStartPosition:=1;
    index:=1;
    if (responseLength > 0) then
      while found loop
        found:=false;
        newStartPosition:=  Modelica.Utilities.Strings.find(
          message,
          ",",
          oldStartPosition);
        if newStartPosition>0 and index<27 then
          outputSignals[index]:=  Modelica.Utilities.Strings.scanReal( Modelica.Utilities.Strings.substring(
            message,
            oldStartPosition,
            newStartPosition-1));
          index:=index + 1;
          found:=true;
          oldStartPosition:=newStartPosition + 1;
        end if;
        if index<=26 then
          outputSignals[index]:=Modelica.Utilities.Strings.scanReal( Modelica.Utilities.Strings.substring(
            message,
            oldStartPosition,
            responseLength));
        end if;
      end while;
    end if;
    if (index < 26) then
      for i in index:26 loop
        outputSignals[i]:=0;
      end for;
    end if;
  end receiveOutputSignalVector;

  end Control;

  package Communication
    function sendMessage "send a message through the socket"
      annotation(Include="#include <Socket.h>", Library={"SocketModelica","wsock32"});
      input String message;
      external "C" sendMessage(message);
    end sendMessage;

    function receiveMessage "receive a message through the socket"
      annotation(Include="#include <Socket.h>", Library={"SocketModelica","wsock32"});
      output String message;
      external "C" message = 
        receiveMessage();
    end receiveMessage;

    function createSocket "Create the TCP socket"
      annotation(Include="#include <Socket.h>", Library={"SocketModelica","wsock32"});
      input String IPAddress;
      input Integer port;
      external "C" createSocket(IPAddress,port);
    end createSocket;

    function startUP "Initialize the Windows Socket Library" 
      annotation(Include="#include <Socket.h>", Library={"SocketModelica","wsock32"});
      external "C" startUP();
    end startUP;

    model Socket
      "Model to create a TCP socket to specified IPAddress and port" 
      parameter String IPAddress="127.0.0.1" "IP-Address of Server";
      parameter Integer port=12345 "Port to connect to";
    algorithm 
      if (initial()) then
        startUP();
        createSocket(IPAddress,port);
      end if;
    end Socket;
  end Communication;

end PGWS1011;

