using ESystem.Logging;
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
    public readonly Func<SimDataSnapshot> simDataSnapshotProvider;
    public readonly Settings settings;

    public ContextHandlerArgs(Logger logger, 
      RuntimeDataBox data, 
      Raas raas,
      Func<SimDataSnapshot> simDataSnapshotProvider, 
      Settings settings)
    {
      this.logger = logger;
      this.data = data;
      this.raas = raas;
      this.simDataSnapshotProvider = simDataSnapshotProvider;
      this.settings = settings;
    }
  }
}
