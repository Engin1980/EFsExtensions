using ELogging;
using Eng.Chlaot.ChlaotModuleBase;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateCheckingSimConnection;
using FailuresModule.Model.App;
using FailuresModule.Model.Sim;
using FailuresModule.Types.Run;
using FailuresModule.Types.Run.Sustainers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;

namespace FailuresModule
{
  public class RunContext : NotifyPropertyChangedBase
  {
    private readonly Random random = new();
    private readonly SimConManagerWrapper simConWrapper;
    private List<RunIncidentDefinition>? _IncidentDefinitions = null;

    public List<FailureDefinition> FailureDefinitions { get; }
    public List<RunIncident> Incidents { get; }
    public SimData SimData { get => this.simConWrapper.SimData; }
    public List<FailureSustainer> Sustainers { get; }
    internal List<RunIncidentDefinition> IncidentDefinitions
    {
      get
      {
        if (_IncidentDefinitions == null)
        {
          _IncidentDefinitions = CalculateFlatIncidentDefinitions(Incidents);
        }
        return _IncidentDefinitions;
      }
    }
    public RunContext(List<FailureDefinition> failureDefinitions, List<RunIncident> incidents)
    {
      FailureDefinitions = failureDefinitions;
      Incidents = incidents;
      Sustainers = failureDefinitions.Select(q => FailureSustainerFactory.Create(q)).ToList();

      simConWrapper = new();
      simConWrapper.SimErrorRaised += SimConWrapper_SimErrorRaised;
      Logger.RegisterSender(simConWrapper, Logger.GetSenderName(this) + ".SimConWrapper");
    }

    public static RunContext Create(List<FailureDefinition> failureDefinitions, IncidentTopGroup failureSet)
    {
      IncidentGroup ig = new()
      {
        Incidents = failureSet.Incidents
      };
      RunIncidentGroup top = RunIncidentGroup.Create(ig);

      RunContext ret = new(failureDefinitions, top.Incidents);
      return ret;
    }

    public void Start()
    {
      this.simConWrapper.StartAsync();
    }

    internal void Init()
    {
      //throw new NotImplementedException();
    }

    private List<RunIncidentDefinition> CalculateFlatIncidentDefinitions(List<RunIncident> incidents)
    {
      List<RunIncidentDefinition> ret = new();

      foreach (var incident in incidents)
      {
        if (incident is RunIncidentGroup rig)
        {
          var tmp = CalculateFlatIncidentDefinitions(rig.Incidents);
          ret.AddRange(tmp);
        }
        else if (incident is RunIncidentDefinition rid)
        {
          ret.Add(rid);
        }
      }

      return ret;
    }

    private void EvaluateAndFireFailures()
    {
      foreach (var incident in this.IncidentDefinitions)
      {
        EvaluateIncidentDefinition(incident, out bool isActivated);
        if (!isActivated) continue;

        List<FailId> failItems = PickFailItems(incident);
        List<FailureDefinition> failDefs = failItems.Select(q => this.FailureDefinitions.First(p => q.Id == p.Id)).ToList();
        InitializeFailures(failDefs);
      }
    }

    private void EvaluateIncidentDefinition(RunIncidentDefinition incident, out bool isActivated)
    {
      isActivated = false;
      foreach (var trigger in incident.IncidentDefinition.Triggers)
      {
        if (incident.OneShotTriggersInvoked.Contains(trigger)) continue;
        if (trigger.Repetitive == false)
          incident.OneShotTriggersInvoked.Add(trigger);

        bool isConditionTrue = IsTriggerConditionTrue(trigger.Condition);
        if (isConditionTrue)
        {
          double prob = random.NextDouble();
          isActivated = prob <= trigger.Probability;
        }
      }
    }

    private List<FailId> FlatterFailGroup(Fail failItem)
    {
      void DoFlattening(Fail fi, List<FailId> lst)
      {
        if (fi is Fail fg)
          DoFlattening(fg, lst);
        else if (fi is FailId f)
          lst.Add(f);
        else
          throw new NotImplementedException();
      }
      List<FailId> ret = new();
      DoFlattening(failItem, ret);
      return ret;
    }

    private void InitializeFailures(List<FailureDefinition> failures)
    {
      foreach (var failure in failures)
      {
        if (this.Sustainers.Any(q => q.Failure == failure)) continue;
        FailureSustainer fs = FailureSustainerFactory.Create(failure);
        this.Sustainers.Add(fs);
      }
    }

    private bool IsTriggerConditionTrue(IStateCheckItem condition)
    {
      StateCheckEvaluator sce = new StateCheckEvaluator(this.SimData);
      bool ret = sce.Evaluate(condition);
      return ret;
    }

    private List<FailId> PickFailItems(RunIncidentDefinition incident)
    {
      FailGroup rootGroup = incident.IncidentDefinition.FailGroup;
      List<FailId> ret = PickFailItems(rootGroup);
      return ret;
    }

    private List<FailId> PickFailItems(FailGroup root)
    {
      List<FailId> ret;
      switch (root.Selection)
      {
        case FailGroup.ESelection.None:
          ret = new List<FailId>();
          break;
        case FailGroup.ESelection.All:
          ret = FlatterFailGroup(root);
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

    private void SimConWrapper_SimErrorRaised(Exception ex)
    {
      if (this.simConWrapper.IsRunning)
        this.simConWrapper.StopAsync();

      //TODO resolve
      if (ex is SimConManagerWrapper.StartFailedException sfe)
      {
        throw new ApplicationException("Failed to start sim readout.", sfe);
      }
    }
  }
}
