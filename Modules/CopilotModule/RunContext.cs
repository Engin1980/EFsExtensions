using ChlaotModuleBase;
using ChlaotModuleBase.ModuleUtils.StateChecking;
using CopilotModule;
using ELogging;
using Eng.Chlaot.ChlaotModuleBase;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.Playing;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.SimConWrapping;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.SimConWrapping.PrdefinedTypes;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.SimObjects;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking.VariableModel;
using Eng.Chlaot.Modules.CopilotModule.Types;
using ESystem.Asserting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Eng.Chlaot.Modules.CopilotModule
{
  internal class RunContext : NotifyPropertyChangedBase
  {

    #region Classes + Structs

    public class SpeechDefinitionInfo : NotifyPropertyChangedBase
    {

      #region Properties

      public StateCheckEvaluator Evaluator
      {
        get => base.GetProperty<StateCheckEvaluator>(nameof(Evaluator))!;
        set => base.UpdateProperty(nameof(Evaluator), value);
      }

      public bool IsReadyToBeSpoken
      {
        get => base.GetProperty<bool>(nameof(IsReadyToBeSpoken))!;
        set => base.UpdateProperty(nameof(IsReadyToBeSpoken), value);
      }

      public SpeechDefinition SpeechDefinition { get; set; }
      public Dictionary<string, double> VariableValuesDict
      {
        get => base.GetProperty<Dictionary<string, double>>(nameof(VariableValuesDict))!;
        set => base.UpdateProperty(nameof(VariableValuesDict), value);
      }

      #endregion Properties

      #region Constructors

      public SpeechDefinitionInfo(SpeechDefinition speechDefinition, Func<Dictionary<string, double>> propertyValuesProvider)
      {
        EAssert.Argument.IsNotNull(speechDefinition, nameof(speechDefinition));
        EAssert.Argument.IsNotNull(propertyValuesProvider, nameof(propertyValuesProvider));

        this.SpeechDefinition = speechDefinition;
        this.IsReadyToBeSpoken = true;
        this.VariableValuesDict = new();
        this.Evaluator = new StateCheckEvaluator(() => this.VariableValuesDict, propertyValuesProvider);
        FillVariableValuesDict();
      }

      #endregion Constructors

      #region Methods

      private void FillVariableValuesDict()
      {
        foreach (var varUsage in StateCheckUtils.ExtractVariables(this.SpeechDefinition.Trigger))
        {
          Variable var = this.SpeechDefinition.Variables.First(q => q.Name == varUsage.VariableName);
          this.VariableValuesDict[var.Name] = var.Value;
        }
        if (this.SpeechDefinition.ReactivationTrigger != null)
          foreach (var varUsage in StateCheckUtils.ExtractVariables(this.SpeechDefinition.ReactivationTrigger))
          {
            Variable var = this.SpeechDefinition.Variables.First(q => q.Name == varUsage.VariableName);
            this.VariableValuesDict[var.Name] = var.Value;
          }
      }

      #endregion Methods

    }

    #endregion Classes + Structs

    #region Fields

    private readonly Logger logger;
    private readonly Dictionary<string, double> propertyValues = new();
    private readonly SimObject simObject;

    #endregion Fields

    #region Properties


    public SpeechDefinitionInfo EvaluatorRecentResultSpeechDefinitionInfo
    {
      get => base.GetProperty<SpeechDefinitionInfo>(nameof(EvaluatorRecentResultSpeechDefinitionInfo))!;
      set => base.UpdateProperty(nameof(EvaluatorRecentResultSpeechDefinitionInfo), value);
    }

    public BindingList<StateCheckEvaluator.RecentResult> EvaluatorRecentResultView
    {
      get => base.GetProperty<BindingList<StateCheckEvaluator.RecentResult>>(nameof(EvaluatorRecentResultView))!;
      set => base.UpdateProperty(nameof(EvaluatorRecentResultView), value);
    }

    public BindingList<SpeechDefinitionInfo> Infos { get; set; } = new(); //TODO rename to runtimes
    public BindingList<BindingKeyValue<string, double>> PropertyValuesView { get; set; } = new();
    public CopilotSet Set { get; private set; }

    #endregion Properties

    #region Constructors

    internal RunContext(InitContext initContext)
    {
      this.logger = Logger.Create(this, "Copilot.RunContext");

      this.Set = initContext.Set;
      this.Set.SpeechDefinitions.ForEach(q => Infos.Add(new SpeechDefinitionInfo(q, () => this.propertyValues)));

      var allProps = initContext.SimPropertyGroup.GetAllSimPropertiesRecursively()
        .Where(q => initContext.PropertyUsageCounts.Any(p => p.Property == q))
        .ToList();
      PropertyValuesView = new BindingList<BindingKeyValue<string, double>>(
        allProps.Select(q => new BindingKeyValue<string, double>(q.Name, double.NaN)).OrderBy(q => q.Key).ToList()
        );

      this.simObject = SimObject.GetInstance();
      this.simObject.SimSecondElapsed += SimObject_SimSecondElapsed;
      this.simObject.SimPropertyChanged += SimObject_SimPropertyChanged;
      this.simObject.Started += () => this.simObject.RegisterProperties(allProps);
    }

    #endregion Constructors

    #region Methods

    internal void Run()
    {
      Log(LogLevel.INFO, "Run");

      logger?.Invoke(LogLevel.VERBOSE, "Starting simObject connection");
      this.simObject.StartAsync();
    }

    internal void Stop()
    {
      throw new NotImplementedException();
      //this.simConManager.SimSecondElapsed -= SimConManager_SimSecondElapsed;
      //Log(LogLevel.INFO, "Stopped");
    }

    private void EvaluateActives(IEnumerable<SpeechDefinitionInfo> readys)
    {
      // play one one at once
      SpeechDefinitionInfo? activated = readys
        .FirstOrDefault(q => q.Evaluator.Evaluate(q.SpeechDefinition.Trigger));

      if (activated != null)
      {
        Player player = new(activated.SpeechDefinition.Speech.Bytes);
        player.PlayAsync();

        activated.IsReadyToBeSpoken = false;
        this.logger.Invoke(LogLevel.VERBOSE,
          $"Activated speech {activated.SpeechDefinition.Title}");
      }
    }

    private void EvaluateForSpeeches()
    {
      StateCheckEvaluator.UpdateDictionaryBySimObject(simObject, propertyValues);

      var readys = this.Infos.Where(q => q.IsReadyToBeSpoken);
      this.logger.Invoke(LogLevel.VERBOSE, $"Evaluating {readys.Count()} readys");
      EvaluateActives(readys);

      var waits = this.Infos.Where(q => !q.IsReadyToBeSpoken);
      this.logger.Invoke(LogLevel.VERBOSE, $"Evaluating {waits.Count()} waits");
      EvaluateInactives(waits);

      if (EvaluatorRecentResultSpeechDefinitionInfo != null)
      {
        this.EvaluatorRecentResultView = new(EvaluatorRecentResultSpeechDefinitionInfo.Evaluator.GetRecentResultSet());
      }
    }

    private void EvaluateInactives(IEnumerable<SpeechDefinitionInfo> waits)
    {
      waits
        .Where(q => q.SpeechDefinition.ReactivationTrigger != null)
        .Where(q => q.Evaluator.Evaluate(q.SpeechDefinition.ReactivationTrigger!))
        .ForEach(q =>
        {
          q.IsReadyToBeSpoken = true;
          this.logger.Invoke(LogLevel.VERBOSE,
          $"Reactivated speech {q.SpeechDefinition.Title}");
        });
    }

    private void Log(LogLevel level, string message)
    {
      logger.Invoke(level, "[RunContext] :: " + message);
    }

    private void SimObject_SimPropertyChanged(SimProperty property, double value)
    {
      this.propertyValues[property.Name] = value;
      this.PropertyValuesView.First(q => q.Key == property.Name).Value = value;
    }
    private void SimObject_SimSecondElapsed()
    {
      if (this.simObject.IsSimPaused) return;
      this.logger.Invoke(LogLevel.VERBOSE, "SimSecondElapsed (non-paused)");

      if (Monitor.TryEnter(this) == false)
      {
        this.logger.Invoke(LogLevel.WARNING, "SimSecondElapsed() method calculation took longer than sim-second time interval! Performance issue?");
        return;
      }

      EvaluateForSpeeches();

      Monitor.Exit(this);
    }

    #endregion Methods

  }
}
