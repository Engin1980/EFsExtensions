using ChlaotModuleBase.ModuleUtils.SimConWrapping.Exceptions;
using ELogging;
using Eng.Chlaot.ChlaotModuleBase;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.SimConWrapping;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking;
using Eng.Chlaot.Modules.FailuresModule.Model.Incidents;
using Eng.Chlaot.Modules.FailuresModule.Model.Failures;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.WPF.VMs;
using Eng.Chlaot.Modules.FailuresModule.Model.VMs;
using Eng.Chlaot.Modules.FailuresModule.Model.Sustainers;

namespace Eng.Chlaot.Modules.FailuresModule
{
    public class RunContext : NotifyPropertyChangedBase
  {
    #region Fields

    private readonly Random random = new();
    private readonly SimConWrapperWithSimData simConWrapper;
    private List<IncidentDefinitionVM>? _IncidentDefinitions = null;
    private bool isRunning = false;
    private readonly Dictionary<string, double> propertyValues = new();

    #endregion Fields

    #region Properties

    public List<FailureDefinition> FailureDefinitions { get; }
    public List<IncidentVM> IncidentVMs { get; }
    public BindingList<FailureSustainer> Sustainers { get; }

    public int SustainersCount
    {
      get => base.GetProperty<int>(nameof(SustainersCount))!;
      set => base.UpdateProperty(nameof(SustainersCount), value);
    }
    internal List<IncidentDefinitionVM> IncidentDefinitions
    {
      get
      {
        if (_IncidentDefinitions == null)
        {
          _IncidentDefinitions = FlattenIncidentDefinitions(IncidentVMs);
        }
        return _IncidentDefinitions;
      }
    }

    #endregion Properties

    #region Constructors

    public RunContext(InitContext initContext)
    {
      // preparation
      List<FailureDefinition> failureDefinitions = initContext.FailureDefinitionsFlat;
      IncidentGroup rootIncidentGroup = new IncidentGroup()
      {
        Incidents = initContext.FailureSet.Incidents
      };
      IncidentGroupVM top = IncidentGroupVM.Create(rootIncidentGroup, () => propertyValues);

      //RunContext ret = new(failureDefinitions, top.Incidents);
      //return ret;


      // creation
      ESimConnect.ESimConnect eSimCon = new();
      simConWrapper = new(eSimCon);
      simConWrapper.SimSecondElapsed += SimConWrapper_SimSecondElapsed;
      simConWrapper.SimConErrorRaised += SimConWrapper_SimConErrorRaised;

      FailureSustainer.SetSimCon(eSimCon);
      FailureDefinitions = failureDefinitions;
      IncidentVMs = top.Incidents;
      Sustainers = new();
      Sustainers.ListChanged += (s, e) => this.SustainersCount = Sustainers.Count;
    }

    #endregion Constructors

    #region Methods

    public void Start()
    {
      this.simConWrapper.OpenAsync(
        () =>
        {
          this.simConWrapper.Start();
          this.isRunning = true;
        },
        ex => { });
    }

    private static List<IncidentDefinitionVM> FlattenIncidentDefinitions(List<IncidentVM> incidents)
    {
      List<IncidentDefinitionVM> ret = new();

      foreach (var incident in incidents)
      {
        if (incident is IncidentGroupVM rig)
        {
          var tmp = FlattenIncidentDefinitions(rig.Incidents);
          ret.AddRange(tmp);
        }
        else if (incident is IncidentDefinitionVM rid)
        {
          ret.Add(rid);
        }
      }

      return ret;
    }

    private void EvaluateAndFireFailures()
    {
      foreach (var runIncidentDefinition in this.IncidentDefinitions)
      {
        EvaluateIncidentDefinition(runIncidentDefinition, out bool isActivated);
        if (!isActivated) continue;

        List<FailId> failItems = PickFailItems(runIncidentDefinition);
        List<FailureDefinition> failDefs = failItems.Select(q => this.FailureDefinitions.First(p => q.Id == p.Id)).ToList();
        StartFailures(failDefs);
      }
    }

    private void EvaluateIncidentDefinition(IncidentDefinitionVM incident, out bool isActivated)
    {
      isActivated = false;
      foreach (var trigger in incident.Triggers)
      {
        if (incident.InvokedOneShotTriggers.Contains(trigger)) continue;

        bool isConditionTrue = trigger.Evaluate();

        if (isConditionTrue)
        {
          if (trigger.Trigger.Repetitive == false)
            incident.InvokedOneShotTriggers.Add(trigger);

          double prob = random.NextDouble();
          isActivated = prob <= trigger.Trigger.Probability;
          if (isActivated) return;
        }
      }
    }

