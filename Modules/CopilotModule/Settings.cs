using Eng.EFsExtensions.EFsExtensionsModuleBase;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.Synthetization;
using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Serialization;

namespace Eng.EFsExtensions.Modules.CopilotModule
{
  public class Settings : NotifyPropertyChanged
  {

    private const string FILE_NAME = "copilot-module-settings.xml";
    public bool LogSimConnectToFile
    {
      get => base.GetProperty<bool>(nameof(LogSimConnectToFile))!;
      set => base.UpdateProperty(nameof(LogSimConnectToFile), value);
    }

    public SynthetizerSettings Synthetizer { get; set; } = new();

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
