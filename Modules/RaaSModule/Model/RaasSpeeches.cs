namespace Eng.Chlaot.Modules.RaaSModule.Model
{
  public class RaasSpeeches
  {
    public RaasDistanceSpeech TaxiToRunway { get; set; }
    public RaasDistanceSpeech TaxiToShortRunway { get; set; }
    public RaasSpeech OnRunway { get; set; }
    public RaasSpeech OnShortRunway { get; set; }
    public RaasDistanceSpeech LandingRunway { get; set; }
    public RaasDistancesSpeech DistanceRemaining { get; set; }
  }
}
