using ChlaotModuleBase;
using CopilotModule;
using CopilotModule.Types;
using Eng.Chlaot.ChlaotModuleBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.Modules.CopilotModule
{
  public class RunContext
  {
    public CopilotSet Set { get; private set; }

    private Settings settings;
    private LogHandler logHandler;

    public RunContext(InitContext initContext, LogHandler logHandler)
    {
      this.Set = initContext.Set;
      this.settings = initContext.Settings;
      this.logHandler = logHandler;
    }

    private void Log(LogLevel level, string message)
    {
      logHandler?.Invoke(level, "[RunContext] :: " + message);
    }

    internal void Stop()
    {
      throw new NotImplementedException();
    }
  }
}
