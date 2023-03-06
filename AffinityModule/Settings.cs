using ChlaotModuleBase;
using ChlaotModuleBase.ModuleUtils.Storable;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.Modules.AffinityModule
{
  public class Settings : StorableObject
  {
    public int RefreshIntervalInSeconds
    {
      get => base.GetProperty<int>(nameof(RefreshIntervalInSeconds))!;
      set => base.UpdateProperty(nameof(RefreshIntervalInSeconds), value);
    }

    public Settings()
    {
      this.RefreshIntervalInSeconds = 60;
    }
  }
}
