using System;
using System.Collections.Generic;
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
    public Settings Settings { get; private set; }

    public CtrSettings()
    {
      InitializeComponent();
      Settings = new Settings();
    }

    public CtrSettings(Settings settings) : base()
    {
      this.Settings = settings;
    }
  }
}
