using Microsoft.FlightSimulator.SimConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESimConnect
{
  [AttributeUsage(AttributeTargets.Field)]
  public class SimConnectDataDefinitionAttribute : Attribute
  {
    public string SimPropertyName { get; set; }
    public string Unit { get; set; }
    public SIMCONNECT_DATATYPE  Type { get; set; }
  }
}
