using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChecklistModule.Types.Autostarts
{
  public class AutostartDelay : IAutostart
  {
    public int Seconds { get; set; }
    public IAutostart Item { get; set; }

    public string DisplayString => $"(delay={Seconds} {Item.DisplayString})";
  }
}
