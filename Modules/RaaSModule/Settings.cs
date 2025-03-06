using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.TTSs.MsSapi;
using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Eng.EFsExtensions.Modules.RaaSModule
{
  public class Settings : NotifyPropertyChanged
  {
    public class IcaoRule : NotifyPropertyChanged
    {

      public string Icao
      {
        get { return base.GetProperty<string>(nameof(Icao))!; }
        set { base.UpdateProperty(nameof(Icao), value); }
      }

      public int OrthoDistanceInMeters
      {
        get { return base.GetProperty<int>(nameof(OrthoDistanceInMeters))!; }
        set { base.UpdateProperty(nameof(OrthoDistanceInMeters), value); }
      }
    }

    public class HoldingPointThresholdData : NotifyPropertyChanged
    {
      public int MaxHeight
      {
        get { return base.GetProperty<int>(nameof(MaxHeight))!; }
        set { base.UpdateProperty(nameof(MaxHeight), value); }
      }


      public int TooCloseOrthoDistance
      {
        get { return base.GetProperty<int>(nameof(TooCloseOrthoDistance))!; }
        set { base.UpdateProperty(nameof(TooCloseOrthoDistance), value); }
      }


      public int AnnounceOrthoDistanceShortRwy
      {
        get { return base.GetProperty<int>(nameof(AnnounceOrthoDistanceShortRwy))!; }
        set { base.UpdateProperty(nameof(AnnounceOrthoDistanceShortRwy), value); }
      }


      public int AnnounceOrthoDistanceLongRwy
      {
        get { return base.GetProperty<int>(nameof(AnnounceOrthoDistanceLongRwy))!; }
        set { base.UpdateProperty(nameof(AnnounceOrthoDistanceLongRwy), value); }
      }


      public int ShortLongRunwayLengthThreshold
      {
        get { return base.GetProperty<int>(nameof(ShortLongRunwayLengthThreshold))!; }
        set { base.UpdateProperty(nameof(ShortLongRunwayLengthThreshold), value); }
      }


      public int TooFarOrthoDistance
      {
        get { return base.GetProperty<int>(nameof(TooFarOrthoDistance))!; }
        set { base.UpdateProperty(nameof(TooFarOrthoDistance), value); }
      }

      public ObservableCollection<IcaoRule> IcaoRules
      {
        get { return base.GetProperty<ObservableCollection<IcaoRule>>(nameof(IcaoRules))!; }
        set { base.UpdateProperty(nameof(IcaoRules), value); }
      }

      public HoldingPointThresholdData()
      {
        this.MaxHeight = 50;
        this.TooCloseOrthoDistance = 40;
        this.TooFarOrthoDistance = 250;
        this.AnnounceOrthoDistanceShortRwy = 60;
        this.AnnounceOrthoDistanceLongRwy = 90;
        this.ShortLongRunwayLengthThreshold = 1200;
        this.IcaoRules = new()
        {
          new IcaoRule()
          {
              Icao="TFFJ",
              OrthoDistanceInMeters = 50
          }
        };
      }
    }

    public class LineUpThresholdData : NotifyPropertyChanged
    {
      public LineUpThresholdData()
      {
        this.MaxHeadingDiff = 15;
        this.MaxHeight = 50;
        this.MaxOrthoDistance = 40;
        this.MaxSpeed = 20;
      }

      public int MaxHeight
      {
        get { return base.GetProperty<int>(nameof(MaxHeight))!; }
        set { base.UpdateProperty(nameof(MaxHeight), value); }
      }


      public int MaxOrthoDistance
      {
        get { return base.GetProperty<int>(nameof(MaxOrthoDistance))!; }
        set { base.UpdateProperty(nameof(MaxOrthoDistance), value); }
      }


      public int MaxSpeed
      {
        get { return base.GetProperty<int>(nameof(MaxSpeed))!; }
        set { base.UpdateProperty(nameof(MaxSpeed), value); }
      }


      public double MaxHeadingDiff
      {
        get { return base.GetProperty<double>(nameof(MaxHeadingDiff))!; }
        set { base.UpdateProperty(nameof(MaxHeadingDiff), value); }
      }


    }

    public class LandingThresholdData : NotifyPropertyChanged
    {
      public LandingThresholdData()
      {
        MinHeight = 50;
        MaxHeight = 800;
        MaxOrthoDistance = 300;
        MaxDistance = (int) new Model.RaasDistance(4, Model.RaasDistance.RaasDistanceUnit.nm).GetInMeters();
      }

      public int MinHeight
      {
        get { return base.GetProperty<int>(nameof(MinHeight))!; }
        set { base.UpdateProperty(nameof(MinHeight), value); }
      }

      public int MaxHeight
      {
        get { return base.GetProperty<int>(nameof(MaxHeight))!; }
        set { base.UpdateProperty(nameof(MaxHeight), value); }
      }

      public int MaxDistance
      {
        get { return base.GetProperty<int>(nameof(MaxDistance))!; }
        set { base.UpdateProperty(nameof(MaxDistance), value); }
      }

      public int MaxOrthoDistance
      {
        get { return base.GetProperty<int>(nameof(MaxOrthoDistance))!; }
        set { base.UpdateProperty(nameof(MaxOrthoDistance), value); }
      }

      public int MaxVerticalSpeed
      {
        get { return base.GetProperty<int>(nameof(MaxVerticalSpeed))!; }
        set { base.UpdateProperty(nameof(MaxVerticalSpeed), value); }
      }
    }

    public class RemainingDistanceThresholdData : NotifyPropertyChanged
    {
      public RemainingDistanceThresholdData()
      {
        MaxHeight = 50;
        MaxOrthoDistance = 40;
        MaxHeadingDiff = 15;
      }

      public int MaxHeight
      {
        get { return base.GetProperty<int>(nameof(MaxHeight))!; }
        set { base.UpdateProperty(nameof(MaxHeight), value); }
      }

      public int MaxOrthoDistance
      {
        get { return base.GetProperty<int>(nameof(MaxOrthoDistance))!; }
        set { base.UpdateProperty(nameof(MaxOrthoDistance), value); }
      }

      public int MaxHeadingDiff
      {
        get { return base.GetProperty<int>(nameof(MaxHeadingDiff))!; }
        set { base.UpdateProperty(nameof(MaxHeadingDiff), value); }
      }
    }

    private const string FILE_NAME = "raas-module-settings.xml";

    public MsSapiSettings Synthetizer { get; set; } = new();

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


    public HoldingPointThresholdData HoldingPointThresholds
    {
      get { return base.GetProperty<HoldingPointThresholdData>(nameof(HoldingPointThresholds))!; }
      set { base.UpdateProperty(nameof(HoldingPointThresholds), value); }
    }

    public LineUpThresholdData LineUpThresholds
    {
      get { return base.GetProperty<LineUpThresholdData>(nameof(LineUpThresholds))!; }
      set { base.UpdateProperty(nameof(LineUpThresholds), value); }
    }
    public LandingThresholdData LandingThresholds
    {
      get { return base.GetProperty<LandingThresholdData>(nameof(LandingThresholds))!; }
      set { base.UpdateProperty(nameof(LandingThresholds), value); }
    }

    public RemainingDistanceThresholdData RemainingDistanceThresholds
    {
      get { return base.GetProperty<RemainingDistanceThresholdData>(nameof(RemainingDistanceThresholds))!; }
      set { base.UpdateProperty(nameof(RemainingDistanceThresholds), value); }
    }

    public Settings()
    {
      this.AutoLoadedAirportsFile = null;
      this.AutoLoadedRaasFile = null;
      this.HoldingPointThresholds = new();
      this.LineUpThresholds = new();
      this.LandingThresholds = new();
      this.RemainingDistanceThresholds = new();
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
