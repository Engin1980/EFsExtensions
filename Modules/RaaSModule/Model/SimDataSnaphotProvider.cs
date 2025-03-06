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
  class SimDataSnaphotProvider
  {
    private ValueCacheExtender cache = null!;
    private readonly Dictionary<string, TypeId> propTypeId = new();

    private double GetPropertyValue(string propertyName) => cache.GetValue(propTypeId[propertyName]);

    public SimDataSnapshot GetSnapshot()
    {
      SimDataSnapshot ret = new()
      {
        Height = (int)GetPropertyValue(nameof(SimDataSnapshot.Height)),
        IndicatedSpeed = (int)GetPropertyValue(nameof(SimDataSnapshot.IndicatedSpeed)),
        Heading = (int)GetPropertyValue(nameof(SimDataSnapshot.Heading)),
        Latitude = (int)GetPropertyValue(nameof(SimDataSnapshot.Latitude)),
        Longitude = (int)GetPropertyValue(nameof(SimDataSnapshot.Longitude)),
        VerticalSpeed = (int)GetPropertyValue(nameof(SimDataSnapshot.VerticalSpeed))
      };
      return ret;
    }

    internal void Connect(ValueCacheExtender valueCacheExtender)
    {
      this.cache = valueCacheExtender;

      propTypeId[nameof(SimDataSnapshot.Height)] =
        this.cache.Register(SimVars.Aircraft.Miscelaneous.PLANE_ALT_ABOVE_GROUND, SimUnits.Length.FOOT);
      propTypeId[nameof(SimDataSnapshot.IndicatedSpeed)] =
        this.cache.Register(SimVars.Aircraft.Miscelaneous.AIRSPEED_INDICATED, SimUnits.Speed.KNOT);
      propTypeId[nameof(SimDataSnapshot.Heading)] =
        this.cache.Register(SimVars.Aircraft.Miscelaneous.PLANE_HEADING_DEGREES_MAGNETIC, SimUnits.Angle.DEGREE);
      propTypeId[nameof(SimDataSnapshot.Latitude)] =
        this.cache.Register(SimVars.Aircraft.Miscelaneous.PLANE_LATITUDE, SimUnits.Angle.DEGREE);
      propTypeId[nameof(SimDataSnapshot.Longitude)] =
        this.cache.Register(SimVars.Aircraft.Miscelaneous.PLANE_LONGITUDE, SimUnits.Angle.DEGREE);
      propTypeId[nameof(SimDataSnapshot.VerticalSpeed)] =
        this.cache.Register(SimVars.Aircraft.Miscelaneous.VERTICAL_SPEED, SimUnits.Length.FOOT);
    }
  }
}
