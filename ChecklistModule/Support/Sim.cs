using ChlaotModuleBase;
using Eng.Chlaot.ChlaotModuleBase;
using ESimConnect;
using Microsoft.FlightSimulator.SimConnect;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ChecklistModule.Support
{
  public class Sim
  {
    public delegate void SimSecondElapsedDelegate();
    public event SimSecondElapsedDelegate? SimSecondElapsed;

    private readonly LogHandler logHandler;
    private ESimConnect.ESimConnect simcon;
    public SimData SimData { get; set; } = SimData.Empty;
    public Sim(LogHandler logHandler, bool enableSimConnectLogToFile)
    {
      this.logHandler = logHandler ?? throw new ArgumentNullException(nameof(logHandler));
      if (enableSimConnectLogToFile)
        ESimConnect.ESimConnect.SetLogHandler(s => System.IO.File.AppendAllText("esimcon.log.txt", s));
      else
        ESimConnect.ESimConnect.SetLogHandler(s => { });
    }

    public void Close()
    {
      simcon.Close();
      simcon.Dispose();
      simcon = null;
    }
    public void Open()
    {
      if (simcon != null) throw new ApplicationException("SimCon is expected to be null here.");

      Log(LogLevel.INFO, "Connecting to FS2020");
      try
      {
        Log(LogLevel.VERBOSE, "Creating simconnect instance");
        var tmp = new ESimConnect.ESimConnect();
        Log(LogLevel.VERBOSE, "Connecting simconnect handlers");
        tmp.DataReceived += Simcon_DataReceived;
        tmp.EventInvoked += Simcon_EventInvoked;
        Log(LogLevel.VERBOSE, "Opening simconnect");
        tmp.Open();
        Log(LogLevel.VERBOSE, "Simconnect ready");
        simcon = tmp;
      }
      catch (Exception ex)
      {
        throw new Exception("Failed to open connection to FS2020", ex);
      }
    }

    public void Start()
    {
      Log(LogLevel.VERBOSE, "Simconnect - registering structs");
      simcon.RegisterType<CommonDataStruct>();
      simcon.RegisterType<RareDataStruct>();

      Log(LogLevel.VERBOSE, "Simconnect - requesting structs");
      simcon.RequestDataRepeatedly<CommonDataStruct>(null, SIMCONNECT_PERIOD.SECOND, sendOnlyOnChange: true);
      simcon.RequestDataRepeatedly<RareDataStruct>(null, SIMCONNECT_PERIOD.SECOND, sendOnlyOnChange: true);

      Log(LogLevel.VERBOSE, "Simconnect - attaching to events");
      simcon.RegisterSystemEvent(SimEvents.System.Pause);
      simcon.RegisterSystemEvent(SimEvents.System._1sec);

      Log(LogLevel.VERBOSE, "Simconnect connection ready");
    }

    private void Simcon_DataReceived(ESimConnect.ESimConnect sender, ESimConnect.ESimConnect.ESimConnectDataReceivedEventArgs e)
    {
      Log(LogLevel.INFO, $"FS2020 sim data '{e.RequestId}' of type '{e.Type.Name}' obtained");
      if (e.Type == typeof(CommonDataStruct))
      {
        CommonDataStruct s = (CommonDataStruct)e.Data;
        this.SimData.Update(s);
      }
      else if (e.Type == typeof(RareDataStruct))
      {
        RareDataStruct s = (RareDataStruct)e.Data;
        this.SimData.Update(s);
      }
    }

    private void Simcon_EventInvoked(ESimConnect.ESimConnect sender, ESimConnect.ESimConnect.ESimConnectEventInvokedEventArgs e)
    {
      Log(LogLevel.VERBOSE, "Simcon event");
      if (e.Event == SimEvents.System.Pause)
      {
        bool isPaused = e.Value != 0;
        this.SimData.IsSimPaused = isPaused;
      }
      else if (e.Event == SimEvents.System._1sec)
      {
        this.SimSecondElapsed?.Invoke();
      }
    }

    private void Log(LogLevel level, string message)
    {
      this.logHandler?.Invoke(level, "[SimCon] :: " + message);
    }
  }

  public class SimData : NotifyPropertyChangedBase
  {
    public enum ECameraState
    {
      Cockpit = 2,
      ExternalOrChase = 3,
      Drone = 4,
      FixedOnPlane = 5,
      Environment = 6,
      SixDoF = 7,
      Gameplay = 8,
      Showcase = 9,
      DroneAircraft = 10,
      Waiting = 11,
      WorldMap = 12,
      HangarTC = 13,
      HangarCustom = 14,
      MenuRTC = 15,
      InGameRTC = 16,
      Replay = 17,
      DroneTopDown = 18,
      Hangar = 21,
      Ground = 24,
      FollowTrafficAircraft = 25
    }

    public static SimData Empty
    {
      get
      {
        SimData ret = new SimData()
        {
          IsSimPaused = true,
          EngineCombustion = new()
        };
        for (int i = 0; i < 4; i++)
        {
          ret.EngineCombustion.Add(false);
        }
        return ret;
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

    public ObservableCollection<bool> EngineCombustion
    {
      get => base.GetProperty<ObservableCollection<bool>>(nameof(EngineCombustion))!;
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

    private SimData() { }

    public void Update(CommonDataStruct ss)
    {
      this.Callsign = ss.callsign;
      this.Altitude = ss.altitude;
      this.BankAngle = ss.bankAngle;
      this.Height = ss.height;
      this.IndicatedSpeed = ss.indicatedSpeed;
      this.GroundSpeed = ss.groundSpeed;
      this.VerticalSpeed = ss.verticalSpeed;
      this.Acceleration = ss.accelerationBodyZ;
    }

    public void Update(RareDataStruct s)
    {
      this.ParkingBrakeSet = s.parkingBrake != 0;
      this.EngineCombustion[0] = s.engineOneCombustion != 0;
      this.EngineCombustion[1] = s.engineTwoCombustion != 0;
      this.EngineCombustion[2] = s.engineThreeCombustion != 0;
      this.EngineCombustion[3] = s.engineFourCombustion != 0;
      this.PushbackTugConnected = s.pushbackTugConnected != 0;
    }
  }

  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
  public struct CommonDataStruct
  {
    [DataDefinition(SimVars.Aircraft.RadioAndNavigation.ATC_ID)]
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string callsign;

    [DataDefinition(SimVars.Aircraft.Miscelaneous.PLANE_ALTITUDE, SimUnits.Length.FOOT)]
    public int altitude;

    [DataDefinition(SimVars.Aircraft.Miscelaneous.PLANE_BANK_DEGREES, SimUnits.Angle.DEGREE)]
    public double bankAngle;

    [DataDefinition(SimVars.Aircraft.Miscelaneous.PLANE_ALT_ABOVE_GROUND, SimUnits.Length.FOOT)]
    public int height;

    [DataDefinition(SimVars.Aircraft.Miscelaneous.AIRSPEED_INDICATED, SimUnits.Speed.KNOT)]
    public int indicatedSpeed;

    [DataDefinition(SimVars.Aircraft.Miscelaneous.GROUND_VELOCITY, SimUnits.Speed.KNOT)]
    public int groundSpeed;

    [DataDefinition(SimVars.Aircraft.Miscelaneous.VERTICAL_SPEED, SimUnits.Length.FOOT)]
    public int verticalSpeed;

    [DataDefinition(SimVars.Aircraft.Miscelaneous.ACCELERATION_BODY_Z, SimUnits.Acceleration.FEET_PER_SECOND_SQUARED)]
    public double accelerationBodyZ;
  }

  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
  public struct RareDataStruct
  {
    [DataDefinition(SimVars.Aircraft.Engine.ENG_COMBUSTION__index + "1")]
    public int engineOneCombustion;
    [DataDefinition(SimVars.Aircraft.Engine.ENG_COMBUSTION__index + "2")]
    public int engineTwoCombustion;
    [DataDefinition(SimVars.Aircraft.Engine.ENG_COMBUSTION__index + "3")]
    public int engineThreeCombustion;
    [DataDefinition(SimVars.Aircraft.Engine.ENG_COMBUSTION__index + "4")]
    public int engineFourCombustion;
    [DataDefinition(SimVars.Aircraft.BrakesAndLandingGear.BRAKE_PARKING_POSITION)]
    public int parkingBrake;
    [DataDefinition(SimVars.Services.PUSHBACK_ATTACHED)]
    public int pushbackTugConnected;
  }
}
