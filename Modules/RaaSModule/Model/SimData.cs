using ESimConnect.Definitions;
using ESimConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.SimObjects;
using ESimConnect.Extenders;

namespace Eng.EFsExtensions.Modules.RaaSModule.Model
{
  class SimData
  {
    private ValueCacheExtender cache = null!;
    private readonly Dictionary<string, TypeId> propTypeId = new();

    public int Height => (int)GetPropertyValue(nameof(Height));
    public int IndicatedSpeed => (int)GetPropertyValue(nameof(IndicatedSpeed));
    public double Heading => GetPropertyValue(nameof(Heading));
    public double Latitude => GetPropertyValue(nameof(Latitude));
    public double Longitude => GetPropertyValue(nameof(Longitude));
    public double VerticalSpeed => GetPropertyValue(nameof(VerticalSpeed));

    private double GetPropertyValue(string propertyName) => cache.GetValue(propTypeId[propertyName]);

    internal void Connect(ValueCacheExtender valueCacheExtender)
    {
      this.cache = valueCacheExtender;

      propTypeId[nameof(Height)] =
        this.cache.Register(SimVars.Aircraft.Miscelaneous.PLANE_ALT_ABOVE_GROUND, SimUnits.Length.FOOT);
      propTypeId[nameof(IndicatedSpeed)] =
        this.cache.Register(SimVars.Aircraft.Miscelaneous.AIRSPEED_INDICATED, SimUnits.Speed.KNOT);
      propTypeId[nameof(Heading)] =
        this.cache.Register(SimVars.Aircraft.Miscelaneous.PLANE_HEADING_DEGREES_MAGNETIC, SimUnits.Angle.DEGREE);
      propTypeId[nameof(Latitude)] =
        this.cache.Register(SimVars.Aircraft.Miscelaneous.PLANE_LATITUDE, SimUnits.Angle.DEGREE);
      propTypeId[nameof(Longitude)] =
        this.cache.Register(SimVars.Aircraft.Miscelaneous.PLANE_LONGITUDE, SimUnits.Angle.DEGREE);
      propTypeId[nameof(VerticalSpeed)] =
        this.cache.Register(SimVars.Aircraft.Miscelaneous.VERTICAL_SPEED, SimUnits.Length.FOOT);
    }
  }
}
