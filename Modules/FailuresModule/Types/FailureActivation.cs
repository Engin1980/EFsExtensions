using Eng.Chlaot.ChlaotModuleBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FailuresModule.Types
{
  public abstract class FailureFrequency : NotifyPropertyChangedBase
  {
  }

  public class ProbabilityFailureFrequency : FailureFrequency
  {
    public int Probability
    {
      get => base.GetProperty<int>(nameof(Probability))!;
      set => base.UpdateProperty(nameof(Probability), value);
    }
  }

  public class MtbfFailureFrequency : FailureFrequency
  {

    public double MTBF
    {
      get => base.GetProperty<double>(nameof(MTBF))!;
      set
      {
        base.UpdateProperty(nameof(MTBF), value);
        this.CalculatedProbability = 100d / value;
      }
    }


    public double CalculatedProbability
    {
      get => base.GetProperty<double>(nameof(CalculatedProbability))!;
      set => base.UpdateProperty(nameof(CalculatedProbability), value);
    }
  }

}
