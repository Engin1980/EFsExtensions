
namespace Eng.Chlaot.Modules.RaaSModule.Model
{
  public class RaasVariables
  {
    public RaasDistanceVariable MinimalTakeOffDistance { get; set; } = null!;
    public RaasDistanceVariable MinimalLandingDistance { get; set; } = null!;
    public RaasDistancesVariable AnnouncedRemainingDistances { get; set; } = null!;

    internal void CheckSanity()
    {
      if (MinimalTakeOffDistance == null) throw new ApplicationException("RaasVariables.MinimalTakeOffDistance is null");
      if (MinimalLandingDistance == null) throw new ApplicationException("RaasVariables.MinimalLandingDistance is null");
      if (AnnouncedRemainingDistances == null) throw new ApplicationException("RaasVariables.AnnouncedRemainingDistances is null.");
      MinimalLandingDistance.CheckSanity();
      MinimalLandingDistance.CheckSanity();
      AnnouncedRemainingDistances.CheckSanity();
    }
  }
}
