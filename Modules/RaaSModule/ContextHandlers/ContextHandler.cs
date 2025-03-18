using ESystem.Logging;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.AudioPlaying;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.TTSs;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.TTSs.MsSapi;
using Eng.EFsExtensions.Libs.AirportsLib;
using Eng.EFsExtensions.Modules.RaaSModule.Model;
using ESystem.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.RaaSModule.ContextHandlers
{
  internal abstract class ContextHandler
  {
    protected readonly Logger logger;
    protected readonly Context.RuntimeDataBox data;
    protected readonly Func<SimDataSnapshot> simDataSnapshotProvider;
    protected readonly Raas raas;
    protected readonly Settings settings;
    private readonly ITtsProvider? synthetizer;

    protected ContextHandler(ContextHandlerArgs args)
    {
      this.logger = args.logger;
      this.data = args.data;
      this.raas = args.raas;
      this.simDataSnapshotProvider = args.simDataSnapshotProvider;
      this.settings = args.settings;
      this.synthetizer = new MsSapiModule().GetProvider(this.settings.Synthetizer);
    }

    public abstract void Handle();

    protected void Say(RaasSpeech speech, RunwayThreshold threshold)
    {
      string d = string.Join(" ", threshold.Designator.ToArray());
      d = d.Replace("L", "Left").Replace("R", "Right").Replace("C", "Center");
      string s = speech.Speech.Replace("%rwy", d);

      logger.Log(LogLevel.INFO, "Saying: " + s);

      Debug.Assert(synthetizer != null);
      var bytes = synthetizer!.Convert(s);
      AudioPlayer player = new(bytes);
      player.PlayAsync();
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

      var bytes = synthetizer!.Convert(s);
      AudioPlayer player = new(bytes);
      player.PlayAsync();
    }
  }
}
