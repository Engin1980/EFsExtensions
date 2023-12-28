namespace FailuresModule.Model.Sim
{
  public class SimVarFailureDefinition : FailureDefinition
  {
    public SimVarFailureDefinition(string id, string title, SimConPoint simConPoint) : base(id, title, simConPoint)
    {
    }

    public override string Type => "SimVar";
  }
}
