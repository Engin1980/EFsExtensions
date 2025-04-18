using Eng.EFsExtensions.Libs.AirportsLib;
using ESystem.Exceptions;
using ESystem.Miscelaneous;
using ESystem.Structs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation.Peers;
using System.Xml.Serialization;

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

    public DistanceUnit ShortDistanceUnit
    {
      get => base.GetProperty<DistanceUnit>(nameof(ShortDistanceUnit))!;
      set => base.UpdateProperty(nameof(ShortDistanceUnit), value);
    }

    public DistanceUnit LongDistanceUnit
    {
      get => base.GetProperty<DistanceUnit>(nameof(LongDistanceUnit))!;
      set => base.UpdateProperty(nameof(LongDistanceUnit), value);
    }

    public SpeedUnit SpeedUnit
    {
      get => base.GetProperty<SpeedUnit>(nameof(SpeedUnit))!;
      set => base.UpdateProperty(nameof(SpeedUnit), value);
    }

    public WeightUnit WeightUnit
    {
      get => base.GetProperty<WeightUnit>(nameof(WeightUnit))!;
      set => base.UpdateProperty(nameof(WeightUnit), value);
    }

    public Settings()
    {
      this.DataFolder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "EFsExtensions");
      if (System.IO.Directory.Exists(this.DataFolder) == false)
        System.IO.Directory.CreateDirectory(this.DataFolder);

      ShortDistanceUnit = DistanceUnit.Meters;
      LongDistanceUnit = DistanceUnit.Kilometers;
      WeightUnit = WeightUnit.Kilograms;
      SpeedUnit = SpeedUnit.KTS;

      //todo remove
      this.SimBriefId = "475902";
      this.VatsimId = "964586";
    }

    private const string FILE_NAME = "flight-log-settings.xml";

    public static Settings Load()
    {
      Settings ret;
      try
      {
        using FileStream fs = new(FILE_NAME, FileMode.Open);
        XmlSerializer ser = new(typeof(Settings));
        ret = (Settings)ser.Deserialize(fs)!;
      }
      catch (Exception ex)
      {
        throw new ApplicationException($"Failed to deserialize settings from {FILE_NAME}.", ex);
      }
      return ret;
    }

    public void Save()
    {
      try
      {
        string file = Path.GetTempFileName();
        using (FileStream fs = new(file, FileMode.Truncate))
        {
          XmlSerializer ser = new(typeof(Settings));
          ser.Serialize(fs, this);
        }
        System.IO.File.Copy(file, FILE_NAME, true);
        System.IO.File.Delete(file);
      }
      catch (Exception ex)
      {
        throw new ApplicationException($"Failed to serialize settings to {FILE_NAME}.", ex);
      }
    }
  }
}
