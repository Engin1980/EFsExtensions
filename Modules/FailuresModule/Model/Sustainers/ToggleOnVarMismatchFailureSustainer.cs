using ESystem.Logging;
using Eng.EFsExtensions.Modules.FailuresModule.Model.Failures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Eng.EFsExtensions.Modules.FailuresModule.Model.Sustainers
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
      updateTimer = new Timer(this.failure.RefreshIntervalInMs);
      updateTimer.Elapsed += UpdateTimer_Elapsed;
      DataReceived += StuckFailureSustainer_DataReceived;
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
        updateTimer.Enabled = false;
        isRunning = false;
      }
    }

    protected override void StartInternal()
    {
      isRunning = true;
      isDataRequested = false;
      updateTimer.Start();
    }

    #endregion Protected Methods

    #region Private Methods

    private void StuckFailureSustainer_DataReceived(double data)
    {
      Logger.Log(this, LogLevel.INFO, "ReceivedData");
      if (data != failure.FailValue && isRunning)
      {
        Logger.Log(this, LogLevel.INFO, "Invoking event");
        ESimObj.ESimCon.ClientEvents.Invoke(failure.SimEvent);
      }
      isDataRequested = false;
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
