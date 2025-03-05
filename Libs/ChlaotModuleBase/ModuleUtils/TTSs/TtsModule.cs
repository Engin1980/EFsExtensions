using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.TTSs
{
  public abstract class TtsModule<TtsProviderType, TtsSettingsType> : ITtsModule
    where TtsSettingsType : ITtsSettings, new()
    where TtsProviderType : ITtsProvider
  {
    public abstract string Name { get; }

    public virtual ITtsSettings GetDefaultSettings() => new TtsSettingsType();
    public ITtsProvider GetProvider(ITtsSettings settings)
    {
      CheckSettingsType(settings);
      ITtsProvider ret = GetTypedProvider((TtsSettingsType)settings);
      return ret;
    }

    protected abstract TtsProviderType GetTypedProvider(TtsSettingsType settings);
    protected abstract UserControl GetTypedSettingsControl(TtsSettingsType settings);

    public UserControl GetSettingsControl(ITtsSettings settings)
    {
      CheckSettingsType(settings);
      UserControl ret = GetTypedSettingsControl((TtsSettingsType)settings);
      return ret;
    }

    private static void CheckSettingsType(ITtsSettings settings)
    {
      if (settings == null)
        throw new TtsApplicationException(
          $"Settings are expected to be not null and of type '{typeof(TtsSettingsType).Name}'.");
      if (settings is not TtsSettingsType)
      {
        throw new TtsApplicationException(
          $"Settings are expected to be of type '{typeof(TtsSettingsType).Name}' (provided: '{settings.GetType().Name}')");
      }
    }
  }
}
