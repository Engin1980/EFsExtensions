
namespace Eng.Chlaot.Modules.RaaSModule.Model
{
  public class RaasDistanceVariable : RaasVariable
  {
    public RaasDistance Default { get; set; }
    public RaasDistance? Value { get; set; }

    internal override void CheckSanity()
    {
      base.CheckSanity();
      Default.CheckSanity();
    }
  }
}
