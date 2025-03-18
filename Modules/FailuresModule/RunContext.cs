using ESystem.Logging;
using Eng.EFsExtensions.EFsExtensionsModuleBase;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.SimConWrapping;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.StateChecking;
using Eng.EFsExtensions.Modules.FailuresModule.Model.Incidents;
using Eng.EFsExtensions.Modules.FailuresModule.Model.Failures;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.WPF.VMs;
using Eng.EFsExtensions.Modules.FailuresModule.Model.VMs;
using Eng.EFsExtensions.Modules.FailuresModule.Model.Sustainers;
using ESystem.Miscelaneous;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.SimObjects;
using Eng.Chloat.Modules.FailuresModule.Model;

namespace Eng.EFsExtensions.Modules.FailuresModule
{
  public class RunContext : NotifyPropertyChanged
  {
    #region Fields

    private readonly Random random = new();
    private readonly NewSimObject eSimObj;
    private List<IncidentDefinitionVM>? _IncidentDefinitions = null;
    private bool isRunning = false;
    private readonly Dictionary<string, double> propertyValues = new();

    #endregion Fields

    #region Properties

    public List<FailureDefinition> FailureDefinitions { get; }
    public List<IncidentVM> IncidentVMs { get; }
    public BindingList<FailureSustainer> Sustainers { get; }


    public bool IsSupressed
    {
      get => base.GetProperty<bool>(nameof(IsSupressed))!;
      set => base.UpdateProperty(nameof(IsSupressed), value);
    }

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


    public FailSimData SimData
    {
      get { return base.GetProperty<FailSimData>(nameof(SimData))!; }
      set { base.UpdateProperty(nameof(SimData), value); }
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

      // creation
      this.eSimObj = NewSimObject.GetInstance();
      this.eSimObj.SimSecondElapsed += ESimCon_SimSecondElapsed;

      FailureSustainer.SetSimCon(this.eSimObj);
      FailureDefinitions = failureDefinitions;
      IncidentVMs = top.Incidents;
      Sustainers = new();
      Sustainers.ListChanged += (s, e) => this.SustainersCount = Sustainers.Count;

      this.IsSupressed = false;
    }

    #endregion Constructors

    #region Methods

    public void Start()
    {
      this.eSimObj.StartInBackground(() =>
      {
        this.eSimObj.ExtType.Register<FailSimData>();
        this.isRunning = true;
      });
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
      if (incident.IsOneShotTriggerInvoked)
      {
        isActivated = false;
        return;
      }

      bool isConditionTrue = incident.Trigger.Evaluate();

      if (isConditionTrue)
      {
        if (incident.Trigger.Trigger.Repetitive == false)
          incident.IsOneShotTriggerInvoked = true;

        double prob = random.NextDouble();
        isActivated = prob <= incident.Trigger.Trigger.Probability;
      }
      else
        isActivated = false;
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

    private void ESimCon_SimSecondElapsed()
    {
      if (isRunning && !IsSupressed)
      {
        this.SimData = eSimObj.ExtType.GetSnapshot<FailSimData>();
        StateCheckEvaluator.UpdateDictionaryByObject(this.SimData, propertyValues);
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
