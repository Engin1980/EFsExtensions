using Microsoft.FlightSimulator.SimConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESimConnect
{
  [AttributeUsage(AttributeTargets.Field)]
  public class DataDefinitionAttribute : Attribute
  {
    public DataDefinitionAttribute(string simVarName, string? unit = null, SIMCONNECT_DATATYPE type = SIMCONNECT_DATATYPE.FLOAT64) {
      this.Name = simVarName;
      this.Unit = unit;
      this.Type= type;
    }
    public string Name { get; }
    public string? Unit { get; }
    public SIMCONNECT_DATATYPE Type { get; }
  }
}
