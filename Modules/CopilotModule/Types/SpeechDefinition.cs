using ChlaotModuleBase.ModuleUtils.StateChecking;
using Eng.Chlaot.ChlaotModuleBase;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking.StateModel;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking.VariableModel;
using ESystem.Asserting;
using EXmlLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.Modules.CopilotModule.Types
{
  public class SpeechDefinition : IXmlObjectPostDeserialize
  {
    public string Title { get; set; } = null!;
    public Speech Speech { get; set; } = null!;
    public IStateCheckItem Trigger { get; set; } = null!;
    public IStateCheckItem? ReactivationTrigger { get; set; } = null;
    public List<Variable> Variables { get; set; } = new();

    /*
     * This is here because I need those values to be as collections for WPF TreeView
     */
    public Collection<IStateCheckItem> __TriggerCollection => CreateCollectionWith(this.Trigger);
    public Collection<IStateCheckItem> __ReactivationTriggerCollection => CreateCollectionWith(this.ReactivationTrigger);

    public void PostDeserialize()
    {
      FillVariablesWithUndeclaredOnes();
      EAssert.IsNonEmptyString(Title);
      EAssert.IsNotNull(Speech);
      EAssert.IsNotNull(Trigger);
      EAssert.IsNotNull(Variables);
    }

    public void FillVariablesWithUndeclaredOnes()
    {
      this.Speech
        .GetUsedVariables()
        .Except(this.Variables.Select(q => q.Name))
        .Select(q => new UserVariable()
        {
          Name = q,
          DefaultValue = null
        })
        .ForEach(q => this.Variables.Add(q));

      this.ExtractVariablePairsFromStateChecks()
        .Select(q => q.VariableName)
        .Except(this.Variables.Select(q => q.Name))
        .Select(q => new UserVariable()
        {
          Name = q,
          DefaultValue = null
        })
        .ForEach(q => this.Variables.Add(q));
    }

    private List<StateCheckUtils.VariableUsage> ExtractVariablePairsFromStateChecks()
    {
      List<StateCheckUtils.VariableUsage> ret = StateCheckUtils.ExtractVariablesFromProperties(this.Trigger);
      if (this.ReactivationTrigger != null)
      {
        var tmp = StateCheckUtils.ExtractVariablesFromProperties(this.ReactivationTrigger);
        ret.AddRange(tmp);
      }
      return ret;
    }

    private Collection<IStateCheckItem> CreateCollectionWith(IStateCheckItem item)
    {
      Collection<IStateCheckItem> ret = new()
      {
        item
      };
      return ret;
    }
  }
}
