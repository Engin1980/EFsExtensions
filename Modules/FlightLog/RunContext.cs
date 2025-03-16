using Eng.EFsExtensions.Libs.AirportsLib;
using ESystem.Asserting;
using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.FlightLogModule
{
  public class RunContext : NotifyPropertyChanged
  {
    internal RunModel RunModel
    {
      get { return base.GetProperty<RunModel>(nameof(RunModel))!; }
      set { base.UpdateProperty(nameof(RunModel), value); }
    }
  }
}
