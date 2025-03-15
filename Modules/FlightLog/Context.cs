﻿using Eng.EfsExtensions.Modules.FlightLogModule.Navdata;
using ESystem.Asserting;
using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EfsExtensions.Modules.FlightLogModule
{
  public class Context : NotifyPropertyChanged
  {
    private readonly Action onReadySet;

    public Context(Action onReadySet)
    {
      EAssert.Argument.IsNotNull(onReadySet, nameof(onReadySet));
      this.onReadySet = onReadySet;
    }

    public Dictionary<string, Airport> AirportsDict
    {
      get { return base.GetProperty<Dictionary<string, Airport>>(nameof(AirportsDict))!; }
      set
      {
        base.UpdateProperty(nameof(AirportsDict), value);
        this.AirportsList = value.Values.OrderBy(q => q.ICAO).ToList();
      }
    }

    internal RunModel RunModel
    {
      get { return base.GetProperty<RunModel>(nameof(RunModel))!; }
      set { base.UpdateProperty(nameof(RunModel), value); }
    }

    public List<Airport> AirportsList
    {
      get { return base.GetProperty<List<Airport>>(nameof(AirportsList))!; }
      private set { base.UpdateProperty(nameof(AirportsList), value); }
    }

    public bool IsReady
    {
      get { return base.GetProperty<bool>(nameof(IsReady))!; }
      set
      {
        base.UpdateProperty(nameof(IsReady), value);
        if (value) onReadySet();
      }
    }
  }
}
