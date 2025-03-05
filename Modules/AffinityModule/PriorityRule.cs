using AffinityModule;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.AffinityModule
{
  public class PriorityRule : AbstractRule
  {
    public ProcessPriorityClass Priority { get; set; } = ProcessPriorityClass.Normal;
  }
}
