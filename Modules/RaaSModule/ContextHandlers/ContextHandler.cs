using ELogging;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.AudioPlaying;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.AudioPlaying;
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
    protected readonly Settings settings;
    private readonly static Synthetizer? synthetizer = Synthetizer.CreateDefault();

    protected ContextHandler(ContextHandlerArgs args)
    {
      this.logger = args.logger;
      this.data = args.data;
      this.raas = args.raas;
      this.simDataProvider = args.simDataProvider;
      this.settings = args.settings;
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
      EPlayer player = new(bytes);
      player.PlayAsynchronously();
    }

    protected void Say(RaasSpeech speech, RaasDistance candidateDistance)
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
      EPlayer player = new(bytes);
      player.PlayAsynchronously();
    }
  }
}
