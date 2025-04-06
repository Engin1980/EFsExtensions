using Eng.EFsExtensions.EFsExtensionsModuleBase;
using Eng.EFsExtensions.Libs.AirportsLib;
using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.FlightLogModule
{
  public class InitContext : NotifyPropertyChanged
  {
    public record Profile(string Name, string Path, int XmlFilesCount);
    private Action onReadySet;

    private readonly Settings settings;
    public bool IsReady
    {
      get => base.GetProperty<bool>(nameof(IsReady))!;
      set => base.UpdateProperty(nameof(IsReady), value);
    }

    public BindingList<Profile> Profiles
    {
      get => base.GetProperty<BindingList<Profile>>(nameof(Profiles))!;
      set => base.UpdateProperty(nameof(Profiles), value);
    }

    public List<Airport> Airports
    {
      get => this.settings.Airports;
    }

    public Profile? SelectedProfile
    {
      get => base.GetProperty<Profile?>(nameof(SelectedProfile))!;
      set => base.UpdateProperty(nameof(SelectedProfile), value);
    }

    public InitContext(Settings settings, Action onReadySet)
    {
      this.onReadySet = onReadySet;
      this.settings = settings;
      this.Profiles = new();
      IsReady = false;
      LoadProfiles();
    }

    private void LoadProfiles()
    {
      var folders = System.IO.Directory.GetDirectories(settings.DataFolder).ToList();
      folders.ForEach(q => Profiles.Add(new(System.IO.Path.GetFileName(q), q, System.IO.Directory.GetFiles(q, "*.xml").Length)));
      SelectedProfile = Profiles.FirstOrDefault();
    }

    internal void CreateProfile(string input)
    {
      var path = System.IO.Path.Combine(settings.DataFolder, input);
      System.IO.Directory.CreateDirectory(path);
      Profiles.Add(new(input, path, 0));
    }
  }
}
