using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.SimConWrapping.Exceptions
{
  public class SimConWrapperSimConException : Exception
  {
    public SimConWrapperSimConException(string? FS2020SimConErrorTextCode) : base($"SimCon returned exception code '{FS2020SimConErrorTextCode}'.")
    {
    }
  }
}
