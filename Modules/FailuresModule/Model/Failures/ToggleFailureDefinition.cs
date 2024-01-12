using ESystem.Asserting;
using EXmlLib.Interfaces;
using System.Windows.Media.Animation;

namespace FailuresModule.Model.Failures
{
  public class ToggleFailureDefinition : FailureDefinition
  {
    public override string Type => "Toggle";
    public string SimEvent { get; set; } = null!;
    public override string SimConPoint => SimEvent;

    public override void PostDeserialize()
    {
      base.PostDeserialize();
      EAssert.IsNonEmptyString(SimEvent);
    }
    internal override void ExpandVariableIfExists(string varRef, int variableValue)
    {
      base.ExpandVariableIfExists(varRef, variableValue);
      this.SimEvent = FailureDefinition.ExpandVariableInString(this.SimEvent, varRef, variableValue);
    }
  }
}
