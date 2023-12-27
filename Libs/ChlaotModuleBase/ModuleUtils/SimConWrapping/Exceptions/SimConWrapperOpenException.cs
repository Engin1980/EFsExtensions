using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChlaotModuleBase.ModuleUtils.SimConWrapping.Exceptions
{
  public class SimConWrapperOpenException : Exception
  {
    public SimConWrapperOpenException(Exception innerException) : base("Failed to open connection to sim-con.", innerException) { }
  }
}
