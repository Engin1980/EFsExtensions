namespace Eng.EFsExtensions.Modules.RaaSModule.Model
{
  public class RaasSpeeches
  {
    public RaasSpeech TaxiToRunway { get; set; } = null!;
    public RaasSpeech TaxiToShortRunway { get; set; } = null!;
    public RaasSpeech OnRunway { get; set; } = null!;
    public RaasSpeech OnShortRunway { get; set; } = null!;
    public RaasSpeech LandingRunway { get; set; } = null!;
    public RaasSpeech DistanceRemaining { get; set; } = null!;
  }
}
