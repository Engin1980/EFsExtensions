using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace FailuresModule.Types
{
  public abstract class Failure
  {
    public Failure(string title)
    {
      this.Title = title ?? throw new ArgumentNullException(nameof(title));
    }

    public string Title { get; set; }
    public string GroupId { get; set; } = string.Empty;
  }

  public abstract class InvokableFailure : Failure
  {
    protected InvokableFailure(string title, SimConPoint simConPoint) : base(title)
    {
      this.SimConPoint = simConPoint ?? throw new ArgumentNullException(nameof(simConPoint));
    }

    public SimConPoint SimConPoint { get; set; }
  }

  public class StuckFailure : InvokableFailure
  {
    public StuckFailure(string title, SimConPoint simConPoint) : base(title, simConPoint)
    {
    }
  }
  public class InstantFailure : InvokableFailure
  {
    public InstantFailure(string title, SimConPoint simConPoint) : base(title, simConPoint)
    {
    }
  }

  public class LeakFailure : InvokableFailure
  {
    public LeakFailure(string title, SimConPoint simConPoint) : base(title, simConPoint)
    {
    }
  }

  public class MultiFailure : Failure
  {
    public MultiFailure(string title) : base(title)
    {
    }

    public List<Failure> Failures { get; } = new List<Failure>();
  }

  public class OneOfFailure : MultiFailure
  {
    public OneOfFailure(string title) : base(title)
    {
    }
  }

  public class AnyOfFailure : MultiFailure
  {
    public AnyOfFailure(string title) : base(title)
    {
    }
  }
}
