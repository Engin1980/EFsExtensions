using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.TTSs.MSAPI
{
  public class MSapiModule : TtsModule<MSapiProvider, MSapiSettings>
  {
    public override string Name => "Microsoft SAPI";

    protected override MSapiProvider GetTypedProvider(MSapiSettings settings) => new MSapiProvider(settings);

    protected override UserControl GetTypedSettingsControl(MSapiSettings settings) => new CtrSettings(settings);
  }
}
