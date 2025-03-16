using Eng.EFsExtensions.Libs.AirportsLib;
using ESystem.Exceptions;
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

    public string AirportsXmlFile
    {
      get => base.GetProperty<string>(nameof(AirportsXmlFile))!;
      set
      {
        base.UpdateProperty(nameof(AirportsXmlFile), value);
        if (value == null || System.IO.File.Exists(value) == false)
          Airports = new();
        else
          //TODO do in better way
          try
          {
            Airports = XmlLoader.Load(value, true).OrderBy(q => q.ICAO).ToList();
          }
          catch (Exception ex)
          {
            throw new Exception($"Error loading airports from '{value}'", ex);
          }
      }
    }

    public List<Airport> Airports
    {
      get => base.GetProperty<List<Airport>>(nameof(Airports)) ?? throw new UnexpectedNullException();
      set => base.UpdateProperty(nameof(Airports), value);
    }

    public Settings()
    {
      this.Airports = new();
      this.DataFolder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "EFsExtensions");
      if (System.IO.Directory.Exists(this.DataFolder) == false)
        System.IO.Directory.CreateDirectory(this.DataFolder);
    }
  }
}
