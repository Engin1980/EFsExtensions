using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.AudioPlaying;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.Synthetization;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.TTSs;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.TTSs.MsSapi;
using Eng.EFsExtensions.Modules.ChecklistModule;
using ESystem;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Eng.EFsExtensions.Modules.ChecklistModule
{
  /// <summary>
  /// Interaction logic for CtrSettings.xaml
  /// </summary>
  public partial class CtrSettings : Window
  {
    private const string AUDIO_CHANNEL_NAME = AudioPlayManager.CHANNEL_COPILOT;
    private readonly MsSapiModule msSapiModule = new();
    private readonly Settings settings;

    public CtrSettings()
    {
      InitializeComponent();
      this.settings = null!;
    }

    public CtrSettings(Settings settings) : this()
    {
      this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
      this.DataContext = settings;
    }

    [SuppressMessage("", "IDE1006")]
    private async void btnTestSynthetizer_Click(object sender, RoutedEventArgs e)
    {
      btnTestSynthetizer.IsEnabled = false;
      Task t = new(() =>
      {
        ChannelAudioPlayer cap = new();
        try
        {
          ITtsProvider provider = msSapiModule.GetProvider(settings.Synthetizer);
          var a = provider.Convert("Landing gear");
          var b = provider.Convert("Down, three green");

          a = AudioUtils.AppendSilence(a, settings.DelayAfterCall);
          b = AudioUtils.AppendSilence(b, settings.DelayAfterConfirmation);

          cap.PlayAndWait(a, AUDIO_CHANNEL_NAME);
          cap.PlayAndWait(b, AUDIO_CHANNEL_NAME);
        }
        catch (Exception ex)
        {
          throw new ApplicationException("Failed to generate or play.", ex);
        }
      });
      t.Start();
      await t;
      btnTestSynthetizer.IsEnabled = true;
    }

    private void Window_Closed(object sender, EventArgs e)
    {
      this.settings.Save();
    }
  }
}
