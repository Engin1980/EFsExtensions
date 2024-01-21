using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking.Interfaces
{
  public interface IVariableValuesProvider
  {
    public Dictionary<string, double> GetVariableValues();
  }
}
