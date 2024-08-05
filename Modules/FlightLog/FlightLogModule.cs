using Eng.Chlaot.ChlaotModuleBase;
using Eng.Chlaot.Modules.FlightLogModule.Navdata;
using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using FlightLogModule;

namespace Eng.Chlaot.Modules.FlightLogModule
{
  public class FlightLogModule : NotifyPropertyChanged, IModule
  {

    public bool IsReady
    {
      get { return base.GetProperty<bool>(nameof(IsReady))!; }
      set { base.UpdateProperty(nameof(IsReady), value); }
    }

    public Context Context
    {
      get => GetProperty<Context>(nameof(Context))!;
      set => UpdateProperty(nameof(Context), value);
    }


    public Control InitControl { get; private set; } = null!;

    public Control RunControl { get; private set; } = null!;

    public string Name => "Flight Module";

    public void Init()
    {
      var airports = LoadNavdata();

      this.Context = new Context(() => this.IsReady = true)
      {
        AirportsDict = airports
      };

      InitControl = new CtrInit(Context);
    }

    private Dictionary<string, Airport> LoadNavdata()
    {
      const string airportFile = @".\Data\airports.csv";
      const string runwaysFile = @".\Data\runways.csv";

      Dictionary<string, Airport> airports = Loader.LoadAirports(airportFile);
      Loader.LoadRunways(runwaysFile, airports);
      return airports;
    }

    public void Restore(Dictionary<string, string> restoreData)
    {
      //throw new NotImplementedException();
    }

    public void Run()
    {
      this.RunControl = new CtrRun(this.Context);
    }

    public void SetUp(ModuleSetUpInfo setUpInfo)
    {
      //throw new NotImplementedException();
    }

    public void Stop()
    {
      //throw new NotImplementedException();
    }

    public Dictionary<string, string>? TryGetRestoreData()
    {
      return null; //TODO
    }
  }
}
