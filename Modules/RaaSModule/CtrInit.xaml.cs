using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.Storable;
using Eng.Chlaot.Libs.AirportsLib;
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

namespace Eng.Chlaot.Modules.RaaSModule
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

    private string? recentXmlFile;

    public CtrInit()
    {
      InitializeComponent();
    }

    private readonly Context context = null!;
    private readonly FilteredAirports filteredAirports = null!;
    private readonly Action setReadyCallback;
    internal CtrInit(Context context, Action setReadyCallback) : this()
    {
      this.DataContext = this.context = context;
      this.tabAirports.DataContext = this.filteredAirports = new();
      this.setReadyCallback = setReadyCallback;
    }

    private void btnLoadAirports_Click(object sender, RoutedEventArgs e)
    {
      var dialog = new CommonOpenFileDialog()
      {
        AddToMostRecentlyUsedList = true,
        EnsureFileExists = true,
        DefaultFileName = recentXmlFile,
        Multiselect = false,
        Title = "Select XML file with airports data..."
      };
      dialog.Filters.Add(StorableUtils.CreateCommonFileDialogFilter("Airports files", "airports.xml"));
      dialog.Filters.Add(StorableUtils.CreateCommonFileDialogFilter("XML files", "xml"));
      dialog.Filters.Add(StorableUtils.CreateCommonFileDialogFilter("All files", "*"));
      if (dialog.ShowDialog() != CommonFileDialogResult.Ok || dialog.FileName == null) return;

      recentXmlFile = dialog.FileName;
      this.context.LoadFile(recentXmlFile);
      this.filteredAirports.SetBaseAirports(this.context.Airports);
      this.txtAirportsCount.Text = $" ({this.context.Airports.Count})";
      this.setReadyCallback();
    }

    private void btnSettings_Click(object sender, RoutedEventArgs e)
    {
      //TODO: Implement settings
    }
  }
}
