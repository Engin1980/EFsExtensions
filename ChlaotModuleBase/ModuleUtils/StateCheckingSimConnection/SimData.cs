using ChlaotModuleBase.ModuleUtils.StateChecking;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChlaotModuleBase.ModuleUtils.StateCheckingSimConnection
{
  public class SimData : NotifyPropertyChangedBase, IPlaneData
  {
    private const int SECONDS_PER_MINUTE = 60;

    public SimData()
    {
      this.IsSimPaused = true;
      this.EngineCombustion = new bool[4];
      for (int i = 0; i < 4; i++)
      {
        this.EngineCombustion[i] = false;
      }
    }

    public int Altitude
    {
      get => base.GetProperty<int>(nameof(Altitude))!;
      set => base.UpdateProperty(nameof(Altitude), value);
    }

    public double BankAngle
    {
      get => base.GetProperty<double>(nameof(BankAngle))!;
      set => base.UpdateProperty(nameof(BankAngle), value);
    }

    public string Callsign
    {
      get => base.GetProperty<string>(nameof(Callsign))!;
      set => base.UpdateProperty(nameof(Callsign), value);
    }

    public bool[] EngineCombustion
    {
      get => base.GetProperty<bool[]>(nameof(EngineCombustion))!;
      set => base.UpdateProperty(nameof(EngineCombustion), value);
    }

    public int GroundSpeed
    {
      get => base.GetProperty<int>(nameof(GroundSpeed))!;
      set => base.UpdateProperty(nameof(GroundSpeed), value);
    }

    public int Height
    {
      get => base.GetProperty<int>(nameof(Height))!;
      set => base.UpdateProperty(nameof(Height), value);
    }

    public int IndicatedSpeed
    {
      get => base.GetProperty<int>(nameof(IndicatedSpeed))!;
      set => base.UpdateProperty(nameof(IndicatedSpeed), value);
    }

    public bool IsSimPaused
    {
      get => base.GetProperty<bool>(nameof(IsSimPaused))!;
      set => base.UpdateProperty(nameof(IsSimPaused), value);
    }
    public bool ParkingBrakeSet
    {
      get => base.GetProperty<bool>(nameof(ParkingBrakeSet))!;
      set => base.UpdateProperty(nameof(ParkingBrakeSet), value);
    }
    public int VerticalSpeed
    {
      get => base.GetProperty<int>(nameof(VerticalSpeed))!;
      set => base.UpdateProperty(nameof(VerticalSpeed), value);
    }

    public bool PushbackTugConnected
    {
      get => base.GetProperty<bool>(nameof(PushbackTugConnected))!;
      set => base.UpdateProperty(nameof(PushbackTugConnected), value);
    }

    public double Acceleration
    {
      get => base.GetProperty<double>(nameof(Acceleration))!;
      set => base.UpdateProperty(nameof(Acceleration), value);
    }

    public bool IsTugConnected
    {
      get => base.GetProperty<bool>(nameof(IsTugConnected))!;
      set => base.UpdateProperty(nameof(IsTugConnected), value);
    }

    internal void Update(CommonDataStruct ss)
    {
      this.Callsign = ss.callsign;
      this.Altitude = ss.altitude;
      this.BankAngle = ss.bankAngle;
      this.Height = ss.height;
      this.IndicatedSpeed = ss.indicatedSpeed;
      this.GroundSpeed = ss.groundSpeed;
      this.VerticalSpeed = ss.verticalSpeed * SECONDS_PER_MINUTE;
      this.Acceleration = (int) ss.accelerationBodyZ;
    }

    internal void Update(RareDataStruct s)
    {
      this.ParkingBrakeSet = s.parkingBrake != 0;
      this.EngineCombustion[0] = s.engineOneCombustion != 0;
      this.EngineCombustion[1] = s.engineTwoCombustion != 0;
      this.EngineCombustion[2] = s.engineThreeCombustion != 0;
      this.EngineCombustion[3] = s.engineFourCombustion != 0;
      this.PushbackTugConnected = s.pushbackTugConnected != 0;
    }
  }
}
