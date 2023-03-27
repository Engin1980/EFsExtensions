using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FailuresModule.Types
{
  public class SimConPoint
  {
  }

  public class EventSimConPoint : SimConPoint
  {
    public string SimEvent { get; }

    public EventSimConPoint(string simEvent)
    {
      this.SimEvent = simEvent;
    }
  }

  public class VarSimConPoint : SimConPoint
  {
    public string SimVar { get; }

    public VarSimConPoint(string simVar)
    {
      SimVar = simVar;
    }
  }
}
