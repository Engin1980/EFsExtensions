using Eng.Chlaot.Libs.AirportsLib;
using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.Modules.RaaSModule
{
  internal class Context : NotifyPropertyChanged
  {
    public List<Airport> Airports
    {
      get { return base.GetProperty<List<Airport>>(nameof(Airports))!; }
      set { base.UpdateProperty(nameof(Airports), value); }
    }

    internal void LoadFile(string recentXmlFile)
    {
      this.Airports = XmlLoader.Load(recentXmlFile);
    }
  }
}
