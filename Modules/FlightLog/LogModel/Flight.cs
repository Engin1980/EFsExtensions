using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.Modules.FlightLogModule.LogModel
{
  internal record StartUp(DateTime Time);
  internal record TakeOff(DateTime Time);
  internal record Landing(DateTime Time);
  internal record ShutDown(DateTime Time);
  internal record Flight(string? DepartureICAO, string? DestinationICAO, StartUp StartUp, TakeOff TakeOff, Landing Landing, ShutDown ShutDown);
}
