
namespace Eng.Chlaot.Modules.RaaSModule.Model
{
  public class RaasVariables
  {
    public RaasDistanceVariable MinimalTakeOffDistance { get; set; }
    public RaasDistanceVariable MinimalLandingDistance { get; set; }

    internal void CheckSanity()
    {
      if (MinimalTakeOffDistance == null) throw new ApplicationException("RaasVariables.MinimalTakeOffDistance is null"); 
      if (MinimalLandingDistance == null) throw new ApplicationException("RaasVariables.MinimalLandingDistance is null");
      MinimalLandingDistance.CheckSanity();
      MinimalLandingDistance.CheckSanity();
    }
  }
}
