using Eng.Chlaot.ChlaotModuleBase;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.Modules.FailuresModule.Model.Incidents
{
  public abstract class Trigger : NotifyPropertyChangedBase
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
