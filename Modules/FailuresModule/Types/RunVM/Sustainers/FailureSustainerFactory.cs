using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FailuresModule.Types.RunVM.Sustainers
{
  internal class FailureSustainerFactory
  {
    internal static FailureSustainer Create(FailureDefinition failItem)
    {
      FailureSustainer ret;
      if (failItem is InstantFailureDefinition ifd)
        ret = CreateInstant(ifd);
      else if (failItem is StuckFailureDefinition sfd)
        ret = CreateStuck(sfd);
      else if (failItem is Types.LeakFailureSustainer lfd)
        ret = CreateLeak(lfd);
      else
        throw new NotImplementedException();
      return ret;
    }

    private static FailureSustainer CreateLeak(Types.LeakFailureSustainer lfd)
    {
      LeakFailureSustainer ret = new LeakFailureSustainer(lfd);
      return ret;
    }

    private static FailureSustainer CreateStuck(StuckFailureDefinition sfd)
    {
      StuckFailureSustainer ret = new StuckFailureSustainer(sfd);
      return ret;
    }

    private static FailureSustainer CreateInstant(InstantFailureDefinition ifd)
    {
      InstantFailureSustainer ret = new InstantFailureSustainer(ifd);
      return ret;
    }
  }
}
