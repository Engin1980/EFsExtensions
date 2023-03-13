using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking
{
  public interface IPlaneData
  {
    bool[] EngineCombustion { get; }
    int Altitude { get; }
    int GroundSpeed { get; }
    double BankAngle { get; }
    int VerticalSpeed { get; }
    bool PushbackTugConnected { get; }
    double Acceleration { get; }
    int IndicatedSpeed { get; }
    int Height { get; }
    bool ParkingBrakeSet { get; }
  }
}
