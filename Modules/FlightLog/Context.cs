using Eng.Chlaot.Modules.FlightLogModule.Navdata;
using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.Modules.FlightLogModule
{
  public class Context : NotifyPropertyChanged
  {

    public Dictionary<string, Airport> AirportsDict
    {
      get { return base.GetProperty<Dictionary<string, Airport>>(nameof(AirportsDict))!; }
      set
      {
        base.UpdateProperty(nameof(AirportsDict), value);
        this.AirportsList = value.Values.OrderBy(q => q.ICAO).ToList();
      }
    }


    public List<Airport> AirportsList
    {
      get { return base.GetProperty<List<Airport>>(nameof(AirportsList))!; }
      private set { base.UpdateProperty(nameof(AirportsList), value); }
    }
  }
}
