﻿using ESystem.Logging;
using Eng.EFsExtensions.EFsExtensionsModuleBase;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.AudioPlaying;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.SimObjects;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.WPF.VMs;
using Eng.EFsExtensions.Modules.CopilotModule.Types.VMs;
using ESystem.Asserting;
using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace Eng.EFsExtensions.Modules.CopilotModule
{
  internal class RunContext : NotifyPropertyChanged
  {
    private readonly AudioPlayManager audioPlayManager = AudioPlayManagerProvider.Instance;

    #region Fields

    private readonly Logger logger;
    private readonly NewSimObject eSimObj;

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

      this.eSimObj = NewSimObject.GetInstance();
      this.eSimObj.SimSecondElapsed += SimObject_SimSecondElapsed;
      this.eSimObj.SimPropertyChanged += SimObject_SimPropertyChanged;
      this.eSimObj.Started += () => this.eSimObj.RegisterProperties(this.PropertyVMs.Select(q => q.Property));
    }

    #endregion Constructors

    #region Methods

    internal void Run()
    {
      Log(LogLevel.INFO, "Run");

      logger.Log(LogLevel.DEBUG, "Starting simObject connection");
      this.eSimObj.ExtOpen.OpenInBackground();
    }

    internal void Stop()
    {
      // intentionally blank; Run does "StartAsync()", once started, repeated start does nothing
      Log(LogLevel.INFO, "Stopped");
    }

    private void EvaluateActives(IEnumerable<SpeechDefinitionVM> readys)
    {
      // play one one at once
      SpeechDefinitionVM? activated = readys
        .FirstOrDefault(q => q.RunTime.Evaluate(q.SpeechDefinition.Trigger));

      if (activated != null)
      {
        audioPlayManager.Enqueue(activated.SpeechDefinition.Speech.Bytes, AudioPlayManager.CHANNEL_COPILOT);

        activated.RunTime.IsReadyToBeSpoken = false;
        this.logger.Log(LogLevel.DEBUG,
          $"Activated speech {activated.SpeechDefinition.Title}");
      }
    }

    private void EvaluateForSpeeches()
    {
      var readys = this.SpeechDefinitionVMs.Where(q => q.RunTime.IsReadyToBeSpoken);
      this.logger.Log(LogLevel.DEBUG, $"Evaluating {readys.Count()} readys");
      EvaluateActives(readys);

      var waits = this.SpeechDefinitionVMs.Where(q => !q.RunTime.IsReadyToBeSpoken);
      this.logger.Log(LogLevel.DEBUG, $"Evaluating {waits.Count()} waits");
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
          this.logger.Log(LogLevel.DEBUG,
          $"Reactivated speech {q.SpeechDefinition.Title}");
        });
    }

    private void Log(LogLevel level, string message)
    {
      logger.Log(level, "[RunContext] :: " + message);
    }

    private void SimObject_SimPropertyChanged(SimProperty property, double value)
    {
      this.PropertyVMs.SetIfExists(property, value);
    }
    private void SimObject_SimSecondElapsed()
    {
      if (this.eSimObj.IsSimPaused) return;
      this.logger.Log(LogLevel.DEBUG, "SimSecondElapsed (non-paused)");

      if (Monitor.TryEnter(this) == false)
      {
        this.logger.Log(LogLevel.WARNING, "SimSecondElapsed() method calculation took longer than sim-second time interval! Performance issue?");
        return;
      }

      EvaluateForSpeeches();

      Monitor.Exit(this);
    }

    #endregion Methods

  }
}
