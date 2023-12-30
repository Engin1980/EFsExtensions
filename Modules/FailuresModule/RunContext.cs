using ChlaotModuleBase.ModuleUtils.SimConWrapping.Exceptions;
using ELogging;
using Eng.Chlaot.ChlaotModuleBase;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.SimConWrapping;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking;
using FailuresModule.Model.App;
using FailuresModule.Model.Sim;
using FailuresModule.Types.Run;
using FailuresModule.Types.Run.Sustainers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace FailuresModule
{
  public class RunContext : NotifyPropertyChangedBase
  {
    #region Fields

    private readonly Random random = new();
    private readonly SimConWrapperWithSimData simConWrapper;
    private List<RunIncidentDefinition>? _IncidentDefinitions = null;
    private readonly Dictionary<string, double> propertyValues = new();
    private bool isRunning = false;

    #endregion Fields

    #region Properties

    public List<FailureDefinition> FailureDefinitions { get; }
    public List<RunIncident> Incidents { get; }
    public BindingList<FailureSustainer> Sustainers { get; }

    public int SustainersCount
    {
      get => base.GetProperty<int>(nameof(SustainersCount))!;
      set => base.UpdateProperty(nameof(SustainersCount), value);
    }
    internal List<RunIncidentDefinition> IncidentDefinitions
    {
      get
      {
        if (_IncidentDefinitions == null)
        {
          _IncidentDefinitions = FlattenIncidentDefinitions(Incidents);
        }
        return _IncidentDefinitions;
      }
    }

    #endregion Properties

    #region Constructors

    public RunContext(List<FailureDefinition> failureDefinitions, List<RunIncident> incidents)
    {
      ESimConnect.ESimConnect eSimCon = new();
      simConWrapper = new(eSimCon);
      simConWrapper.SimSecondElapsed += SimConWrapper_SimSecondElapsed;
      simConWrapper.SimConErrorRaised += SimConWrapper_SimConErrorRaised;

      FailureSustainer.SetSimCon(eSimCon);
      FailureDefinitions = failureDefinitions;
      Incidents = incidents;
      Sustainers = new();
      Sustainers.ListChanged += (s, e) => this.SustainersCount = Sustainers.Count;
    }

    #endregion Constructors

    #region Methods

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
      this.simConWrapper.OpenAsync(
        () =>
        {
          SetVariableValues();
          this.simConWrapper.Start();
          this.isRunning = true;
        },
        ex => { });
    }

    private void SetVariableValues()
    {
      foreach (var runIncidentDefinition in this.IncidentDefinitions)
      {
        Dictionary<string, double> tmp = runIncidentDefinition.IncidentDefinition.Variables
          .ToDictionary(
          k => k.Name,
          v => v.Value);
        StateCheckEvaluator sce = new(tmp, propertyValues);
        incidentEvaluators[runIncidentDefinition] = sce;
      }
    }

    private static List<RunIncidentDefinition> FlattenIncidentDefinitions(List<RunIncident> incidents)
    {
      List<RunIncidentDefinition> ret = new();

      foreach (var incident in incidents)
      {
        if (incident is RunIncidentGroup rig)
        {
          var tmp = FlattenIncidentDefinitions(rig.Incidents);
          ret.AddRange(tmp);
        }
        else if (incident is RunIncidentDefinition rid)
        {
          ret.Add(rid);
        }
      }

      return ret;
    }

    private readonly Dictionary<RunIncidentDefinition, StateCheckEvaluator> incidentEvaluators = new();
    private void EvaluateAndFireFailures()
    {
      foreach (var runIncidentDefinition in this.IncidentDefinitions)
      {
        EvaluateIncidentDefinition(runIncidentDefinition, out bool isActivated);
        if (!isActivated) continue;

        List<FailId> failItems = PickFailItems(runIncidentDefinition);
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

        StateCheckEvaluator sce = incidentEvaluators[incident];

        bool isConditionTrue = IsTriggerConditionTrue(sce, trigger.Condition);
        if (isConditionTrue)
        {
          if (trigger.Repetitive == false)
            incident.OneShotTriggersInvoked.Add(trigger);

          double prob = random.NextDouble();
          isActivated = prob <= trigger.Probability;
          if (isActivated) return;
        }
      }
    }

    private static List<FailId> FlattenFailGroup(Fail failItem)
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
        fs.Start();
      }
    }

    private bool IsTriggerConditionTrue(StateCheckEvaluator stateCheckEvaluator, IStateCheckItem condition)
    {
      bool ret = stateCheckEvaluator.Evaluate(condition);
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

    #endregion Methods
  }
}
