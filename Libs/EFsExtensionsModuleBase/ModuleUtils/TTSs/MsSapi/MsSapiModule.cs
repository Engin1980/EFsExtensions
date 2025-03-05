using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.TTSs.MsSapi
{
  public class MsSapiModule : TtsModule<MsSapiProvider, MsSapiSettings>
  {
    public override string Name => "Microsoft SAPI";

    protected override MsSapiProvider GetTypedProvider(MsSapiSettings settings) => new MsSapiProvider(settings);

    protected override UserControl GetTypedSettingsControl(MsSapiSettings settings) => new CtrSettings(settings);
  }
}
