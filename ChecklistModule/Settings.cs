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

namespace ChecklistModule
{
  public class Settings
  {
    public class SynthetizerSettings : NotifyPropertyChangedBase
    {
      [XmlIgnore]
      public string[] AvailableVoices { get; set; }
      public string Voice
      {
        get => base.GetProperty<string>(nameof(Voice))!;
        set => base.UpdateProperty(nameof(Voice), value);
      }

      public int Rate
      {
        get => base.GetProperty<int>(nameof(Rate))!;
        set => base.UpdateProperty(nameof(Rate), Math.Max(Math.Min(value, 10), -10));
      }

      public SynthetizerSettings()
      {
        this.AvailableVoices = new SpeechSynthesizer().GetInstalledVoices().Select(q => q.VoiceInfo.Name).ToArray();
        this.Voice = this.AvailableVoices.FirstOrDefault() ?? "";
        this.Rate = 0;
      }
    }

    public class KeyShortcut : NotifyPropertyChangedBase
    {
      public KeyShortcut()
      {
        this.Alt = false;
        this.Control = true;
        this.Shift = false;
        this.Key = Key.X;
      }

      public KeyShortcut(bool control, bool alt, bool shift, Key key)
      {
        Control = control;
        Alt = alt;
        Shift = shift;
        Key = key;
      }

      public bool Alt
      {
        get => base.GetProperty<bool>(nameof(Alt))!;
        set => base.UpdateProperty(nameof(Alt), value);
      }

      public bool Control
      {
        get => base.GetProperty<bool>(nameof(Control))!;
        set => base.UpdateProperty(nameof(Control), value);
      }

      public bool Shift
      {
        get => base.GetProperty<bool>(nameof(Shift))!;
        set => base.UpdateProperty(nameof(Shift), value);
      }

      public Key Key
      {
        get => base.GetProperty<Key>(nameof(Key))!;
        set => base.UpdateProperty(nameof(Key), value);
      }

      [XmlIgnore]
      public List<Key> AllKeys => Enum.GetValues(typeof(Key)).Cast<Key>().ToList();

      public override string ToString()
      {
        List<string> tmp = new();
        if (Alt) tmp.Add("Alt");
        if (Control) tmp.Add("Ctrl");
        if (Shift) tmp.Add("Shift");
        tmp.Add(Key.ToString());
        string ret = string.Join("+", tmp);
        return ret;
      }
    }

    public class KeyShortcuts
    {
      public KeyShortcut PlayPause { get; set; } = new();
      public KeyShortcut SkipToNext { get; set; } = new();
      public KeyShortcut SkipToPrevious { get; set; } = new();
    }

    public SynthetizerSettings Synthetizer { get; set; } = new();
    public KeyShortcuts Shortcuts { get; set; } = new()
    {
      PlayPause = new KeyShortcut(true, false, false, Key.X),
      SkipToNext = new KeyShortcut(true, true, false, Key.X),
      SkipToPrevious = new KeyShortcut(true, true, true, Key.X)
    };
    private const string FILE_NAME = "checklist-module-settings.xml";
    public static Settings Load()
    {
      Settings ret;
      try
      {
        using (FileStream fs = new(FILE_NAME, FileMode.Open))
        {
          XmlSerializer ser = new XmlSerializer(typeof(Settings));
          ret = (Settings)ser.Deserialize(fs);
        }
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
        string file = System.IO.Path.GetTempFileName();
        using (FileStream fs = new FileStream(file, FileMode.Truncate))
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
