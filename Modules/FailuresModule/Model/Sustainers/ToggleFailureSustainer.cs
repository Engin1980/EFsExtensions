using Eng.EFsExtensions.Modules.FailuresModule.Model.Failures;
using ESimConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Eng.EFsExtensions.Modules.FailuresModule.Model.Sustainers
{
  internal class ToggleFailureSustainer : FailureSustainer
  {
    public ToggleFailureSustainer(ToggleFailureDefinition failure) : base(failure)
    {
      // intentionally blank
    }

    protected override void InitInternal()
    {
      // intentionally blank
    }

    protected override void ResetInternal()
    {
      SendEvent();
    }

    protected override void StartInternal()
    {
      SendEvent();
    }

    private void SendEvent()
    {
      string @event = Failure.SimConPoint;
      ESimObj.ESimCon.ClientEvents.Invoke(@event, validate: true);
    }
  }
}
