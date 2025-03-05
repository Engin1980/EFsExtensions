using Eng.EFsExtensions.EFsExtensionsModuleBase;
using ESystem.Miscelaneous;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.FailuresModule.Model.Incidents
{
  public abstract class Trigger : NotifyPropertyChanged
  {

    public Percentage Probability
    {
      get => base.GetProperty<Percentage>(nameof(Probability))!;
      set => base.UpdateProperty(nameof(Probability), value);
    }


    public bool Repetitive
    {
      get => base.GetProperty<bool>(nameof(Repetitive))!;
      set => base.UpdateProperty(nameof(Repetitive), value);
    }
  }
}
