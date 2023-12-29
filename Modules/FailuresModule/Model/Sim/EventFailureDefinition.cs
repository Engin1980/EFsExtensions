namespace FailuresModule.Model.Sim
{
  public class EventFailureDefinition : FailureDefinition
  {
    public EventFailureDefinition(string id, string title, string simConPoint) : base(id, title, simConPoint)
    {
    }

    public override string Type => "Event";
  }
}
