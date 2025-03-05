using ESystem.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.RaaSModule.Model
{
  public class Raas
  {
    public RaasVariables Variables { get; set; }
    public RaasSpeeches Speeches { get; set; }

    internal void CheckSanity()
    {
      if (Speeches == null) throw new ApplicationException("Raas.Speeches are null");
      if (Variables == null) throw new ApplicationException("Raas.Variables are null");

      if (Speeches.TaxiToRunway == null) throw new ApplicationException("Raas.Speeches.TaxiToRunway are null");
      if (Speeches.TaxiToShortRunway == null) throw new ApplicationException("Raas.Speeches.TaxiToShortRunway are null");
      if (Speeches.OnRunway == null) throw new ApplicationException("Raas.Speeches.OnRunway are null");
      if (Speeches.OnShortRunway == null) throw new ApplicationException("Raas.Speeches.OnShortRunway are null");
      if (Speeches.LandingRunway == null) throw new ApplicationException("Raas.Speeches.LandingRunway are null");
      if (Speeches.DistanceRemaining == null) throw new ApplicationException("Raas.Speeches.DistanceRemaining are null");
      Speeches.TaxiToRunway.CheckSanity();
      Speeches.TaxiToShortRunway.CheckSanity();
      Speeches.OnRunway.CheckSanity();
      Speeches.OnShortRunway.CheckSanity();
      Speeches.LandingRunway.CheckSanity();
      Speeches.DistanceRemaining.CheckSanity();

      Variables.CheckSanity();
    }
  }
}
