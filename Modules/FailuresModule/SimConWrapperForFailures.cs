using ELogging;
using Eng.Chlaot.ChlaotModuleBase;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.SimConWrapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Eng.Chlaot.Modules.FailuresModule
{
  public class SimConWrapperForFailures : SimConWrapperWithSimSecond
  {
    public SimConWrapperForFailures(ESimConnect.ESimConnect simConnect) : base(simConnect)
    {
    }

    private void Log(LogLevel level, string message)
    {
      Logger.Log(this, level, message);
    }
  }
}
