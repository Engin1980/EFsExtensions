using System;
using System.Collections.Generic;
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
    private Settings settings;
    public CtrSettings()
    {
      InitializeComponent();
    }

    public CtrSettings(Settings settings) : this()
    {
      this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
      this.DataContext = settings;
    }

    private void btnTestSynthetizer_Click(object sender, RoutedEventArgs e)
    {
      btnTestSynthetizer.IsEnabled = false;
      try
      {
        SpeechSynthesizer ss = new SpeechSynthesizer();
        ss.SelectVoice(settings.Synthetizer.Voice);
        ss.Rate = settings.Synthetizer.Rate;

        ss.Speak("Landing lights - on");
      }
      catch (Exception ex)
      {

      }
      finally
      {
        btnTestSynthetizer.IsEnabled = true;
      }
    }
  }
}
