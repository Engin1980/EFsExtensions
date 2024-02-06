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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.TTSs.ElevenLabs
{
  /// <summary>
  /// Interaction logic for CtrElevenLabsSettings.xaml
  /// </summary>
  public partial class CtrElevenLabsSettings : UserControl
  {
    public CtrElevenLabsSettings()
    {
      InitializeComponent();
      this.DataContext = VM;
      VM.PropertyChanged += VM_PropertyChanged;
      ResetTts();
    }

    private ElevenLabsTts tts = null!;
    public MainWindowVM VM
    {
      get { return (MainWindowVM)GetValue(VMProperty); }
      set { SetValue(VMProperty, value); }
    }

    public static readonly DependencyProperty VMProperty =
        DependencyProperty.Register(nameof(VM), typeof(MainWindowVM), typeof(CtrElevenLabsSettings), new(new MainWindowVM()));

    private void ResetTts()
    {
      ElevenLabsTtsSettings ttsSettings = new ElevenLabsTtsSettings();
      ttsSettings.API = VM.ApiKey;
      this.tts = new ElevenLabsTts(ttsSettings);
    }

    private void VM_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(MainWindowVM.ApiKey))
        ResetTts();
    }

    private async void btnReloadVoices_Click(object sender, RoutedEventArgs e)
    {
      btnReloadVoices.IsEnabled = false;
      try
      {
        this.VM.Voices = (await this.tts.GetVoicesAsync()).OrderBy(q => q.Name).ToList();
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
      }
      btnReloadVoices.IsEnabled = true;
    }
  }
}
