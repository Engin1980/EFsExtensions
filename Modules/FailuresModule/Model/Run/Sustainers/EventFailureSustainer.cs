using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateCheckingSimConnection;
using ESimConnect;
using FailuresModule.Model.Sim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace FailuresModule.Types.Run.Sustainers
{
  internal class EventFailureSustainer : FailureSustainer
  {
    public EventFailureSustainer(EventFailureDefinition failure) : base(failure)
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
      string @event = this.Failure.SimConPoint.SimPointName;
      uint arg = 0;
      base.SimCon.SendClientEvent(@event, new uint[] { arg }, true);
    }
  }
}
