using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.RaaSModule.Model
{
  class SimDataSnapshot
  {
    public int Height { get; set; }
    public int IndicatedSpeed { get; set; }
    public double Heading { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double VerticalSpeed { get; set; }
  }
}
