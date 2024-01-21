using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking.VariableModel
{
  public class RandomVariable : Variable
  {
    public double Minimum { get; set; }
    public double Maximum { get; set; }
    public bool IsInteger { get; set; } = false;
  }
}
