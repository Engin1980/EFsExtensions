using ESimConnect;
using FailuresModule.Model.Failures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace FailuresModule.Model.Run.Sustainers
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
      string @event = this.Failure.SimConPoint;
      uint arg = 0;
      base.SimCon.SendClientEvent(@event, new uint[] { }, true);
    }
  }
}
