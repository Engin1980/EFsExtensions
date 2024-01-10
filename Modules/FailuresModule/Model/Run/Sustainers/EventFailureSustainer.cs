using ESimConnect;
using FailuresModule.Model.Sim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace FailuresModule.Model.Run.Sustainers
{
  internal class EventFailureSustainer : FailureSustainer
  {
    public EventFailureSustainer(SimEventFailureDefinition failure) : base(failure)
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
