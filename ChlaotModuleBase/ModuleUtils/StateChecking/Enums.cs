using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChlaotModuleBase.ModuleUtils.StateChecking
{
  public enum StateCheckConditionOperator
  {
    And,
    Or
  }

  public enum StateCheckPropertyDirection
  {
    Above,
    Below,
    Exactly,
    Passing
  }

  public enum StateCheckPropertyName
  {
    Altitude,
    Height,
    GS,
    IAS,
    Bank,
    ParkingBrakeSet,
    PushbackTugConnected,
    EngineStarted,
    VerticalSpeed,
    Acceleration
  }
}
