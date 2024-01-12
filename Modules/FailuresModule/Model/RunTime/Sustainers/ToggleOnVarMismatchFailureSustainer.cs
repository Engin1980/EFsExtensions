using ELogging;
using FailuresModule.Model.Failures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace FailuresModule.Model.Run.Sustainers
{
  internal class ToggleOnVarMismatchFailureSustainer : SimVarBasedFailureSustainer
  {
    #region Private Fields

    private readonly ToggleOnVarMismatchFailureDefinition failure;
    private readonly Timer updateTimer;
    private bool isRunning = false;
    private bool isDataRequested = false;

    #endregion Private Fields

    #region Public Constructors

    public ToggleOnVarMismatchFailureSustainer(ToggleOnVarMismatchFailureDefinition failure) : base(failure)
    {
      this.failure = failure;
      this.updateTimer = new Timer(this.failure.RefreshIntervalInMs);
      this.updateTimer.Elapsed += UpdateTimer_Elapsed;
      base.DataReceived += StuckFailureSustainer_DataReceived;
      //TODO is not using "onlyWhenChanged" flag
    }

    #endregion Public Constructors

    #region Protected Methods

    protected override void InitInternal()
    {
      base.InitInternal();
    }

    protected override void ResetInternal()
    {
      lock (this)
      {
        this.updateTimer.Enabled = false;
        this.isRunning = false;
      }
    }

    protected override void StartInternal()
    {
      this.isRunning = true;
      this.isDataRequested = false;
      updateTimer.Start();
    }

    #endregion Protected Methods

    #region Private Methods

    private void StuckFailureSustainer_DataReceived(double data)
    {
      Logger.Log(this, LogLevel.INFO, "ReceivedData");
      if (data != this.failure.FailValue && isRunning)
      {
        Logger.Log(this, LogLevel.INFO, "Invoking event");
        base.SimCon.SendClientEvent(this.failure.SimEvent, null, false);
      }
      this.isDataRequested = false;
    }
    private void UpdateTimer_Elapsed(object? sender, ElapsedEventArgs e)
    {
      lock (updateTimer)
      {
        Logger.Log(this, LogLevel.INFO, "UpdateTimer_Elapsed");
        if (!isDataRequested)
        {
          Logger.Log(this, LogLevel.INFO, "UpdateTimer_Elapsed - requesting data");
          isDataRequested = true;
          RequestData();
        }
      }
    }

    #endregion Private Methods

  }
}