    private static List<FailId> FlattenFailGroup(Fail failItem)
    {
      void DoFlattening(Fail fi, List<FailId> lst)
      {
        if (fi is FailGroup fg)
          fg.Items.ForEach(q => DoFlattening(q, lst));
        else if (fi is FailId f)
          lst.Add(f);
        else
          throw new NotImplementedException();
      }
      List<FailId> ret = new();
      DoFlattening(failItem, ret);
      return ret;
    }

    private void StartFailures(List<FailureDefinition> failures)
    {
      foreach (var failure in failures)
      {
        if (this.Sustainers.Any(q => q.Failure == failure)) continue;
        FailureSustainer fs = FailureSustainerFactory.Create(failure);
        if (fs is SneakFailureSustainer sfs)
          sfs.Finished += SneakFailureSustainer_Finished;
        this.Sustainers.Add(fs);
        fs.Start();
      }
    }

    private void SneakFailureSustainer_Finished(SneakFailureSustainer sustainer)
    {
      this.Sustainers.Remove(sustainer);
      FailureDefinition finalFailure = FailureDefinitions.First(q => q.Id == sustainer.Failure.FinalFailureId);
      if (this.Sustainers.Any(q => q.Failure == finalFailure)) return;
      FailureSustainer fs = FailureSustainerFactory.Create(finalFailure);
      this.Sustainers.Add(fs);
      fs.Start();
    }

    private List<FailId> PickFailItems(IncidentDefinitionVM incident)
    {
      FailGroup rootGroup = incident.IncidentDefinition.FailGroup;
      List<FailId> ret = PickFailItems(rootGroup);
      return ret;
    }

    private List<FailId> PickFailItems(FailGroup root)
    {
      //TOTO this is not correct as multiple nested gorups with combination of all/one will not be selected correctly
      List<FailId> ret;
      switch (root.Selection)
      {
        case FailGroup.ESelection.None:
          ret = new List<FailId>();
          break;
        case FailGroup.ESelection.All:
          ret = FlattenFailGroup(root);
          break;
        case FailGroup.ESelection.One:
          Fail tmp = PickRandomFailItem(root.Items);
          if (tmp is FailGroup fg)
            ret = PickFailItems(fg);
          else if (tmp is FailId f)
          {
            ret = new List<FailId>().With(f);
          }
          else
            throw new NotImplementedException();
          break;
        default:
          throw new NotImplementedException();
      }
      return ret;
    }

    private Fail PickRandomFailItem(List<Fail> items)
    {
      Fail? ret = null;
      var totalWeight = items.Sum(q => q.Weight);
      var randomWeight = random.NextDouble(0, totalWeight);
      foreach (var item in items)
      {
        randomWeight -= item.Weight;
        if (randomWeight < 0)
        {
          ret = item;
          break;
        }
      }
      if (ret == null)
        ret = items.Last();

      return ret;
    }

    private void SimConWrapper_SimConErrorRaised(SimConWrapperSimConException ex)
    {
      //TODO resolve
      throw new ApplicationException("Failed sim-con-wrapper-for-failure.", ex);
    }

    private void SimConWrapper_SimSecondElapsed()
    {
      if (isRunning)
      {
        StateCheckEvaluator.UpdateDictionaryByObject(this.simConWrapper.SimData, propertyValues);
        DateTime now = DateTime.Now;
        propertyValues["realTimeSecond"] = now.Second;
        propertyValues["realTimeMinute"] = now.Minute;
        propertyValues["realTimeMinuteLastDigit"] = now.Minute % 10;
        EvaluateAndFireFailures();
      }
    }

    internal void FireIncidentDefinition(IncidentDefinitionVM runIncidentDefinition)
    {
      var tmp = PickFailItems(runIncidentDefinition);
      var lst = tmp
        .Select(q => this.FailureDefinitions.First(p => q.Id == p.Id))
        .ToList();
      StartFailures(lst);
    }

    internal void FireFail(FailId f)
    {
      FailureDefinition fd = this.FailureDefinitions.First(q => q.Id == f.Id);
      List<FailureDefinition> fds = new()
      {
        fd
      };
      StartFailures(fds);
    }

    internal void CancelFailure(FailureSustainer fs)
    {
      fs.Reset();
      this.Sustainers.Remove(fs);
    }

    #endregion Methods
  }
}
