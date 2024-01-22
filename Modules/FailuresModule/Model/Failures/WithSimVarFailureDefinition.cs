using ESystem.Asserting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace Eng.Chlaot.Modules.FailuresModule.Model.Failures
{
  public abstract class WithSimVarFailureDefinition : FailureDefinition
  {
    public string SimVar { get; set; } = null!;
    public override string SimConPoint => SimVar;

    public override void PostDeserialize()
    {
      this.Title ??= SimVar;
      base.PostDeserialize();
      EAssert.IsNonEmptyString(SimVar, $"{nameof(SimVar)} is empty or null");
    }

    internal override void ExpandVariableIfExists(string varRef, int variableValue)
    {      
      base.ExpandVariableIfExists(varRef, variableValue);
      SimVar = FailureDefinition.ExpandVariableInString(this.SimVar, varRef, variableValue);
    }
  }
}
