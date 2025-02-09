namespace Eng.Chlaot.Modules.RaaSModule.Model
{
  public class RaasDistancesSpeech : RaasSpeech
  {
    public List<RaasDistance> Distances { get; set; } = null!;

    internal override void CheckSanity()
    {
      base.CheckSanity();
      Distances.ForEach(q => q.CheckSanity());
    }
  }
}
