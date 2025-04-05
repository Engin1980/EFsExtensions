using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.SimObjects;
using Eng.EFsExtensions.Modules.FlightLogModule.Models;
using ESimConnect;
using ESimConnect.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static Eng.EFsExtensions.Modules.FlightLogModule.Models.RunViewModel;

namespace Eng.EFsExtensions.Modules.FlightLogModule
{
  public partial class RunContext
  {
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TakeOffStruct
    {
      [DataDefinition(SimVars.Aircraft.BrakesAndLandingGear.GEAR_IS_ON_GROUND__index + "0")]
      public double gear0;

      [DataDefinition(SimVars.Aircraft.BrakesAndLandingGear.GEAR_IS_ON_GROUND__index + "1")]
      public double gear1;

      [DataDefinition(SimVars.Aircraft.BrakesAndLandingGear.GEAR_IS_ON_GROUND__index + "2")]
      public double gear2;

      [DataDefinition(SimVars.Aircraft.Miscelaneous.PLANE_ALT_ABOVE_GROUND, SimUnits.Length.FOOT)]
      public double height;

      [DataDefinition(SimVars.Aircraft.Miscelaneous.PLANE_PITCH_DEGREES, SimUnits.Angle.DEGREE)]
      public double pitch;

      [DataDefinition(SimVars.Aircraft.Miscelaneous.PLANE_BANK_DEGREES, SimUnits.Angle.DEGREE)]
      public double bank;

      [DataDefinition(SimVars.Aircraft.Miscelaneous.VERTICAL_SPEED, SimUnits.Speed.FEETBYMINUTE)]
      public double vs;

      [DataDefinition(SimVars.Aircraft.Miscelaneous.AIRSPEED_INDICATED, SimUnits.Speed.KNOT)]
      public double ias;

      [DataDefinition(SimVars.Aircraft.Miscelaneous.SURFACE_RELATIVE_GROUND_SPEED, SimUnits.Speed.KNOT)]
      public double gs;

      [DataDefinition(SimVars.Aircraft.Miscelaneous.ACCELERATION_BODY_Y)]
      public double accelerationY;

      [DataDefinition(SimVars.Aircraft.Miscelaneous.PLANE_LATITUDE, SimUnits.Angle.DEGREE)]
      public double latitude;

      [DataDefinition(SimVars.Aircraft.Miscelaneous.PLANE_LONGITUDE, SimUnits.Angle.DEGREE)]
      public double longitude;

      [DataDefinition(SimVars.Aircraft.Miscelaneous.PLANE_HEADING_DEGREES_TRUE, SimUnits.Angle.DEGREE)]
      public double heading;
    }

    public class TakeOffDetector
    {
      private class TakeOffRunData
      {
        public readonly List<TakeOffRunList> takeOffRunList = new();
        public DateTime? noseGearInAirDateTime = null;
        public DateTime? allGearInAirDateTime = null;
        public double? allGearInAirLatitude = null;
        public double? allGearInAirLongitude = null;
        public double? allGearInAirIas = null;
        public double? allGearInAirGs = null;
        public double maxBank = 0;
        public double maxPitch = 0;
        public DateTime? maxPitchDateTime = null;
        public double maxVs = 0;
        public double maxAccY = 0;
        public int deccelerationCounter = 0;
        public double deccelerationCounterIas = 0;
      }
      private record TakeOffRunList(int Heading, DateTime dateTime, double latitude, double longitude);
      private readonly TakeOffRunData runData = new();
      private readonly NewSimObject simObj;
      private readonly RunViewModel runVM;
      private RequestId? requestId;
      private bool isDisposed = false;
      public event Action<TakeOffAttemptData>? AttemptRecorded;
      private bool isActive = true;

      public TakeOffDetector(NewSimObject simObj, RunViewModel runVM)
      {
        this.simObj = simObj;
        this.runVM = runVM;
      }

      internal void InitAndStart()
      {
        var simCon = this.simObj.ESimCon;
        var typeId = simCon.Structs.Register<TakeOffStruct>();
        requestId = simCon.Structs.RequestRepeatedly<TakeOffStruct>(SimConnectPeriod.SIM_FRAME, true);
        simCon.DataReceived += SimCon_DataReceived;
        this.isActive = true;
      }
      private void SimCon_DataReceived(ESimConnect.ESimConnect sender, ESimConnect.ESimConnect.ESimConnectDataReceivedEventArgs e)
      {
        if (!isActive) return;
        if (e.RequestId != requestId) return;

        TakeOffStruct data = (TakeOffStruct)e.Data;
        UpdateByData(data);
      }

