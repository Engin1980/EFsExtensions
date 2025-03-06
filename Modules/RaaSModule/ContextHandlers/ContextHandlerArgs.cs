using ELogging;
using Eng.EFsExtensions.Modules.RaaSModule.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Eng.EFsExtensions.Modules.RaaSModule.Context;

namespace Eng.EFsExtensions.Modules.RaaSModule.ContextHandlers
{
  class ContextHandlerArgs
  {
    public readonly Logger logger;
    public readonly RuntimeDataBox data;
    public readonly Raas raas;
    public readonly SimDataSnaphotProvider simData;
    public readonly Settings settings;

    public ContextHandlerArgs(Logger logger, 
      RuntimeDataBox data, 
      Raas raas, 
      SimDataSnaphotProvider simDataSnapshotProvider, 
      Settings settings)
    {
      this.logger = logger;
      this.data = data;
      this.raas = raas;
      this.simData = simDataSnapshotProvider;
      this.settings = settings;
    }
  }
}
