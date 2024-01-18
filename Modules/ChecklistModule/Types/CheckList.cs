using ChlaotModuleBase.ModuleUtils.StateChecking;
using Eng.Chlaot.ChlaotModuleBase;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking.VariableModel;
using ESystem.Asserting;
using EXmlLib.Attributes;
using EXmlLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Eng.Chlaot.Modules.ChecklistModule.Types
{
  public class CheckList : IXmlObjectPostDeserialize
  {
    public string Id { get; set; } = null!;
    public string CallSpeech { get; set; } = null!;
    public List<CheckItem> Items { get; set; } = null!;
    public string NextChecklistIds { get; set; } = null!;
    public List<CheckList> NextChecklists { get; set; } = null!;
    public byte[] EntrySpeechBytes { get; set; } = null!; //xml-ignored
    public byte[] ExitSpeechBytes { get; set; } = null!; // xml-ignored
    public List<Variable> Variables { get; set; } = new();
    public IStateCheckItem? Trigger { get; set; } = null;
    public CheckDefinition CustomEntrySpeech { get; set; } = null!;
    public CheckDefinition CustomExitSpeech { get; set; } = null!;

    public void PostDeserialize()
    {
      FillVariablesWithUndeclaredOnes();
      EAssert.IsNonEmptyString(Id, $"{nameof(Id)} is empty string.");
      EAssert.IsNonEmptyString(CallSpeech, $"{nameof(CallSpeech)} is empty string.");
      EAssert.IsTrue(string.IsNullOrEmpty(this.NextChecklistIds) || Regex.IsMatch(this.NextChecklistIds, @"\S+(;\S+)*")); // sequence of ids delimited by semicolon
      EAssert.IsNotNull(Variables);
    }

    public void FillVariablesWithUndeclaredOnes()
    {
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
      List<StateCheckUtils.VariableUsage> ret = new();
      if (this.Trigger != null)
      {
        var tmp = StateCheckUtils.ExtractVariables(this.Trigger);
        ret.AddRange(tmp);
      }
      return ret;
    }

    public override string ToString() => $"{this.CallSpeech} ({this.Id}) {{checklist}}";
  }
}
