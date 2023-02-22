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
    public static SimData Empty
    {
      get => new()
      {
        Altitude = 1,
        IsSimPaused = true,
        ParkingBrake = true
      };
    }

    public int Altitude { get; private set; }

    public double BankAngle { get; private set; }

    public int GroundSpeed { get; private set; }

    public int Height { get; private set; }

    public int IndicatedSpeed { get; private set; }

    public bool IsSimPaused { get; private set; }

    public bool ParkingBrake { get; private set; }

    public SimData(SimStruct ss)
    {
      this.Altitude = ss.altitude;
      this.BankAngle = ss.bankAngle;
      this.Height = ss.height;
      this.IndicatedSpeed = ss.indicatedSpeed;
      this.GroundSpeed = ss.groundSpeed;
      this.IsSimPaused = ss.isSimPaused != 0;
      this.ParkingBrake = ss.parkingBrake != 0;
    }

    private SimData() { }
  }

  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
  public struct SimStruct
  {
    [DataDefinition(
      SimVars.Aircraft.Miscelaneous.PLANE_ALTITUDE,
      SimUnits.Length.FOOT,
      SIMCONNECT_DATATYPE.INT32)]
    public int altitude;

    [DataDefinition(
      SimVars.Aircraft.Miscelaneous.PLANE_BANK_DEGREES,
      SimUnits.Angle.DEGREE,
      SIMCONNECT_DATATYPE.INT32)]
    public double bankAngle;

    [DataDefinition(
     SimVars.Aircraft.Miscelaneous.PLANE_ALT_ABOVE_GROUND,
     SimUnits.Length.FOOT,
     SIMCONNECT_DATATYPE.INT32)]
    public int height;

    [DataDefinition(
      SimVars.Aircraft.Miscelaneous.AIRSPEED_INDICATED,
      SimUnits.Speed.KNOT,
      SIMCONNECT_DATATYPE.INT32)]
    public int indicatedSpeed;

    [DataDefinition(
      SimVars.Aircraft.Miscelaneous.GROUND_VELOCITY,
      SimUnits.Speed.KNOT,
      SIMCONNECT_DATATYPE.INT32)]
    public int groundSpeed;

    [DataDefinition(
      SimVars.Miscellaneous.SIM_DISABLED,
      null,
      SIMCONNECT_DATATYPE.INT32)]
    public int isSimPaused;

    [DataDefinition(
      SimVars.Aircraft.BrakesAndLandingGear.BRAKE_PARKING_POSITION,
      null,
      SIMCONNECT_DATATYPE.INT32)]
    public int parkingBrake;
  }
}
