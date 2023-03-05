using ChlaotModuleBase;
using ChlaotModuleBase.ModuleUtils.Playing;
using ChlaotModuleBase.ModuleUtils.StateChecking;
using ChlaotModuleBase.ModuleUtils.StateCheckingSimConnection;
using ChlaotModuleBase.ModuleUtils.StateCheckingSimConnection.Mock;
using CopilotModule;
using CopilotModule.Types;
using ELogging;
using Eng.Chlaot.ChlaotModuleBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.Modules.CopilotModule
{
  public class RunContext
  {
    private const int INITIAL_CONNECTION_TIMER_INTERVAL = 1500;
    private const int REPEATED_CONNECTION_TIMER_INTERVAL = 10500;

    public class SpeechDefinitionInfo : NotifyPropertyChangedBase
    {
      public SpeechDefinitionInfo(SpeechDefinition speechDefinition)
      {
        SpeechDefinition = speechDefinition ?? throw new ArgumentNullException(nameof(speechDefinition));
        this.IsActive = true;
      }

      public SpeechDefinition SpeechDefinition { get; set; }

      public bool IsActive
      {
        get => base.GetProperty<bool>(nameof(IsActive))!;
        set => base.UpdateProperty(nameof(IsActive), value);
      }

      public DateTime? ReactivationDateTime
      {
        get => base.GetProperty<DateTime?>(nameof(ReactivationDateTime))!;
        set
        {
          base.UpdateProperty(nameof(ReactivationDateTime), value);
          this.IsActive = value == null;
        }
      }
    }
    public CopilotSet Set { get; private set; }
    public SimData SimData => this.simConManager.SimData;
    public BindingList<SpeechDefinitionInfo> Infos { get; set; } = new();
    private readonly StateCheckEvaluator evaluator;
    private readonly Settings settings;
    private readonly NewLogHandler logHandler;
    private readonly ISimConManager simConManager;
    private System.Timers.Timer? connectionTimer = null;

    public RunContext(InitContext initContext)
    {
      this.Set = initContext.Set;
      this.settings = initContext.Settings;
      this.logHandler = Logger.RegisterSender(this, "[Copilot.RunContext]");
#if USE_MOCK
      this.simConManager = SimConManagerMock.CreateTakeOff();
#else
      this.simConManager = new SimConManager();
#endif
      this.evaluator = new(this.simConManager.SimData);

      this.Set.SpeechDefinitions.ForEach(q => Infos.Add(new SpeechDefinitionInfo(q)));
    }

    private void SimConManager_SimSecondElapsed()
    {      
      if (this.simConManager.SimData.IsSimPaused) return;
      this.logHandler.Invoke(LogLevel.VERBOSE, "SimSecondElapsed (non-paused)");

      EvaluateForSpeeches();
    }

    private void EvaluateForSpeeches()
    {
      var readys = this.Infos.Where(q => q.IsActive);
      this.logHandler.Invoke(LogLevel.VERBOSE, $"Evaluating {readys.Count()} readys");
      EvaluateActives(readys);

      var waits = this.Infos.Where(q => !q.IsActive);
      this.logHandler.Invoke(LogLevel.VERBOSE, $"Evaluating {waits.Count()} waits");
      EvaluateInactives(waits);
    }

    private void EvaluateActives(IEnumerable<SpeechDefinitionInfo> readys)
    {
      // play one one at once
      SpeechDefinitionInfo? active = readys
        .FirstOrDefault(q => this.evaluator.Evaluate(q.SpeechDefinition.When));
      if (active != null)
      {
        Player player = new(active.SpeechDefinition.Speech.Bytes);
        player.PlayAsync();

        var rai = active.SpeechDefinition.ReactivateIn;
        active.ReactivationDateTime = rai == null || rai.Value < 0
            ? DateTime.Now.AddDays(365)
            : DateTime.Now.AddSeconds(active.SpeechDefinition.ReactivateIn!.Value);
        this.logHandler.Invoke(LogLevel.VERBOSE,
          $"Activated speech {active.SpeechDefinition.Title}, reactivation at {active.ReactivationDateTime}");
      }
    }

    private void EvaluateInactives(IEnumerable<SpeechDefinitionInfo> waits)
    {
      DateTime now = DateTime.Now;
      waits
        .Where(q => q.ReactivationDateTime < now)
        .ToList()
        .ForEach(q => q.ReactivationDateTime = null);
    }

    private void Log(LogLevel level, string message)
    {
      logHandler.Invoke(level, "[RunContext] :: " + message);
    }

    internal void Stop()
    {
      this.simConManager.SimSecondElapsed -= SimConManager_SimSecondElapsed;
      Log(LogLevel.INFO, "Stopped");
    }

    internal void Run()
    {
      Log(LogLevel.INFO, "Run");
      this.simConManager.SimSecondElapsed += SimConManager_SimSecondElapsed;

      logHandler.Invoke(LogLevel.VERBOSE, "Starting connection timer");
      this.connectionTimer = new System.Timers.Timer(INITIAL_CONNECTION_TIMER_INTERVAL)
      {
        AutoReset = true,
        Enabled = true
      };
      this.connectionTimer.Elapsed += ConnectionTimer_Elapsed;
    }

    private void ConnectionTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
      if (this.connectionTimer!.Interval == INITIAL_CONNECTION_TIMER_INTERVAL)
        this.connectionTimer!.Interval = REPEATED_CONNECTION_TIMER_INTERVAL;
      try
      {
        Log(LogLevel.VERBOSE, "Opening connection");
        this.simConManager.Open();
        Log(LogLevel.VERBOSE, "Opening connection - done");
        this.connectionTimer!.Stop();
        this.connectionTimer = null;

        this.simConManager.Start();
        Log(LogLevel.INFO, "Connected to FS2020, starting updates");
      }
      catch (Exception ex)
      {
        Log(LogLevel.WARNING, "Failed to connect to FS2020, will try it again in a few seconds...");
        Log(LogLevel.WARNING, "Fail reason: " + ex.GetFullMessage());
      }
    }
  }
}
