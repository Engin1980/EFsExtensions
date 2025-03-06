using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.SimConWrapping.PrdefinedTypes;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.StateChecking;
using ESimConnect.Extenders;
using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESimConnect.Definitions;

namespace Eng.Chloat.Modules.FailuresModule.Model
{
  public class FailSimData : NotifyPropertyChanged
  {
    public FailSimData()
    {
      IsSimPaused = true;
      EngineCombustion = new bool[4];
      for (int i = 0; i < 4; i++)
      {
        EngineCombustion[i] = false;
      }
    }

    [SimProperty(SimVars.Aircraft.Miscelaneous.PLANE_ALTITUDE, SimUnits.Length.FOOT)]
    [StateCheckName("alt")]
    public int Altitude
    {
      get => GetProperty<int>(nameof(Altitude))!;
      set => UpdateProperty(nameof(Altitude), value);
    }

    [SimProperty(SimVars.Aircraft.Miscelaneous.PLANE_BANK_DEGREES, SimUnits.Angle.DEGREE)]
    public double BankAngle
    {
      get => GetProperty<double>(nameof(BankAngle))!;
      set => UpdateProperty(nameof(BankAngle), value);
    }

    [SimProperty(SimVars.Aircraft.RadioAndNavigation.ATC_ID)]
    public string Callsign
    {
      get => GetProperty<string>(nameof(Callsign))!;
      set => UpdateProperty(nameof(Callsign), value);
    }

    [StateCheckName("eng")]
    public bool[] EngineCombustion
    {
      get => GetProperty<bool[]>(nameof(EngineCombustion))!;
      set => UpdateProperty(nameof(EngineCombustion), value);
    }

    [SimProperty(SimVars.Aircraft.Engine.ENG_COMBUSTION__index + "1")]
    public int Eng1Combustion { get => EngineCombustion[0] ? 1 : 0; set => EngineCombustion[0] = value != 0; }

    [SimProperty(SimVars.Aircraft.Engine.ENG_COMBUSTION__index + "2")]
    public int Eng2Combustion { get => EngineCombustion[1] ? 1 : 0; set => EngineCombustion[1] = value != 0; }

    [SimProperty(SimVars.Aircraft.Engine.ENG_COMBUSTION__index + "3")]
    public int Eng3Combustion { get => EngineCombustion[2] ? 1 : 0; set => EngineCombustion[2] = value != 0; }

    [SimProperty(SimVars.Aircraft.Engine.ENG_COMBUSTION__index + "4")]
    public int Eng4Combustion { get => EngineCombustion[3] ? 1 : 0; set => EngineCombustion[3] = value != 0; }

    [SimProperty(SimVars.Aircraft.Miscelaneous.GROUND_VELOCITY, SimUnits.Speed.KNOT)]
    [StateCheckName("gs")]
    public int GroundSpeed
    {
      get => GetProperty<int>(nameof(GroundSpeed))!;
      set => UpdateProperty(nameof(GroundSpeed), value);
    }

    [SimProperty(SimVars.Aircraft.Miscelaneous.PLANE_ALT_ABOVE_GROUND, SimUnits.Length.FOOT)]
    [StateCheckName("height")]
    public int Height
    {
      get => GetProperty<int>(nameof(Height))!;
      set => UpdateProperty(nameof(Height), value);
    }

    [SimProperty(SimVars.Aircraft.Miscelaneous.AIRSPEED_INDICATED, SimUnits.Speed.KNOT)]
    [StateCheckName("ias")]
    public int IndicatedSpeed
    {
      get => GetProperty<int>(nameof(IndicatedSpeed))!;
      set => UpdateProperty(nameof(IndicatedSpeed), value);
    }

    public bool IsSimPaused
    {
      get => GetProperty<bool>(nameof(IsSimPaused))!;
      set => UpdateProperty(nameof(IsSimPaused), value);
    }

    [SimProperty(SimVars.Aircraft.BrakesAndLandingGear.BRAKE_PARKING_POSITION)]
    [StateCheckName("parkingBrake")]
    public bool ParkingBrakeSet
    {
      get => GetProperty<bool>(nameof(ParkingBrakeSet))!;
      set => UpdateProperty(nameof(ParkingBrakeSet), value);
    }

    [SimProperty(SimVars.Aircraft.Miscelaneous.VERTICAL_SPEED, SimUnits.Speed.FEETBYMINUTE)]
    [StateCheckName("vs")]
    public int VerticalSpeed
    {
      get => GetProperty<int>(nameof(VerticalSpeed))!;
      set => UpdateProperty(nameof(VerticalSpeed), value);
    }

    [SimProperty(SimVars.Services.PUSHBACK_ATTACHED)]
    public bool PushbackTugConnected
    {
      get => GetProperty<bool>(nameof(PushbackTugConnected))!;
      set => UpdateProperty(nameof(PushbackTugConnected), value);
    }

    [SimProperty(SimVars.Aircraft.Miscelaneous.ACCELERATION_BODY_Z, SimUnits.Acceleration.FEET_PER_SECOND_SQUARED)]
    [StateCheckName("acc")]
    public double Acceleration
    {
      get => GetProperty<double>(nameof(Acceleration))!;
      set => UpdateProperty(nameof(Acceleration), value);
    }
  }
}
