using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FailuresModule.Types
{
  public abstract class SimConPoint
  {
    public abstract string SimPointName { get; }
  }

  public class EventSimConPoint : SimConPoint
  {
    public string SimEvent { get; }
    public override string SimPointName => this.SimEvent;

    public EventSimConPoint(string simEvent)
    {
      this.SimEvent = simEvent;
    }
  }

  public class VarSimConPoint : SimConPoint
  {
    public string SimVar { get; }
    public override string SimPointName => this.SimVar;

    public VarSimConPoint(string simVar)
    {
      SimVar = simVar;
    }
  }
}
