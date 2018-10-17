using System;
using System.IO;

namespace MULTIFORM_PCS.Gateway.ConnectionModule.iRobot {

  // the robots sensor data
  public class iRobotSensorData {
    public readonly byte chargingState;
    public readonly ushort voltage;
    public readonly short current;
    public readonly sbyte batteryTemperature;
    public readonly ushort batteryCharge;
    public readonly ushort batteryCapacity;
    public readonly byte chargingSourceAvailable;

    public iRobotSensorData(BinaryReader br) {
      chargingState = br.ReadByte();
      voltage = (ushort)System.Net.IPAddress.NetworkToHostOrder((short)br.ReadUInt16());
      current = (short)System.Net.IPAddress.NetworkToHostOrder((short)br.ReadUInt16());
      batteryTemperature = br.ReadSByte();
      batteryCharge = (ushort)System.Net.IPAddress.NetworkToHostOrder((short)br.ReadUInt16());
      batteryCapacity = (ushort)System.Net.IPAddress.NetworkToHostOrder((short)br.ReadUInt16());
      chargingSourceAvailable = br.ReadByte();
    }

    public String chargingStateToString() {
      switch (chargingState) {
        case 0: return "Not charging";
        case 1: return "Reconditioning Charging";
        case 2: return "Full Charging";
        case 3: return "Trickle Charging";
        case 4: return "Waiting";
        case 5: return "Charging Fault Condition";
        default: return "Invalid";
      }
    }
    public String voltageToString() {
      return voltage + "mV";
    }
    public String currentToString() {
      return current + "mA";
    }
    public String batteryTemperatureToString() {
      return batteryTemperature + "°C";
    }
    public String batteryChargeToString() {
      return batteryCharge + "mAh";
    }
    public String batteryCapacityToString() {
      return batteryCapacity + "mAh";
    }
    public String chargingSourceAvailableToString() {
      switch (chargingSourceAvailable) {
        case 0: return "None";
        case 1: return "Internal Charger";
        case 2: return "Home Base";
        case 3: return "Internal Charger, Home Base";
        default: return "Invalid";
      }
    }

    public override string ToString() {
      return "{ chargingState=" + "\"" + chargingStateToString() + "\"" +
             "; voltage=" + voltageToString() +
             "; current=" + currentToString() +
             "; batteryTemperature=" + batteryTemperatureToString() +
             "; batteryCharge=" + batteryChargeToString() +
             "; batteryCapacity=" + batteryCapacityToString() +
             "; chargingSourceAvailable=" + "\"" + chargingSourceAvailableToString() + "\"" +
             " }";
    }
  }

}
