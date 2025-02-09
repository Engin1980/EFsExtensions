namespace Eng.Chlaot.Modules.RaaSModule.Model
{
  public class Variable
  {
    public string Name { get; set; }
    public RaasDistance Default { get; set; }
    public RaasDistance? Value { get; set; }
  }
}
