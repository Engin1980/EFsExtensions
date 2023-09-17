using Eng.Chlaot.ChlaotModuleBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FailuresModule.Types
{
  public class Failure : NotifyPropertyChangedBase
  {

    public FailureDefinition Definition
    {
      get => base.GetProperty<FailureDefinition>(nameof(Definition))!;
      set => base.UpdateProperty(nameof(Definition), value);
    }


    public FailureFrequency Frequency
    {
      get => base.GetProperty<FailureFrequency>(nameof(Frequency))!;
      set => base.UpdateProperty(nameof(Frequency), value);
    }
  }
}
