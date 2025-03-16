using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.FlightLogModule.VatsimModel
{
  public record FlightPlan(
    long Id,
    long ConnectionId,
    string VatsimId,
    string FlightType,
    string Callsign,
    string Aircraft,
    string CruiseSpeed,
    string Dep,
    string Arr,
    string Alt,
    string Altitude,
    string Rmks,
    string Route,
    string DeptTime,
    int HrsEnroute,
    int MinEnroute,
    int HrsFuel,
    int MinFuel,
    string Filed,
    string AssignedSquawk,
    string ModifiedByCid,
    string ModifiedByCallsign
  );
}
