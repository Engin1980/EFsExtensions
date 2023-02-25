using ChlaotModuleBase;
using ESimConnect;
using Microsoft.FlightSimulator.SimConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ChecklistModule.Support
{
  public class Sim
  {
    public delegate void SimDataUpdatedDelegate();
    public event SimDataUpdatedDelegate? SimDataUpdated;

    private readonly LogHandler logHandler;
    private ESimConnect.ESimConnect simcon;
    public SimData SimData { get; set; } = SimData.Empty;
    public Sim(LogHandler logHandler)
    {
      this.logHandler = logHandler;
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

      logHandler?.Invoke(Eng.Chlaot.ChlaotModuleBase.LogLevel.INFO, "Connecting to FS2020");
      try
      {
        var tmp = new ESimConnect.ESimConnect();
        tmp.DataReceived += Simcon_DataReceived;
        tmp.Open();
        tmp.RegisterType<SimStruct>(12345);
        simcon = tmp;
      }
      catch (Exception ex)
      {
        throw new Exception("Failed to open connection to FS2020", ex);
      }
    }

    public void Update()
    {
      logHandler?.Invoke(Eng.Chlaot.ChlaotModuleBase.LogLevel.INFO, "FS2020 sim data requested");
      simcon.RequestData<SimStruct>();
    }

    private void Simcon_DataReceived(ESimConnect.ESimConnect sender, ESimConnect.ESimConnect.ESimConnectDataReceivedEventArgs e)
    {
      logHandler?.Invoke(Eng.Chlaot.ChlaotModuleBase.LogLevel.INFO, "FS2020 sim data obtained");
      SimStruct ss = (SimStruct)e.Data;
      this.SimData = new SimData(ss);
      this.SimDataUpdated?.Invoke();
    }
  }

  public class SimData
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
      get => new()
      {
        Altitude = 1,
        IsSimPaused = true,
        ParkingBrake = true,
        CameraState = ECameraState.Waiting
      };
    }

    public int Altitude { get; private set; }
    public double BankAngle { get; private set; }
    public int GroundSpeed { get; private set; }
    public int Height { get; private set; }
    public int IndicatedSpeed { get; private set; }
    public bool IsSimPaused { get; private set; }
    public bool ParkingBrake { get; private set; }
    public int VerticalSpeed { get; set; }
    public bool[] EngineCombustion { get; set; } = new bool[4];

    public ECameraState CameraState { get; private set; }

    public string Callsign { get; set; }

    public SimData(SimStruct ss)
    {
      this.Callsign = ss.callsign;
      this.Altitude = ss.altitude;
      this.BankAngle = ss.bankAngle;
      this.Height = ss.height;
      this.IndicatedSpeed = ss.indicatedSpeed;
      this.GroundSpeed = ss.groundSpeed;
      this.IsSimPaused = ss.isSimPaused != 0;
      this.ParkingBrake = ss.parkingBrake != 0;
      this.CameraState = (ECameraState)ss.cameraState;
      this.VerticalSpeed = ss.verticalSpeed;
      this.EngineCombustion[0] = ss.engineOneCombustion != 0;
      this.EngineCombustion[1] = ss.engineTwoCombustion != 0;
      this.EngineCombustion[2] = ss.engineThreeCombustion != 0;
      this.EngineCombustion[3] = ss.engineFourCombustion != 0;
    }

    public bool IsProbablyOutOfTheSim { get => (int)this.CameraState <= 6; }

    private SimData() { }
  }

  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
  public struct SimStruct
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

    [DataDefinition("CAMERA STATE")]
    public int cameraState;

    [DataDefinition(SimVars.Aircraft.Miscelaneous.VERTICAL_SPEED, SimUnits.Length.FOOT)]
    public int verticalSpeed;

    [DataDefinition(SimVars.Aircraft.Engine.ENG_COMBUSTION__index + "1")]
    public int engineOneCombustion;
    [DataDefinition(SimVars.Aircraft.Engine.ENG_COMBUSTION__index + "2")]
    public int engineTwoCombustion;
    [DataDefinition(SimVars.Aircraft.Engine.ENG_COMBUSTION__index + "3")]
    public int engineThreeCombustion;
    [DataDefinition(SimVars.Aircraft.Engine.ENG_COMBUSTION__index + "4")]
    public int engineFourCombustion;

    [DataDefinition(SimVars.Miscellaneous.SIM_DISABLED)]
    public int isSimPaused;

    [DataDefinition(SimVars.Aircraft.BrakesAndLandingGear.BRAKE_PARKING_POSITION)]
    public int parkingBrake;
  }
}
