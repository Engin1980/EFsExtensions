namespace Eng.EFsExtensions.Modules.RaaSModule.Model
{
  public class RaasVariable
  {
    public string Name { get; set; } = null!;

    internal virtual void CheckSanity()
    {
      if (string.IsNullOrEmpty(Name)) throw new ApplicationException("RaasVariable.Name is null or empty");
    }
  }
}
