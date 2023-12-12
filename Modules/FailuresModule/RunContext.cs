using ELogging;
using Eng.Chlaot.ChlaotModuleBase;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateCheckingSimConnection;
using FailuresModule.Types;
using FailuresModule.Types.RunVM;
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
    private readonly SimConManagerWrapper simConWrapper;
    private readonly Random random = new Random();
    public List<FailureDefinition> FailureDefinitions { get; }
    public List<RunIncident> Incidents { get; }
    private List<RunIncidentDefinition>? _IncidentDefinitions = null;
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

    public SimData SimData => this.simConWrapper.SimData;

    public RunContext(List<FailureDefinition> failureDefinitions, List<RunIncident> incidents)
    {
      FailureDefinitions = failureDefinitions;
      Incidents = incidents;

      simConWrapper = new();
      simConWrapper.SimSecondElapsed += SimConWrapper_SimSecondElapsed;
      simConWrapper.SimErrorRaised += SimConWrapper_SimErrorRaised;
      Logger.RegisterSender(simConWrapper, Logger.GetSenderName(this) + ".SimConWrapper");
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

    private void SimConWrapper_SimSecondElapsed()
    {
      EvaluateAndFireFailures();
    }

    public static RunContext Create(List<FailureDefinition> failureDefinitions, FailureSet failureSet)
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


    private void EvaluateAndFireFailures()
    {
      foreach (var incident in this.IncidentDefinitions)
      {
        EvaluateIncidentDefinition(incident, out bool isActivated);
        if (!isActivated) continue;

        List<Failure> failItems = PickFailItems(incident);

        tady dopsat jak to failne
      }
    }

    private List<Failure> PickFailItems(RunIncidentDefinition incident)
    {
      FailGroup rootGroup = incident.IncidentDefinition.FailGroup;
      List<Failure> ret = PickFailItems(rootGroup);
      return ret;
    }

    private List<Failure> PickFailItems(FailGroup root)
    {
      List<Failure> ret;
      switch (root.Selection)
      {
        case FailGroup.ESelection.None:
          ret = new List<Failure>();
          break;
        case FailGroup.ESelection.All:
          ret = FlatterFailGroup(root);
          break;
        case FailGroup.ESelection.One:
          FailItem tmp = PickRandomFailItem(root.Items);
          if (tmp is FailGroup fg)
            ret = PickFailItems(fg);
          else if (tmp is Failure f)
          {
            ret = new List<Failure>().With(f);
          }
          else
            throw new NotImplementedException();
          break;
        default:
          throw new NotImplementedException();
      }
      return ret;
    }

    private List<Failure> FlatterFailGroup(FailItem failItem)
    {
      void DoFlattening(FailItem fi, List<Failure> lst)
      {
        if (fi is FailGroup fg)
          DoFlattening(fg, lst);
        else if (fi is Failure f)
          lst.Add(f);
        else
          throw new NotImplementedException();
      }
      List<Failure> ret = new();
      DoFlattening(failItem, ret);
      return ret;
    }

    private FailItem PickRandomFailItem(List<FailItem> items)
    {
      FailItem? ret = null;
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

    private bool IsTriggerConditionTrue(IStateCheckItem condition)
    {
      StateCheckEvaluator sce = new StateCheckEvaluator(this.SimData);
      bool ret = sce.Evaluate(condition);
      return ret;
    }
  }
}
