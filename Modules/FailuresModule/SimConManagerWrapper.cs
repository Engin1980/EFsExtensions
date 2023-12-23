using ELogging;
using Eng.Chlaot.ChlaotModuleBase;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateCheckingSimConnection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FailuresModule
{
  public class SimConManagerWrapper
  {
    public class ConnectionFailedException : Exception
    {
      public ConnectionFailedException(Exception? innerException) : base("Failed to connect to FS2020", innerException)
      {
      }
    }

    public class StartFailedException : Exception
    {
      public StartFailedException(Exception? innerException) : base("Failed to start read-out of FS2020", innerException)
      {
      }
    }

    private readonly SimConManager simConManager;
    public delegate void SimConManagerWrapperDelegate();
    public event SimConManagerWrapperDelegate? SimSecondElapsed;
    public event SimConManagerWrapperDelegate? SimConnected;
    public delegate void SimConManagerWrapperErrorDelegate(Exception ex);
    public event SimConManagerWrapperErrorDelegate? SimErrorRaised;

    private const int INITIAL_CONNECTION_TIMER_INTERVAL = 2000;
    private const int REPEATED_CONNECTION_TIMER_INTERVAL = 10000;

    private System.Timers.Timer? connectionTimer = null;

    public bool IsRunning { get; private set; }

    public SimConManagerWrapper(ESimConnect.ESimConnect simConnect)
    {
      this.simConManager = new SimConManager(simConnect);
      this.simConManager.SimSecondElapsed += Sim_SimSecondElapsed;
    }

    public SimData SimData { get => simConManager.SimData; }

    private void Sim_SimSecondElapsed()
    {
      Log(LogLevel.VERBOSE, "FS Second Elapsed");
      if (this.SimData.IsSimPaused) return;

      Log(LogLevel.INFO, "Connected to FS2020, starting updates");
      this.SimSecondElapsed?.Invoke();
    }

    public void StartAsync()
    {
      this.connectionTimer = new System.Timers.Timer(INITIAL_CONNECTION_TIMER_INTERVAL);
      this.connectionTimer.Elapsed += ConnectionTimer_Elapsed;
      this.connectionTimer.AutoReset = false;
      this.connectionTimer.Start();
    }

    private void ConnectionTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
      try
      {
        Log(LogLevel.VERBOSE, "Opening connection");
        this.simConManager.Open();
        Log(LogLevel.VERBOSE, "Opening connection - done");
      }
      catch (Exception ex)
      {
        Log(LogLevel.WARNING, "Failed to connect to FS2020, will try it again in a few seconds...");
        Log(LogLevel.WARNING, "Fail reason: " + ex.GetFullMessage());
        this.SimErrorRaised?.Invoke(new ConnectionFailedException(ex));
        this.connectionTimer!.Interval = REPEATED_CONNECTION_TIMER_INTERVAL; // this autostarts the timer
      }
      try
      {
        this.simConManager.Start();
        this.IsRunning = true;
        Log(LogLevel.INFO, "Connected to FS2020, starting updates");
      }
      catch (Exception ex)
      {
        Log(LogLevel.WARNING, "Failed to start read-out of FS2020...");
        Log(LogLevel.WARNING, "Fail reason: " + ex.GetFullMessage());
        this.SimErrorRaised?.Invoke(new StartFailedException(ex));
      }
    }

    private void Log(LogLevel level, string message)
    {
      Logger.Log(this, level, message);
    }

    internal void StopAsync()
    {
      if (this.IsRunning)
      {
        //TODO not tested
        this.simConManager.Close();
      }
    }
  }
}
