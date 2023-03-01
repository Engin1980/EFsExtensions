using ChlaotModuleBase;
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

namespace CopilotModule
{
  public class Settings : NotifyPropertyChangedBase
  {
    public class SynthetizerSettings : NotifyPropertyChangedBase
    {
      [XmlIgnore]
      public string[] AvailableVoices { get; set; }
      public int EndTrimMiliseconds
      {
        get => base.GetProperty<int>(nameof(EndTrimMiliseconds))!;
        set => base.UpdateProperty(nameof(EndTrimMiliseconds), Math.Max(value, 0));
      }

      public TimeSpan EndTrimMilisecondsTimeSpan { get => TimeSpan.FromMilliseconds(EndTrimMiliseconds); }

      public int Rate
      {
        get => base.GetProperty<int>(nameof(Rate))!;
        set => base.UpdateProperty(nameof(Rate), Math.Max(Math.Min(value, 10), -10));
      }

      public int StartTrimMiliseconds
      {
        get => base.GetProperty<int>(nameof(StartTrimMiliseconds))!;
        set => base.UpdateProperty(nameof(StartTrimMiliseconds), Math.Max(value, 0));
      }

      public TimeSpan StartTrimMilisecondsTimeSpan { get => TimeSpan.FromMilliseconds(StartTrimMiliseconds); }

      public string Voice
      {
        get => base.GetProperty<string>(nameof(Voice))!;
        set => base.UpdateProperty(nameof(Voice), value);
      }
      public SynthetizerSettings()
      {
        this.AvailableVoices = new SpeechSynthesizer().GetInstalledVoices().Select(q => q.VoiceInfo.Name).ToArray();
        this.Voice = this.AvailableVoices.FirstOrDefault() ?? "";
        this.Rate = 0;
        this.StartTrimMiliseconds = 0;
        this.EndTrimMiliseconds = 750;
      }
    }
    private const string FILE_NAME = "copilot-module-settings.xml";
    public bool LogSimConnectToFile
    {
      get => base.GetProperty<bool>(nameof(LogSimConnectToFile))!;
      set => base.UpdateProperty(nameof(LogSimConnectToFile), value);
    }

    public bool ReadConfirmations
    {
      get => base.GetProperty<bool>(nameof(ReadConfirmations))!;
      set => base.UpdateProperty(nameof(ReadConfirmations), value);
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
