using CopilotModule;
using ELogging;
using Eng.Chlaot.ChlaotModuleBase;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.Playing;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.SimConWrapping;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.SimConWrapping.PrdefinedTypes;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking;
using Eng.Chlaot.Modules.CopilotModule.Types;
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
    #region Public Classes

    public class SpeechDefinitionInfo : NotifyPropertyChangedBase
    {
      #region Public Properties

      public bool IsActive
      {
        get => base.GetProperty<bool>(nameof(IsActive))!;
        set => base.UpdateProperty(nameof(IsActive), value);
      }

      public List<StateCheckEvaluator.HistoryRecord>? ReactivationEvaluationHistory
      {
        get => base.GetProperty<List<StateCheckEvaluator.HistoryRecord>?>(nameof(ReactivationEvaluationHistory))!;
        set => base.UpdateProperty(nameof(ReactivationEvaluationHistory), value);
      }

      public SpeechDefinition SpeechDefinition { get; set; }

      public List<StateCheckEvaluator.HistoryRecord>? WhenEvaluationHistory
      {
        get => base.GetProperty<List<StateCheckEvaluator.HistoryRecord>?>(nameof(WhenEvaluationHistory))!;
        set => base.UpdateProperty(nameof(WhenEvaluationHistory), value);
      }

      #endregion Public Properties

      #region Public Constructors

      public SpeechDefinitionInfo(SpeechDefinition speechDefinition)
      {
        SpeechDefinition = speechDefinition ?? throw new ArgumentNullException(nameof(speechDefinition));
        this.IsActive = true;
      }

      #endregion Public Constructors
    }

    #endregion Public Classes

    #region Private Fields

    private readonly object evaluatingLock = new();
    private readonly StateCheckEvaluator evaluator;
    private readonly Logger logger;
    private readonly Dictionary<string, double> propertyValues = new();
    private readonly Settings settings;
    private readonly SimConWrapperWithSimData simConWrapper;
    private readonly Dictionary<string, double> variableValues = new();

    #endregion Private Fields

    #region Public Properties

    public SpeechDefinitionInfo? DebugEvalHistorySource
    {
      get => base.GetProperty<SpeechDefinitionInfo?>(nameof(DebugEvalHistorySource))!;
      set => base.UpdateProperty(nameof(DebugEvalHistorySource), value);
    }

    public BindingList<SpeechDefinitionInfo> Infos { get; set; } = new();
    public CopilotSet Set { get; private set; }
    public SimData SimData { get => this.simConWrapper.SimData; }

    #endregion Public Properties

    #region Public Constructors

    public RunContext(InitContext initContext)
    {
      this.Set = initContext.Set;
      this.settings = initContext.Settings;
      this.logger = Logger.Create(this, "Copilot.RunContext");

      ESimConnect.ESimConnect simCon = new();
      this.simConWrapper = new(simCon);

      this.evaluator = new(variableValues, propertyValues);

      this.Set.SpeechDefinitions.ForEach(q => Infos.Add(new SpeechDefinitionInfo(q)));
    }

    #endregion Public Constructors

    #region Internal Methods

    internal void Run()
    {
      Log(LogLevel.INFO, "Run");
      this.simConWrapper.SimSecondElapsed += SimConWrapper_SimSecondElapsed;

      logger.Invoke(LogLevel.VERBOSE, "Starting connection timer");
      this.simConWrapper.OpenAsync(
        () =>
        {
          this.simConWrapper.Start();
          Log(LogLevel.INFO, "Connected to FS2020, starting updates");
        },
        ex =>
        {
          Log(LogLevel.WARNING, "Failed to connect to FS2020, will try it again in a few seconds...");
          Log(LogLevel.WARNING, "Fail reason: " + ex.GetFullMessage());
        });
    }

    internal void Stop()
    {
      throw new NotImplementedException();
      //this.simConManager.SimSecondElapsed -= SimConManager_SimSecondElapsed;
      //Log(LogLevel.INFO, "Stopped");
    }

    #endregion Internal Methods

    #region Private Methods

    private void EvaluateActives(IEnumerable<SpeechDefinitionInfo> readys)
    {
      // play one one at once
      SpeechDefinitionInfo? activated = readys
        .FirstOrDefault(q =>
        {
          List<StateCheckEvaluator.HistoryRecord>? history = this.settings.EvalDebugEnabled
            ? new List<StateCheckEvaluator.HistoryRecord>()
            : null;

          var ret = this.evaluator.Evaluate(q.SpeechDefinition.Trigger, history);
          q.WhenEvaluationHistory = history;

          return ret;
        });

      if (activated != null)
      {
        Player player = new(activated.SpeechDefinition.Speech.Bytes);
        player.PlayAsync();

        activated.IsActive = false;
        this.logger.Invoke(LogLevel.VERBOSE,
          $"Activated speech {activated.SpeechDefinition.Title}");
      }
    }

    private void EvaluateForSpeeches()
    {
      StateCheckEvaluator.UpdateDictionaryByObject(SimData, propertyValues);

      if (this.settings.EvalDebugEnabled)
        this.Infos.ToList().ForEach(q =>
        {
          q.WhenEvaluationHistory = null;
          q.ReactivationEvaluationHistory = null;
        });

      var readys = this.Infos.Where(q => q.IsActive);
      this.logger.Invoke(LogLevel.VERBOSE, $"Evaluating {readys.Count()} readys");
      EvaluateActives(readys);

      var waits = this.Infos.Where(q => !q.IsActive);
      this.logger.Invoke(LogLevel.VERBOSE, $"Evaluating {waits.Count()} waits");
      EvaluateInactives(waits);
    }

    private void EvaluateInactives(IEnumerable<SpeechDefinitionInfo> waits)
    {
      waits
        .Where(q =>
        {
          var h = GetHistoryListIfRequired();
          var ret = this.evaluator.Evaluate(q.SpeechDefinition.ReactivationTrigger, h);
          q.ReactivationEvaluationHistory = h;
          return ret;
        })
        .ForEach(q =>
        {
          q.IsActive = true;
          this.logger.Invoke(LogLevel.VERBOSE,
          $"Reactivated speech {q.SpeechDefinition.Title}");
        });
    }

    private List<StateCheckEvaluator.HistoryRecord>? GetHistoryListIfRequired()
    {
      List<StateCheckEvaluator.HistoryRecord>? ret = this.settings.EvalDebugEnabled
            ? new List<StateCheckEvaluator.HistoryRecord>()
            : null;
      return ret;
    }

    private void Log(LogLevel level, string message)
    {
      logger.Invoke(level, "[RunContext] :: " + message);
    }

    private void SimConWrapper_SimSecondElapsed()
    {
      if (this.simConWrapper.IsSimPaused) return;
      this.logger.Invoke(LogLevel.VERBOSE, "SimSecondElapsed (non-paused)");

      if (Monitor.TryEnter(evaluatingLock) == false)
      {
        this.logger.Invoke(LogLevel.WARNING, "SimSecondElapsed took longer than sim-second! Performance issue?");
        return;
      }

      EvaluateForSpeeches();

      Monitor.Exit(evaluatingLock);
    }

    #endregion Private Methods
  }
}
