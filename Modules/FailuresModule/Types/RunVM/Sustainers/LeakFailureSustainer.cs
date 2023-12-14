using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FailuresModule.Types.RunVM.Sustainers
{
  internal class LeakFailureSustainer : FailureSustainer
  {
    public LeakFailureSustainer(FailureDefinition failure) : base(failure)
    {
    }

    protected override void InitInternal()
    {
      throw new NotImplementedException();
    }
  }
}
