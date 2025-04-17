using Eng.EFsExtensions.Libs.AirportsLib;
using ESystem.Exceptions;
using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation.Peers;

namespace Eng.EFsExtensions.Modules.FlightLogModule
{
  public class Settings : NotifyPropertyChanged
  {

    public string? VatsimId
    {
      get => base.GetProperty<string?>(nameof(VatsimId))!;
      set => base.UpdateProperty(nameof(VatsimId), value);
    }

    public string? SimBriefId
    {
      get => base.GetProperty<string?>(nameof(SimBriefId))!;
      set => base.UpdateProperty(nameof(SimBriefId), value);
    }

    public string DataFolder
    {
      get => base.GetProperty<string>(nameof(DataFolder))!;
      set => base.UpdateProperty(nameof(DataFolder), value);
    }

    public Settings()
    {
      this.DataFolder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "EFsExtensions");
      if (System.IO.Directory.Exists(this.DataFolder) == false)
        System.IO.Directory.CreateDirectory(this.DataFolder);

      //todo remove
      this.SimBriefId = "475902";
      this.VatsimId = "964586";
    }
  }
}
