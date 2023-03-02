using ChecklistModule.Support;
using ChlaotModuleBase.ModuleUtils.Synthetization;
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
    private void btnTestSynthetizer_Click(object sender, RoutedEventArgs e)
    {
      btnTestSynthetizer.IsEnabled = false;
      try
      {
        Synthetizer s = new(settings.Synthetizer);
        var a = s.Generate("Landing lights");
        var b = s.Generate("On");

        Player p = new();
        p.PlayAsync(a);
        p.PlayAsync(b);
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