      private void UpdateByData(TakeOffStruct data)
      {
        bool isFlying = data.gear0 + data.gear1 + data.gear2 == 0;

        if (!isFlying)
        {
          if (runData.deccelerationCounterIas > data.ias)
            runData.deccelerationCounter++;
          else
            runData.deccelerationCounter = 0;

          if (data.ias < 5 && runData.takeOffRunList.Count > 0)
          {
            runData.takeOffRunList.Clear();
          }
          else if (runData.deccelerationCounter > 1 * 50 && runData.takeOffRunList.Count > 0) // 50 records is 1 second
          {
            runData.takeOffRunList.Clear();
            runData.deccelerationCounter = 0;
          }
          runData.deccelerationCounterIas = data.ias;

          if (data.gear0 == 0)
            runData.noseGearInAirDateTime = DateTime.UtcNow;
          else
            runData.noseGearInAirDateTime = null;
        }

        if (data.ias > 0)
        {
          if (runData.takeOffRunList.Count == 0)
            runData.takeOffRunList.Add(new((int)data.heading, DateTime.UtcNow, data.latitude, data.longitude));
          else if (runData.takeOffRunList.Last().Heading != (int)data.heading)
            runData.takeOffRunList.Add(new((int)data.heading, DateTime.UtcNow, data.latitude, data.longitude));
        }

        if (isFlying)
        {
          if (runData.allGearInAirDateTime == null)
          {
            // for the first time
            if (runData.noseGearInAirDateTime == null)
              runData.noseGearInAirDateTime = DateTime.UtcNow;
            runData.allGearInAirDateTime = DateTime.UtcNow;
            runData.allGearInAirLatitude = data.latitude;
            runData.allGearInAirLongitude = data.longitude;
            runData.allGearInAirIas = data.ias;
            runData.allGearInAirGs = data.gs;
            runData.maxBank = double.MinValue;
            runData.maxPitch = double.MaxValue;
            runData.maxVs = double.MinValue;
          }

          runData.maxAccY = Math.Max(runData.maxAccY, data.accelerationY);
          runData.maxVs = Math.Max(runData.maxVs, data.vs);
          runData.maxBank = Math.Max(runData.maxBank, Math.Abs(data.bank));
          if (runData.maxPitch > data.pitch)
          {
            runData.maxPitch = data.pitch;
            runData.maxPitchDateTime = DateTime.UtcNow;
          }

          if (data.height > 100)
          {
            CloseCurrentAttempt();
          }
        }
      }

      private void CloseCurrentAttempt()
      {
        this.isActive = false;

        if (runData.allGearInAirDateTime == null)
          return;

        double maxBank; double maxPitch; double ias; double gs; double maxVS;
        TimeSpan rollToFrontGearTime; TimeSpan rollToAllGearTime;
        double maxAccY; DateTime rollStartDateTime; DateTime airborneDateTime;
        double rollStartLatitude; double rollStartLongitude;
        double takeOffLatitude; double takeOffLongitude;

        TakeOffRunList takeOffRunStart = ExtractTakeOffRunStartData();

        maxBank = runData.maxBank;
        maxPitch = runData.maxPitch;
        ias = runData.allGearInAirIas!.Value;
        gs = runData.allGearInAirGs!.Value;
        maxVS = runData.maxVs;
        rollToFrontGearTime = runData.noseGearInAirDateTime!.Value - takeOffRunStart.dateTime;
        rollToAllGearTime = runData.allGearInAirDateTime!.Value - takeOffRunStart.dateTime;
        maxAccY = runData.maxAccY;
        rollStartDateTime = takeOffRunStart.dateTime;
        rollStartLatitude = takeOffRunStart.latitude;
        rollStartLongitude = takeOffRunStart.longitude;
        takeOffLatitude = runData.allGearInAirLatitude!.Value;
        takeOffLongitude = runData.allGearInAirLongitude!.Value;
        airborneDateTime = runData.allGearInAirDateTime!.Value;


        TakeOffAttemptData toad = new(
          maxBank, maxPitch, ias, gs, maxVS, rollToFrontGearTime, rollToAllGearTime, maxAccY,
          rollStartDateTime, airborneDateTime,
          rollStartLatitude, rollStartLongitude, takeOffLatitude, takeOffLongitude);

        this.AttemptRecorded?.Invoke(toad);
      }

      private TakeOffRunList ExtractTakeOffRunStartData()
      {
        int currentHeading = runData.takeOffRunList.Last().Heading;
        TakeOffRunList ret = runData.takeOffRunList.Last();

        static int headingDifference(int heading1, int heading2)
        {
          int diff = Math.Abs(heading1 - heading2);
          return Math.Min(diff, 360 - diff);
        }

        for (int i = runData.takeOffRunList.Count - 1; i >= 0; i--)
        {
          int diff = headingDifference(runData.takeOffRunList[i].Heading, currentHeading);
          if (diff < 8)
            ret = runData.takeOffRunList[i];
        }

        return ret;
      }

      internal void Stop()
      {
        var simCon = this.simObj.ESimCon;
        this.simObj.ESimCon.DataReceived -= SimCon_DataReceived;
        simCon.Structs.Unregister<TakeOffStruct>();
        this.isDisposed = true;
      }

      public void Dispose()
      {
        if (!isDisposed) Stop();
      }
    }
  }
}
