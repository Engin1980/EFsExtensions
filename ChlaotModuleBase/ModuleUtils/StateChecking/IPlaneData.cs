using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChlaotModuleBase.ModuleUtils.StateChecking
{
  public interface IPlaneData
  {
    bool[] EngineCombustion { get; }
    double Altitude { get; }
    double GroundSpeed { get; }
    double BankAngle { get; }
    int VerticalSpeed { get; }
    bool PushbackTugConnected { get; }
    int Acceleration { get; }
    double IndicatedSpeed { get; }
    double Height { get; }
    bool ParkingBrakeSet { get; }
  }
}
