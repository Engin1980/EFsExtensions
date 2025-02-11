using ELogging;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.Playing;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.Synthetization;
using Eng.Chlaot.Libs.AirportsLib;
using Eng.Chlaot.Modules.RaaSModule.Model;
using ESystem.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.Modules.RaaSModule.ContextHandlers
{
  internal abstract class ContextHandler
  {
    protected readonly Logger logger;
    protected readonly Context.RuntimeDataBox data;
    protected readonly Raas raas;
    protected Func<SimDataStruct> simDataProvider;
    private readonly static Synthetizer? synthetizer = Synthetizer.CreateDefault();

    protected ContextHandler(Logger logger, Context.RuntimeDataBox data, Raas raas, Func<SimDataStruct> simDataProvider)
    {
      this.logger = logger;
      this.data = data;
      this.raas = raas;
      this.simDataProvider = simDataProvider;
    }

    public abstract void Handle();

    protected void Say(RaasSpeech speech, RunwayThreshold threshold)
    {
      string d = string.Join(" ", threshold.Designator.ToArray());
      d = d.Replace("L", "Left").Replace("R", "Right").Replace("C", "Center");
      string s = speech.Speech.Replace("%rwy", d);

      logger.Log(LogLevel.INFO, "Saying: " + s);

      Debug.Assert(synthetizer != null);
      var bytes = synthetizer!.Generate(s);
      Player player = new(bytes);
      player.Play();
    }

    protected void Say(RaasDistancesSpeech speech, RaasDistance candidateDistance)
    {
      string s = speech.Speech;
      s = s.Replace("%dist", candidateDistance.Value + " " + candidateDistance.Unit switch
      {
        RaasDistance.RaasDistanceUnit.km => "kilometers",
        RaasDistance.RaasDistanceUnit.m => "meters",
        RaasDistance.RaasDistanceUnit.ft => "feet",
        RaasDistance.RaasDistanceUnit.nm => "miles",
        _ => throw new UnexpectedEnumValueException(candidateDistance.Unit)
      });

      var bytes = synthetizer!.Generate(s);
      Player player = new(bytes);
      player.Play();
    }
  }
}
