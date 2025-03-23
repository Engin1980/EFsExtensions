using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.SimObjects;
using ESimConnect;
using ESimConnect.Definitions;
using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Eng.EFsExtensions.Modules.FlightLogModule.RunViewModel;

namespace Eng.EFsExtensions.Modules.FlightLogModule
{
  public partial class RunContext
  {
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct LandingStruct
    {
      [DataDefinition(SimVars.Aircraft.BrakesAndLandingGear.GEAR_IS_ON_GROUND__index + "0")]
      public double gear0;

      [DataDefinition(SimVars.Aircraft.BrakesAndLandingGear.GEAR_IS_ON_GROUND__index + "1")]
      public double gear1;

      [DataDefinition(SimVars.Aircraft.BrakesAndLandingGear.GEAR_IS_ON_GROUND__index + "2")]
      public double gear2;

      [DataDefinition(SimVars.Aircraft.Miscelaneous.PLANE_ALT_ABOVE_GROUND, SimUnits.Length.FOOT)]
      public double height;

      [DataDefinition(SimVars.Aircraft.Miscelaneous.PLANE_ALTITUDE, SimUnits.Length.FOOT)]
      public double altitude;

      [DataDefinition(SimVars.Aircraft.Miscelaneous.PLANE_PITCH_DEGREES, SimUnits.Angle.DEGREE)]
      public double pitch;

      [DataDefinition(SimVars.Aircraft.Miscelaneous.PLANE_BANK_DEGREES, SimUnits.Angle.DEGREE)]
      public double bank;

      [DataDefinition(SimVars.Aircraft.Miscelaneous.VERTICAL_SPEED, SimUnits.Speed.FEETBYMINUTE)]
      public double vs;

      [DataDefinition(SimVars.Aircraft.Miscelaneous.AIRSPEED_INDICATED, SimUnits.Speed.KNOT)]
      public double ias;

      [DataDefinition(SimVars.Aircraft.Miscelaneous.ACCELERATION_BODY_Y)]
      public double accelerationY;
    }

    private class LandingDetector : IDisposable
    {
      private class RecordingData
      {
        public int gear0Count;
        public int gear1Count;
        public int gear2Count;
        public int notGroundCount;
        public double bank;
        public double pitch;
        public double ias;
        public double vs;
        public double maxAccY;
        public DateTime? dateTime;
      }

      private readonly NewSimObject simObj;
      private readonly RunViewModel runVM;
      private RequestId? requestId;
      private bool isDisposed = false;
      private readonly RecordingData current = new();
      private readonly List<LandingAttemptData> recordedData = new();

      public event Action<LandingAttemptData>? AttemptRecorded;

      public LandingDetector(NewSimObject simObj, RunViewModel runVM)
      {
        this.simObj = simObj;
        this.runVM = runVM;
      }

      internal void InitAndStart()
      {
        var simCon = this.simObj.ESimCon;
        var typeId = simCon.Structs.Register<LandingStruct>();
        requestId = simCon.Structs.RequestRepeatedly<LandingStruct>(SimConnectPeriod.SIM_FRAME, true);
        simCon.DataReceived += SimCon_DataReceived;
      }

      private void SimCon_DataReceived(ESimConnect.ESimConnect sender, ESimConnect.ESimConnect.ESimConnectDataReceivedEventArgs e)
      {
        if (e.RequestId != requestId) return;

        LandingStruct data = (LandingStruct)e.Data;
        UpdateByData(data);
      }

      private void UpdateByData(LandingStruct data)
      {
        if (data.gear0 + data.gear1 + data.gear2 == 0)
        {
          // is flying
          current.notGroundCount++;

          // is flying long (over 50*20ms) and was on ground
          if (current.notGroundCount > 50 && current.gear0Count + current.gear1Count + current.gear2Count > 0)
          {
            CloseCurrentAttempt();
          }
        }
        else
        {
          // is on ground
          current.gear0Count += (int)data.gear0;
          current.gear1Count += (int)data.gear1;
          current.gear2Count += (int)data.gear2;

          // adjust acc-Y only if within 1 sec after touchdown
          if (Math.Min(current.gear1Count, current.gear2Count) < 50)
            current.maxAccY = Math.Max(current.maxAccY, data.accelerationY);

          if (current.notGroundCount > 50) // was not on ground in prevous 1 second
          {
            // first time on ground
            current.bank = data.bank;
            current.pitch = data.pitch;
            current.ias = data.ias;
            current.vs = data.vs;
            current.dateTime = DateTime.Now;

            current.notGroundCount = 0;
          }
        }
      }

      private void CloseCurrentAttempt()
      {
        // no active landing in progress
        if (this.current.dateTime == null)
          return;

        double mainGearTime = Math.Abs(this.current.gear1Count - this.current.gear2Count) * (1 / 50d);
        double allGearTime = (Math.Max(this.current.gear1Count,
            Math.Max(this.current.gear2Count, this.current.gear0Count)) -
          Math.Min(this.current.gear1Count,
            Math.Min(this.current.gear2Count, this.current.gear0Count))) * (1 / 50d);

        LandingAttemptData item = new(
          this.current.bank,
          this.current.pitch,
          this.current.ias,
          this.current.vs,
          mainGearTime,
          allGearTime,
          this.current.maxAccY,
          this.current.dateTime.Value);

        recordedData.Add(item);

        // reset current:
        current.bank = 0;
        current.pitch = 0;
        current.ias = 0;
        current.vs = 0;
        current.gear1Count = 0;
        current.gear2Count = 0;
        current.gear0Count = 0;
        current.maxAccY = 0;
        current.notGroundCount = 51; // to behave like not on ground for more than second
        current.dateTime = null;

        this.AttemptRecorded?.Invoke(item);
      }


      internal void Stop()
      {
        var simCon = this.simObj.ESimCon;
        simCon.Structs.Unregister<LandingStruct>();
        this.simObj.ESimCon.DataReceived -= SimCon_DataReceived;
        this.isDisposed = true;

        CloseCurrentAttempt();
      }

      public void Dispose()
      {
        if (!isDisposed) Stop();
      }
    }
  }
}
