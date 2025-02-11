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
  internal class LandingContextHandler : ContextHandler
  {
    private const int MIN_PLANE_HEIGHT_TO_PROCESS_IN_M = 33;
    private const int MAX_PLANE_HEIGHT_TO_PROCESS_IN_M = 700;
    private const int MAX_SHIFT_DISTANCE_OFF_THE_RUNWAY_IN_M = 300;
    private RunwayThreshold? lastLandingThreshold = null;

    public LandingContextHandler(Logger logger, Context.RuntimeDataBox data, Raas raas, Func<SimDataStruct> simDataProvider) : base(logger, data, raas, simDataProvider)
    {
    }

    public override void Handle()
    {
      Debug.Assert(data.NearestAirport != null);
      var airport = data.NearestAirport.Airport;
      var simData = simDataProvider();

      if (simData.Height > MAX_PLANE_HEIGHT_TO_PROCESS_IN_M)
      {
        data.LandingStatus = $"Plane height {simData.Height} over limit {MAX_PLANE_HEIGHT_TO_PROCESS_IN_M}";
        lastLandingThreshold = null;
        return;
      }
      else if (simData.Height < MIN_PLANE_HEIGHT_TO_PROCESS_IN_M)
      {
        data.LandingStatus = $"Plane height {simData.Height} under limit {MIN_PLANE_HEIGHT_TO_PROCESS_IN_M}";
        lastLandingThreshold = null;
        return;
      }

      var tmpR = data.NearestRunways; //TODO calculate the next only for the closest runway?
      var tmpT = tmpR.SelectMany(q => q.Runway.Thresholds, (r, t) => new
      {
        Runway = r.Runway,
        Threshold = t,
        ShiftDistance = r.ShiftDistance,
        ThresholdDistance = GpsCalculator.GetDistance(
          t.Coordinate.Latitude, t.Coordinate.Longitude,
          simData.latitude, simData.longitude),
        Bearing = GpsCalculator.InitialBearing(
          simData.latitude, simData.longitude,
          t.Coordinate.Latitude, t.Coordinate.Longitude)
      });
      data.Landing = tmpT
        .Select(q => new LandingRaasData(airport, q.Runway, q.Threshold, q.ShiftDistance, q.ThresholdDistance, (Heading)q.Bearing))
        .OrderBy(q => q.ShiftDistance).ThenBy(q => q.ThresholdDistance)
        .ToList();

      var thrsCandidate = data.Landing.First() ?? throw new UnexpectedNullException();

      if (thrsCandidate.ThresholdDistance > raas.Speeches.LandingRunway.Distance.GetInMeters())
      {
        data.LandingStatus = $"{thrsCandidate.Airport.ICAO}/{thrsCandidate.Threshold.Designator} threshold-distance {thrsCandidate.ThresholdDistance} over limit {raas.Speeches.LandingRunway.Distance.GetInMeters()}";
      }
      else if (thrsCandidate.ShiftDistance > MAX_SHIFT_DISTANCE_OFF_THE_RUNWAY_IN_M)
      {
        data.LandingStatus = $"{thrsCandidate.Airport.ICAO}/{thrsCandidate.Threshold.Designator} shift-distance {thrsCandidate.ShiftDistance} over limit {MAX_SHIFT_DISTANCE_OFF_THE_RUNWAY_IN_M}";
      }
      else
      {
        if (lastLandingThreshold != thrsCandidate.Threshold)
        {
          lastLandingThreshold = thrsCandidate.Threshold;
          Say(raas.Speeches.LandingRunway, thrsCandidate.Threshold);
          data.LandingStatus = $"{thrsCandidate.Airport.ICAO}/{thrsCandidate.Threshold.Designator} announced";
        }
        else
        {
          data.LandingStatus = $"{thrsCandidate.Airport.ICAO}/{thrsCandidate.Threshold.Designator} already announced";
        }
      }
    }
  }
}
