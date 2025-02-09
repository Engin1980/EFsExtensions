using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.Synthetization;
using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Eng.Chlaot.Modules.RaaSModule
{
  public class Settings : NotifyPropertyChanged
  {

    private const string FILE_NAME = "raas-module-settings.xml";
    
    public SynthetizerSettings Synthetizer { get; set; } = new();


    public string? AutoLoadedAirportsFile
    {
      get { return base.GetProperty<string?>(nameof(AutoLoadedAirportsFile))!; }
      set { base.UpdateProperty(nameof(AutoLoadedAirportsFile), value); }
    }

    public string? AutoLoadedRaasFile
    {
      get { return base.GetProperty<string?>(nameof(AutoLoadedRaasFile))!; }
      set { base.UpdateProperty(nameof(AutoLoadedRaasFile), value); }
    }

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
