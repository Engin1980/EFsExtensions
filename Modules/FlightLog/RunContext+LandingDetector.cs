using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.SimObjects;
using ESimConnect;
using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.FlightLogModule
{
  public partial class RunContext
  {
    public struct LandingData
    {
      public double gear0;
      public double gear1;
      public double gear2;
      public double height;
      public double altitude;
      public double pitch;
      public double bank;
      public double vs;
      public double ias;
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
      }

      public class RecordedData
      {
        public double Bank { get; internal set; }
        public double Pitch { get; internal set; }
        public double IAS { get; internal set; }
        public double VS { get; internal set; }
        public int MainGearTime { get; internal set; }
        public int AllGearTime { get; internal set; }
        public double MaxAccY { get; internal set; }
      }

      private readonly NewSimObject simObj;
      private readonly RunViewModel runVM;
      private RequestId? requestId;
      private bool isDisposed = false;
      private readonly RecordingData current = new();
      private readonly List<RecordedData> recordedData = new();

      public event Action? CloseRequested;
      public event Action<RecordedData>? AttemptRecorded;

      public LandingDetector(NewSimObject simObj, RunViewModel runVM)
      {
        this.simObj = simObj;
        this.runVM = runVM;
      }

      internal void InitAndStart()
      {
        var simCon = this.simObj.ESimCon;
        var typeId = simCon.Structs.Register<LandingData>();
        requestId = simCon.Structs.RequestRepeatedly<LandingData>(SimConnectPeriod.SIM_FRAME, true);
        simCon.DataReceived += SimCon_DataReceived;
      }

      private void SimCon_DataReceived(ESimConnect.ESimConnect sender, ESimConnect.ESimConnect.ESimConnectDataReceivedEventArgs e)
      {
        if (e.RequestId != requestId) return;

        LandingData data = (LandingData)e.Data;
        UpdateByData(data);
      }

      private void UpdateByData(LandingData data)
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

          // too high for landing
          if (data.height > 100)
          {
            CloseLandingDetector();
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

            current.notGroundCount = 0;
          }
          if (current.ias < 30)
          {
            // end of landing
            CloseCurrentAttempt();
            CloseLandingDetector();
          }
        }
      }

      private void CloseCurrentAttempt()
      {
        RecordedData item = new()
        {
          Bank = current.bank,
          Pitch = current.pitch,
          IAS = current.ias,
          VS = current.vs,
          MainGearTime = Math.Abs(current.gear1Count - current.gear2Count) * (1 / 50),
          AllGearTime = Math.Abs(Math.Min(current.gear1Count, current.gear2Count) - current.gear0Count) * (1 / 50),
          MaxAccY = current.maxAccY
        };

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

        this.AttemptRecorded?.Invoke(item);
      }

      private void CloseLandingDetector()
      {
        this.CloseRequested?.Invoke();
      }

      internal void Stop()
      {
        var simCon = this.simObj.ESimCon;
        simCon.Structs.Unregister<LandingData>();
        this.simObj.ESimCon.DataReceived -= SimCon_DataReceived;
        this.isDisposed = true;
      }

      public void Dispose()
      {
        if (!isDisposed) Stop();
      }
    }
  }
}
