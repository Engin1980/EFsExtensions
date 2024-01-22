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
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.WPF.VMs;
using Eng.Chlaot.Modules.CopilotModule.Types;
using Eng.Chlaot.Modules.CopilotModule.Types.VMs;
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

    #region Fields

    private readonly Logger logger;
    private readonly SimObject simObject;

    #endregion Fields

    #region Properties

    public PropertyVMS PropertyVMs { get; set; }

    public BindingList<SpeechDefinitionVM> SpeechDefinitionVMs
    {
      get => base.GetProperty<BindingList<SpeechDefinitionVM>>(nameof(SpeechDefinitionVMs))!;
      set => base.UpdateProperty(nameof(SpeechDefinitionVMs), value);
    }

    public SpeechDefinitionVM EvaluatorRecentResultSpeechDefinitionVM
    {
      get => base.GetProperty<SpeechDefinitionVM>(nameof(EvaluatorRecentResultSpeechDefinitionVM))!;
      set => base.UpdateProperty(nameof(EvaluatorRecentResultSpeechDefinitionVM), value);
    }

    #endregion Properties

    #region Constructors

    internal RunContext(InitContext initContext)
    {
      this.logger = Logger.Create(this, "Copilot.RunContext");

      this.SpeechDefinitionVMs = initContext.SpeechDefinitionVMs;
      this.PropertyVMs = PropertyVMS.Create(initContext.SimPropertyGroup
        .GetAllSimPropertiesRecursively()
        .Where(q => initContext.PropertyUsageCounts.Any(p => p.Property == q)));
      this.SpeechDefinitionVMs.ForEach(q => q.CreateRunTime(PropertyVMs));

      this.simObject = SimObject.GetInstance();
      this.simObject.SimSecondElapsed += SimObject_SimSecondElapsed;
      this.simObject.SimPropertyChanged += SimObject_SimPropertyChanged;
      this.simObject.Started += () => this.simObject.RegisterProperties(this.PropertyVMs.Select(q => q.Property));
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

    private void EvaluateActives(IEnumerable<SpeechDefinitionVM> readys)
    {
      // play one one at once
      SpeechDefinitionVM? activated = readys
        .FirstOrDefault(q => q.RunTime.Evaluate(q.SpeechDefinition.Trigger));

      if (activated != null)
      {
        Player player = new(activated.SpeechDefinition.Speech.Bytes);
        player.PlayAsync();

        activated.RunTime.IsReadyToBeSpoken = false;
        this.logger.Invoke(LogLevel.VERBOSE,
          $"Activated speech {activated.SpeechDefinition.Title}");
      }
    }

    private void EvaluateForSpeeches()
    {
      //TODO xa probalby can delete this line
      //this.PropertyVMs.UpdateBySimObject(simObject);

      var readys = this.SpeechDefinitionVMs.Where(q => q.RunTime.IsReadyToBeSpoken);
      this.logger.Invoke(LogLevel.VERBOSE, $"Evaluating {readys.Count()} readys");
      EvaluateActives(readys);

      var waits = this.SpeechDefinitionVMs.Where(q => !q.RunTime.IsReadyToBeSpoken);
      this.logger.Invoke(LogLevel.VERBOSE, $"Evaluating {waits.Count()} waits");
      EvaluateInactives(waits);
    }

    private void EvaluateInactives(IEnumerable<SpeechDefinitionVM> waits)
    {
      waits
        .Where(q => q.SpeechDefinition.ReactivationTrigger != null)
        .Where(q => q.RunTime.Evaluate(q.SpeechDefinition.ReactivationTrigger!))
        .ForEach(q =>
        {
          q.RunTime.IsReadyToBeSpoken = true;
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
      this.PropertyVMs.SetIfExists(property, value);
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
