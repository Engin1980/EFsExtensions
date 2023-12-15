namespace FailuresModule.Model.Sim
{
  //TODO remove types and replace by enum if not used in more complex way
  public class StuckFailureDefinition : FailureDefinition
  {
    public StuckFailureDefinition(string id, string title, SimConPoint simConPoint) : base(id, title, simConPoint)
    {
    }
  }
}
