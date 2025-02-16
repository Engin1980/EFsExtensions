using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.Playing;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.Storable;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.Synthetization;
using Microsoft.WindowsAPICodePack.Dialogs;
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

namespace Eng.Chlaot.Modules.RaaSModule.CopilotModule
{
  /// <summary>
  /// Interaction logic for CtrSettings.xaml
  /// </summary>
  public partial class CtrSettings : Window
  {
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

        autoPlaybackManager.Enqueue(a);
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

    private void btnAirportsFile_Click(object sender, RoutedEventArgs e)
    {
      var dialog = new CommonOpenFileDialog()
      {
        AddToMostRecentlyUsedList = true,
        EnsureFileExists = true,
        Multiselect = false,
        Title = "Select XML file with airports data..."
      };
      dialog.Filters.Add(StorableUtils.CreateCommonFileDialogFilter("Airports files", "airports.xml"));
      dialog.Filters.Add(StorableUtils.CreateCommonFileDialogFilter("XML files", "xml"));
      dialog.Filters.Add(StorableUtils.CreateCommonFileDialogFilter("All files", "*"));
      if (dialog.ShowDialog() != CommonFileDialogResult.Ok || dialog.FileName == null) return;

      settings.AutoLoadedAirportsFile = dialog.FileName;
      this.Focus();
    }

    private void btnAirportsFileDelete_Click(object sender, RoutedEventArgs e)
    {
      settings.AutoLoadedAirportsFile = null;
    }

    private void btnRaasFile_Click(object sender, RoutedEventArgs e)
    {
      var dialog = new CommonOpenFileDialog()
      {
        AddToMostRecentlyUsedList = true,
        EnsureFileExists = true,
        Multiselect = false,
        Title = "Select XML file with RaaS data..."
      };
      dialog.Filters.Add(StorableUtils.CreateCommonFileDialogFilter("RaaS files", "raas.xml"));
      dialog.Filters.Add(StorableUtils.CreateCommonFileDialogFilter("XML files", "xml"));
      dialog.Filters.Add(StorableUtils.CreateCommonFileDialogFilter("All files", "*"));
      if (dialog.ShowDialog() != CommonFileDialogResult.Ok || dialog.FileName == null) return;

      settings.AutoLoadedRaasFile = dialog.FileName;
      this.Focus();
    }

    private void btnRaasFileDelete_Click(object sender, RoutedEventArgs e)
    {
      settings.AutoLoadedRaasFile = null;
    }
  }
}
