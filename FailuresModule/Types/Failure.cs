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
    public Failure(string id, string title, SimConPoint simConPoint)
    {
      this.Id = id ?? throw new ArgumentNullException(nameof(id));
      this.Title = title ?? throw new ArgumentNullException(nameof(title));
      this.SimConPoint = simConPoint ?? throw new ArgumentNullException(nameof(simConPoint));
    }

    public string Id { get; }
    public string Title { get; }
    public SimConPoint SimConPoint { get; set; }

    public string TypeName => this.GetType().Name;
  }

  public class StuckFailure : Failure
  {
    public StuckFailure(string id, string title, SimConPoint simConPoint) : base(id, title, simConPoint)
    {
    }
  }

  public class InstantFailure : Failure
  {
    public InstantFailure(string id, string title, SimConPoint simConPoint) : base(id, title, simConPoint)
    {
    }
  }

  public class LeakFailure : Failure
  {
    public LeakFailure(string id, string title, SimConPoint simConPoint) : base(id, title, simConPoint)
    {
    }
  }
}
