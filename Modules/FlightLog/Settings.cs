using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.FlightLogModule
{
  public class Settings : NotifyPropertyChanged
  {
    public string? SimBriefId
    {
      get => base.GetProperty<string?>(nameof(SimBriefId))!;
      set => base.UpdateProperty(nameof(SimBriefId), value);
    }
  }
}
