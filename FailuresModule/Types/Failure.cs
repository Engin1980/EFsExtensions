using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FailuresModule.Types
{
  public abstract class Failure
  {
    public SimConPoint SimConPoint { get; set; }
  }

  public class StuckFailure : Failure
  {

  }
  public class InstantFailure : Failure
  {

  }

  public class LeakFailure : Failure
  {

  }

  public class MultiFailure : Failure
  {
    public List<Failure> Failures { get; } = new List<Failure>();
  }
}
