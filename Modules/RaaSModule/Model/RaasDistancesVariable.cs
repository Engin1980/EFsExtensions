using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.RaaSModule.Model
{
  public class RaasDistancesVariable : RaasVariable
  {
    public List<RaasDistance> Default { get; set; } = null!;
    public List<RaasDistance> Value => Default;

    internal override void CheckSanity()
    {
      base.CheckSanity();

      if (Default == null || !Default.Any())
        throw new ApplicationException("No distances.");
      foreach (var d in this.Default)
      {
        d.CheckSanity();
      }
    }
  }
}
