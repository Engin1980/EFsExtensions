namespace FailuresModule.Model.Sim
{
  public class SimEventFailureDefinition : FailureDefinition
  {
    public SimEventFailureDefinition(string id, string title, string simConPoint) : base(id, title, simConPoint)
    {
    }

    public override string Type => "Event";
  }
}
