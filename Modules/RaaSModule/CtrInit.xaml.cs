using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.Storable;
using Eng.EFsExtensions.Libs.AirportsLib;
using Eng.EFsExtensions.Modules.RaaSModule.CopilotModule;
using ESystem.Miscelaneous;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace Eng.EFsExtensions.Modules.RaaSModule
{
  /// <summary>
  /// Interaction logic for CtrInit.xaml
  /// </summary>
  public partial class CtrInit : UserControl
  {
    private class FilteredAirports : NotifyPropertyChanged
    {
      private List<Airport> baseAirports;
      public string FilterRegex
      {
        get { return base.GetProperty<string>(nameof(FilterRegex))!; }
        set
        {
          base.UpdateProperty(nameof(FilterRegex), value);
        }
      }

      public ObservableCollection<Airport> Airports
      {
        get => base.GetProperty<ObservableCollection<Airport>>(nameof(Airports))!;
        set => base.UpdateProperty(nameof(Airports), value);
      }

      public FilteredAirports()
      {
        this.baseAirports = new();
        this.PropertyChanged += (s, e) => { if (e.PropertyName == nameof(FilterRegex)) Update(); };
        Update();
      }

      public void SetBaseAirports(List<Airport> airports)
      {
        this.baseAirports = airports;
        Update();
      }

      private void Update()
      {
        if (FilterRegex == null || FilterRegex.Length == 0)
        {
          Airports = new ObservableCollection<Airport>(baseAirports);
        }
        else
        {
          Airports = new ObservableCollection<Airport>(baseAirports
            .Where(a => System.Text.RegularExpressions.Regex.IsMatch(a.ICAO, FilterRegex) ||
                        System.Text.RegularExpressions.Regex.IsMatch(a.Name, FilterRegex) ||
                        System.Text.RegularExpressions.Regex.IsMatch(a.City, FilterRegex) ||
                        System.Text.RegularExpressions.Regex.IsMatch(a.CountryCode, FilterRegex)));
        }
      }
    }

    private string? recentArportsXmlFile;
    private string?  recentRaasXmlFile;

    public CtrInit()
    {
      InitializeComponent();
    }

    private readonly Context context = null!;
    private readonly FilteredAirports filteredAirports = null!;
    internal CtrInit(Context context) : this()
    {
      this.DataContext = this.context = context;
      this.tabAirports.DataContext = this.filteredAirports = new();
    }

    private void btnLoadAirports_Click(object sender, RoutedEventArgs e)
    {
      var dialog = new CommonOpenFileDialog()
      {
        AddToMostRecentlyUsedList = true,
        EnsureFileExists = true,
        DefaultFileName = recentArportsXmlFile,
        Multiselect = false,
        Title = "Select XML file with airports data..."
      };
      dialog.Filters.Add(StorableUtils.CreateCommonFileDialogFilter("Airports files", "airports.xml"));
      dialog.Filters.Add(StorableUtils.CreateCommonFileDialogFilter("XML files", "xml"));
      dialog.Filters.Add(StorableUtils.CreateCommonFileDialogFilter("All files", "*"));
      if (dialog.ShowDialog() != CommonFileDialogResult.Ok || dialog.FileName == null) return;

      recentArportsXmlFile = dialog.FileName;
      this.context.LoadAirportsFile(recentArportsXmlFile);
      this.filteredAirports.SetBaseAirports(this.context.Airports);
      this.txtAirportsCount.Text = $" ({this.context.Airports.Count})";
    }

    private void btnSettings_Click(object sender, RoutedEventArgs e)
    {
      new CtrSettings(context.Settings).ShowDialog();
    }

    private void btnLoadRaas_Click(object sender, RoutedEventArgs e)
    {
      var dialog = new CommonOpenFileDialog()
      {
        AddToMostRecentlyUsedList = true,
        EnsureFileExists = true,
        DefaultFileName = recentRaasXmlFile,
        Multiselect = false,
        Title = "Select XML file with RaaS data..."
      };
      dialog.Filters.Add(StorableUtils.CreateCommonFileDialogFilter("RaaS files", "raas.xml"));
      dialog.Filters.Add(StorableUtils.CreateCommonFileDialogFilter("XML files", "xml"));
      dialog.Filters.Add(StorableUtils.CreateCommonFileDialogFilter("All files", "*"));
      if (dialog.ShowDialog() != CommonFileDialogResult.Ok || dialog.FileName == null) return;

      recentRaasXmlFile = dialog.FileName;
      this.context.LoadRaasFile(recentRaasXmlFile);
    }
  }
}
