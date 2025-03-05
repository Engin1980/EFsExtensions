using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.AudioPlaying;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.Synthetization;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.TTSs;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.TTSs.MsSapi;
using Eng.EFsExtensions.Modules.ChecklistModule;
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

namespace ChecklistModule
{
  /// <summary>
  /// Interaction logic for CtrSettings.xaml
  /// </summary>
  public partial class CtrSettings : Window
  {
    private const string AUDIO_CHANNEL_NAME = AudioPlayManager.CHANNEL_COPILOT;
    private readonly MsSapiModule msSapiModule = new();
    private readonly Settings settings;
    private readonly AudioPlayManager autoPlaybackManager = new();

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
        bool isCompleted = false;
        void callback(AudioPlayManager sender, AudioPlayManager.ChannelEventArgs e)
        {
          isCompleted = true;
        }
        autoPlaybackManager.ChannelPlayCompleted += callback;
        try
        {
          ITtsProvider provider = msSapiModule.GetProvider(settings.Synthetizer);
          var a = provider.Convert("Landing gear");
          var b = provider.Convert("Down, three green");

          a = AudioUtils.AppendSilence(a, settings.DelayAfterCall);
          a = AudioUtils.AppendSilence(a, settings.DelayAfterConfirmation);

          autoPlaybackManager.Enqueue(a, AUDIO_CHANNEL_NAME);
          autoPlaybackManager.Enqueue(b, AUDIO_CHANNEL_NAME);

          while (!isCompleted)
            System.Threading.Thread.Sleep(100);
        }
        catch (Exception ex)
        {
          throw new ApplicationException("Failed to generate or play.", ex);
        }
        finally
        {
          autoPlaybackManager.ChannelPlayCompleted -= callback;
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
