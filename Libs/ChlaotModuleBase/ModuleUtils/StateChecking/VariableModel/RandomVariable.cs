using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking.VariableModel
{
  public class RandomVariable : Variable
  {
    private static readonly Random rnd = new();
    public double Minimum { get; set; }
    public double Maximum { get; set; }

    public bool IsInteger { get; set; } = false;

    public override double Value
    {
      get
      {
        double? ret = base.GetProperty<double?>(nameof(Value));
        if (ret == null)
        {
          ret = Minimum + rnd.NextDouble() * (Maximum - Minimum);
          if (IsInteger)
            ret = Math.Round(ret.Value);
          base.UpdateProperty(nameof(Value), ret, true);
        }
        return ret.Value;
      }
    }
  }
}
