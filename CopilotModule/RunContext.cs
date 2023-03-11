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
using System.Threading;
using System.Threading.Tasks;

namespace Eng.Chlaot.Modules.CopilotModule
{
  public class RunContext : NotifyPropertyChangedBase
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

      public List<StateCheckEvaluator.HistoryRecord>? WhenEvaluationHistory
      {
        get => base.GetProperty<List<StateCheckEvaluator.HistoryRecord>?>(nameof(WhenEvaluationHistory))!;
        set => base.UpdateProperty(nameof(WhenEvaluationHistory), value);
      }

      public List<StateCheckEvaluator.HistoryRecord>? ReactivationEvaluationHistory
      {
        get => base.GetProperty<List<StateCheckEvaluator.HistoryRecord>?>(nameof(ReactivationEvaluationHistory))!;
        set => base.UpdateProperty(nameof(ReactivationEvaluationHistory), value);
      }
    }

    public CopilotSet Set { get; private set; }
    public SimData SimData => this.simConManager.SimData;

    public SpeechDefinitionInfo? DebugEvalHistorySource
    {
      get => base.GetProperty<SpeechDefinitionInfo?>(nameof(DebugEvalHistorySource))!;
      set => base.UpdateProperty(nameof(DebugEvalHistorySource), value);
    }

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
      //this.simConManager = new SimConManager();
      this.simConManager = SimConManagerMock.CreateTakeOff();
#endif
      this.evaluator = new(this.simConManager.SimData);

      this.Set.SpeechDefinitions.ForEach(q => Infos.Add(new SpeechDefinitionInfo(q)));
    }

    private readonly object evaluatingLock = new object();
    private void SimConManager_SimSecondElapsed()
    {
      if (this.simConManager.SimData.IsSimPaused) return;
      this.logHandler.Invoke(LogLevel.VERBOSE, "SimSecondElapsed (non-paused)");

      if (Monitor.TryEnter(evaluatingLock) == false)
      {
        this.logHandler.Invoke(LogLevel.WARNING, "SimSecondElapsed took longer than sim-second! Performance issue?");
        return;
      }

      EvaluateForSpeeches();

      Monitor.Exit(evaluatingLock);
    }

    private void EvaluateForSpeeches()
    {
      if (this.settings.EvalDebugEnabled)
        this.Infos.ToList().ForEach(q =>
        {
          q.WhenEvaluationHistory = null;
          q.ReactivationEvaluationHistory = null;
        });

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
      SpeechDefinitionInfo? activated = readys
        .FirstOrDefault(q =>
        {
          List<StateCheckEvaluator.HistoryRecord>? history = this.settings.EvalDebugEnabled
            ? new List<StateCheckEvaluator.HistoryRecord>()
            : null;

          var ret = this.evaluator.Evaluate(q.SpeechDefinition.When, history);
          q.WhenEvaluationHistory = history;

          return ret;
        });

      if (activated != null)
      {
        Player player = new(activated.SpeechDefinition.Speech.Bytes);
        player.PlayAsync();

        activated.IsActive = false;
        this.logHandler.Invoke(LogLevel.VERBOSE,
          $"Activated speech {activated.SpeechDefinition.Title}");
      }
    }

    private List<StateCheckEvaluator.HistoryRecord>? GetHistoryListIfRequired()
    {
      List<StateCheckEvaluator.HistoryRecord>? ret = this.settings.EvalDebugEnabled
            ? new List<StateCheckEvaluator.HistoryRecord>()
            : null;
      return ret;
    }

    private void EvaluateInactives(IEnumerable<SpeechDefinitionInfo> waits)
    {
      waits
        .Where(q =>
        {
          var h = GetHistoryListIfRequired();
          var ret = this.evaluator.Evaluate(q.SpeechDefinition.ReactivateWhen, h);
          q.ReactivationEvaluationHistory = h;
          return ret;
        })
        .ForEach(q =>
        {
          q.IsActive = true;
          this.logHandler.Invoke(LogLevel.VERBOSE,
          $"Reactivated speech {q.SpeechDefinition.Title}");
        });
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
