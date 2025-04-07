using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.FlightLogModule.Models.ActiveFlight.VatsimModel
{
  public record FlightPlan(
    long Id,
    long ConnectionId,
    string vatsim_id,
    string flight_type,
    string Callsign,
    string Aircraft,
    string cruisespeed,
    string Dep,
    string Arr,
    string Alt,
    string Altitude,
    string Rmks,
    string Route,
    string DepTime,
    int HrsEnroute,
    int MinEnroute,
    int HrsFuel,
    int MinFuel,
    string Filed,
    string AssignedSquawk,
    string ModifiedByCid,
    string ModifiedByCallsign
  )
  {
    public string CruiseSpeed = cruisespeed;
    public string VatsimId => vatsim_id;
    public string FlightType => flight_type;
    public string? GetRegistration()
    {
        string? ret;
        string pattern = @"(?<=\bREG/)[A-Z0-9]+";

        Match match = Regex.Match(Rmks, pattern);
        if (match.Success)
          ret = match.Value;
        else
          ret = null;

        return ret;
    }

    public DateTime GetDepartureDateTime()
    {
        DateTime now = DateTime.Now;
        int hrs = int.Parse(DepTime[..2]);
        int mns = int.Parse(DepTime[2..]);
        DateTime ret = new(now.Year, now.Month, now.Day, hrs, mns, 0);
        return ret;
    }
    public TimeSpan GetEnrouteTime() => new(HrsEnroute, MinEnroute, 0);
    public TimeSpan GetFuelDurationTime() => new(HrsFuel, MinFuel, 0);
  }
}
