namespace Eng.EFsExtensions.Modules.RaaSModule.Model
{
  public class Variable
  {
    public string Name { get; set; } = null!;
    public RaasDistance Default { get; set; }
    public RaasDistance? Value { get; set; }
  }
}
