using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FailuresModule.Types.RunVM.Sustainers
{
  internal class InstantFailureSustainer : FailureSustainer
  {
    public InstantFailureSustainer(FailureDefinition failure) : base(failure)
    {
    }

    protected override void InitInternal()
    {
      throw new NotImplementedException();
    }
  }
}
