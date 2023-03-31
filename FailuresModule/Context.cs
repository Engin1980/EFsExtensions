using FailuresModule.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FailuresModule
{
  public class Context
  {
    public List<Failure> Failures { get; set; }

    internal void BuildFailures()
    {
      this.Failures = FailureFactory.BuildFailures();
    }
  }
}
