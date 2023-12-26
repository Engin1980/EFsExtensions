using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking
{
  public class StateCheckDelay : IStateCheckItem
  {
    public int Seconds { get; set; }
    public IStateCheckItem Item { get; set; } = null!;
    public string DisplayString => $"(delay={Seconds} {Item.DisplayString})";
  }
}
