using Eng.Chlaot.ChlaotModuleBase.ModuleUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FailuresModule.Types
{
  public class FailureSet
  {
    public MetaInfo MetaInfo { get; set; }
    public List<Incident> Incidents { get; set; }
  }
}
