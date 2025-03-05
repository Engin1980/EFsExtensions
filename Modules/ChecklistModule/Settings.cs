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

namespace Eng.EFsExtensions.Modules.ChecklistModule
{
  public class Settings : NotifyPropertyChanged
  {
    public class KeyShortcut : NotifyPropertyChanged
    {
      [XmlIgnore]
      [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822")]
      public List<Key> AllKeys => Enum.GetValues(typeof(Key)).Cast<Key>().ToList();

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

      public Key Key
      {
        get => base.GetProperty<Key>(nameof(Key))!;
        set => base.UpdateProperty(nameof(Key), value);
      }

      public bool Shift
      {
        get => base.GetProperty<bool>(nameof(Shift))!;
        set => base.UpdateProperty(nameof(Shift), value);
      }

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

    private const string FILE_NAME = "checklist-module-settings.xml";
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

    public bool PlayPerItem
    {
      get { return base.GetProperty<bool>(nameof(PlayPerItem))!; }
      set { base.UpdateProperty(nameof(PlayPerItem), value); }
    }


    public bool AlertOnPausedChecklist
    {
      get { return base.GetProperty<bool>(nameof(AlertOnPausedChecklist))!; }
      set { base.UpdateProperty(nameof(AlertOnPausedChecklist), value); }
    }


    public int PausedChecklistAlertInterval
    {
      get { return base.GetProperty<int>(nameof(PausedChecklistAlertInterval))!; }
      set { base.UpdateProperty(nameof(PausedChecklistAlertInterval), Math.Max(0, value)); }
    }


    public KeyShortcuts Shortcuts { get; set; } = new()
    {
      PlayPause = new KeyShortcut(true, false, false, Key.X),
      SkipToNext = new KeyShortcut(true, true, false, Key.X),
      SkipToPrevious = new KeyShortcut(true, true, true, Key.X)
    };

    public SynthetizerSettings Synthetizer { get; set; } = new();
    public bool UseAutoplay
    {
      get => base.GetProperty<bool>(nameof(UseAutoplay))!;
      set => base.UpdateProperty(nameof(UseAutoplay), value);
    }

    public Settings()
    {
      this.PausedChecklistAlertInterval = 30;
      this.UseAutoplay = true;
      this.ReadConfirmations = true;
      this.AlertOnPausedChecklist = true;
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
