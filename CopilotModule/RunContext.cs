using ChlaotModuleBase;
using ChlaotModuleBase.ModuleUtils.StateChecking;
using ChlaotModuleBase.ModuleUtils.StateCheckingSimConnection;
using CopilotModule;
using CopilotModule.Types;
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
    public BindingList<SpeechDefinitionInfo> Infos = new BindingList<SpeechDefinitionInfo>();
    private StateCheckEvaluator evaluator;
    private Settings settings;
    private LogHandler logHandler;
    private SimConManager simConManager;
    public RunContext(InitContext initContext, LogHandler logHandler)
    {
      this.Set = initContext.Set;
      this.settings = initContext.Settings;
      this.logHandler = logHandler;
      this.simConManager = new SimConManager(this.logHandler, "copilot_simcon_log.txt");
      this.simConManager.SimSecondElapsed += SimConManager_SimSecondElapsed;
      this.evaluator = new StateCheckEvaluator(this.simConManager.SimData, logHandler);

      this.Set.SpeechDefinitions.ForEach(q => Infos.Add(new SpeechDefinitionInfo(q)));
    }

    private void SimConManager_SimSecondElapsed()
    {
      if (this.simConManager.SimData.IsSimPaused) return;

      EvaluateForSpeeches();
    }

    private void EvaluateForSpeeches()
    {
      var readys = this.Infos.Where(q => q.IsActive);
      EvaluateActives(readys);

      var waits = this.Infos.Where(q => !q.IsActive);
      EvaluateInactives(waits);
    }

    private void EvaluateActives(IEnumerable<SpeechDefinitionInfo> readys)
    {
      // play one one at once
      SpeechDefinitionInfo? active = readys.FirstOrDefault(q => this.evaluator.Evaluate(q.SpeechDefinition.When));
      if (active != null)
      {
        finish here
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
      logHandler?.Invoke(level, "[RunContext] :: " + message);
    }

    internal void Stop()
    {
      throw new NotImplementedException();
    }

    internal void Run()
    {

    }
  }
}
