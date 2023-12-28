namespace FailuresModule.Model.Sim
{
  public class LeakFailureDefinition : FailureDefinition
  {
    public LeakFailureDefinition(string id, string title, SimConPoint simConPoint) : base(id, title, simConPoint)
    {
    }

    public override string Type => "Leak";
  }
}
