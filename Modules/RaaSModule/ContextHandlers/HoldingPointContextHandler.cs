using ELogging;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.SimConWrapping.PrdefinedTypes;
using Eng.Chlaot.Libs.AirportsLib;
using Eng.Chlaot.Modules.RaaSModule.Model;
using ESystem.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Eng.Chlaot.Modules.RaaSModule.Context;

namespace Eng.Chlaot.Modules.RaaSModule.ContextHandlers
{
  internal class HoldingPointContextHandler : ContextHandler
  {
    private Runway? lastHoldingPointRunway;
    private const int MAX_HEIGHT_TO_BE_EXPECTED_ON_THE_GROUND_IN_FT = 50;
    private const int HOLDING_POINT_ALREADY_ON_RUNWAY_DISTANCE = 100;
    private const int HOLDING_POINT_LINED_UP_DISTANCE = 20;
    private const int HOLDING_POINT_EXIT_SHIFT_DISTANCE = 200;

    public HoldingPointContextHandler(Logger logger, RuntimeDataBox data, Raas raas, Func<SimDataStruct> simDataProvider )
      :base(logger, data, raas, simDataProvider)
    {
    }

    public override void Handle()
    {
      Debug.Assert(data.NearestAirport != null);
      var simData = simDataProvider();

      if (simData.Height > MAX_HEIGHT_TO_BE_EXPECTED_ON_THE_GROUND_IN_FT)
      {
        data.GroundHoldingPointStatus = $"Plane probably airborne - height {simData.Height} over limit {MAX_HEIGHT_TO_BE_EXPECTED_ON_THE_GROUND_IN_FT}";
        lastHoldingPointRunway = null;
        return;
      }

      data.GroundHoldingPoint = data.NearestRunways
        .Select(q => new GroundRaasHoldingPointData(
        data.NearestAirport.Airport,
        q.Runway,
          q.ShiftDistance))
        .OrderBy(q => q.ShiftDistance)
        .ToList();

      var grtd = data.GroundHoldingPoint.First();
      if (grtd.ShiftDistance > HOLDING_POINT_EXIT_SHIFT_DISTANCE)
      {
        lastHoldingPointRunway = null;
        data.GroundHoldingPointStatus = $"Best shift-distance threshold {grtd.Airport.ICAO}/{grtd.Runway.Designator} too far";
      }
      else if (grtd.ShiftDistance < HOLDING_POINT_ALREADY_ON_RUNWAY_DISTANCE)
      {
        // entered runway, calls are ignored
        lastHoldingPointRunway = grtd.Runway;
        data.GroundHoldingPointStatus = $"Best shift-distance threshold {grtd.Airport.ICAO}/{grtd.Runway.Designator} too close (probably on the runway?)";
      }
      else if (grtd.ShiftDistance < HOLDING_POINT_ALREADY_ON_RUNWAY_DISTANCE)
      {
        if (lastHoldingPointRunway != grtd.Runway)
        {
          lastHoldingPointRunway = grtd.Runway;
          var closestThreshold = grtd.Runway.Thresholds
            .MinBy(q => GpsCalculator.GetDistance(q.Coordinate.Latitude, q.Coordinate.Longitude, simData.latitude, simData.longitude))
            ?? throw new UnexpectedNullException();
          Say(raas.Speeches.TaxiToRunway, closestThreshold);
          data.GroundHoldingPointStatus = $"Threshold {grtd.Airport.ICAO}/{grtd.Runway.Designator} announced";
        }
        else
        {
          data.GroundHoldingPointStatus = $"Threshold {grtd.Airport.ICAO}/{grtd.Runway.Designator} already announced";
        }
      }
      else
      {
        data.GroundHoldingPointStatus = $"Threshold {grtd.Airport.ICAO}/{grtd.Runway.Designator} shift-distance {grtd.ShiftDistance} not close enought for announcement.";
      }
    }
  }
}
