namespace Eng.Chlaot.Modules.RaaSModule.Model
{
  public class RaasDistanceSpeech : RaasSpeech
  {
    public RaasDistance Distance { get; set; }

    internal override void CheckSanity()
    {
      base.CheckSanity();
      Distance.CheckSanity();
    }
  }
}
