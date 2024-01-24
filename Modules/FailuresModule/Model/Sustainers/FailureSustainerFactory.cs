using Eng.Chlaot.Modules.FailuresModule.Model.Failures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.Modules.FailuresModule.Model.Sustainers
{
    internal class FailureSustainerFactory
    {
        internal static FailureSustainer Create(FailureDefinition failItem)
        {
            FailureSustainer ret;
            if (failItem is ToggleFailureDefinition efd)
                ret = CreateToggle(efd);
            else if (failItem is SetFailureDefinition svfd)
                ret = CreateSet(svfd);
            else if (failItem is StuckFailureDefinition sfd)
                ret = CreateStuck(sfd);
            else if (failItem is LeakFailureDefinition lfd)
                ret = CreateLeak(lfd);
            else if (failItem is SneakFailureDefinition nfd)
                ret = CreateSneak(nfd);
            else if (failItem is ToggleOnVarMismatchFailureDefinition svvefd)
                ret = CreateToggleOnVarMismatch(svvefd);
            else
                throw new NotImplementedException();
            return ret;
        }

        private static FailureSustainer CreateToggleOnVarMismatch(ToggleOnVarMismatchFailureDefinition svvefd)
        {
            ToggleOnVarMismatchFailureSustainer ret = new(svvefd);
            return ret;
        }

        private static FailureSustainer CreateSneak(SneakFailureDefinition nfd)
        {
            SneakFailureSustainer ret = new SneakFailureSustainer(nfd);
            return ret;
        }

        private static FailureSustainer CreateSet(SetFailureDefinition svfd)
        {
            SetFailureSustainer ret = new SetFailureSustainer(svfd);
            return ret;
        }

        private static FailureSustainer CreateLeak(LeakFailureDefinition lfd)
        {
            LeakFailureSustainer ret = new LeakFailureSustainer(lfd);
            return ret;
        }

        private static FailureSustainer CreateStuck(StuckFailureDefinition sfd)
        {
            StuckFailureSustainer ret = new StuckFailureSustainer(sfd);
            return ret;
        }

        private static FailureSustainer CreateToggle(ToggleFailureDefinition ifd)
        {
            ToggleFailureSustainer ret = new ToggleFailureSustainer(ifd);
            return ret;
        }
    }
}
