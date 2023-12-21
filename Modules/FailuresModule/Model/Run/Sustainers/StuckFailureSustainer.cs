using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateCheckingSimConnection;
using FailuresModule.Model.Run.Sustainers;
using FailuresModule.Model.Sim;

namespace FailuresModule.Types.Run.Sustainers
{
  internal class StuckFailureSustainer : SimVarBasedFailureSustainer
  {
    private const int UPDATE_TIMER_INTERVAL_MS = 100;
    private double? stuckValue = null;
    private bool isRunning = false;
    private Timer updateTimer;

    public StuckFailureSustainer(StuckFailureDefinition failure) : base(failure)
    {
      this.updateTimer = new Timer(UPDATE_TIMER_INTERVAL_MS);
      this.updateTimer.Elapsed += UpdateTimer_Elapsed;
      base.DataReceived += StuckFailureSustainer_DataReceived;
    }

    private void StuckFailureSustainer_DataReceived(double data)
    {
      if (this.stuckValue == null && isRunning)
      {
        lock (this)
        {
          if (this.stuckValue == null)
          {
            this.stuckValue = data;
            updateTimer.Start();
          }
        }
      }
    }

    protected override void ResetInternal()
    {
      lock (this)
      {
        this.updateTimer.Enabled = false;
        this.isRunning = false;
        this.stuckValue = null;
      }
    }

    protected override void StartInternal()
    {
      this.isRunning = true;
      RequestData();
    }

    private void UpdateTimer_Elapsed(object? sender, ElapsedEventArgs e)
    {
      lock (this)
      {
        Debug.Assert(this.stuckValue != null);
        base.SendData(this.stuckValue.Value);
      }
    }
  }
}
