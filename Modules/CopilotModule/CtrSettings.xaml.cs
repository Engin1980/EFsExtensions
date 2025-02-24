using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.AudioPlaying;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.Synthetization;
using Eng.Chlaot.Modules.CopilotModule;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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

namespace CopilotModule
{
  /// <summary>
  /// Interaction logic for CtrSettings.xaml
  /// </summary>
  public partial class CtrSettings : Window
  {
    private const string AUDIO_CHANNEL_NAME = AutoPlaybackManager.CHANNEL_COPILOT;
    private readonly Settings settings;
    private readonly AutoPlaybackManager autoPlaybackManager = new AutoPlaybackManager();

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
    private void btnTestSynthetizer_Click(object sender, RoutedEventArgs e)
    {
      btnTestSynthetizer.IsEnabled = false;
      try
      {
        Synthetizer s = new(settings.Synthetizer);
        var a = s.Generate("Transition level");

        autoPlaybackManager.Enqueue(a, AUDIO_CHANNEL_NAME);
      }
      catch (Exception ex)
      {
        throw new ApplicationException("Failed to generate or play.", ex);
      }
      finally
      {
        btnTestSynthetizer.IsEnabled = true;
      }
    }

    private void Window_Closed(object sender, EventArgs e)
    {
      this.settings.Save();
    }
  }
}
