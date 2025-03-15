using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using Eng.EfsExtensions.Modules.FlightLogModule;
using Eng.EfsExtensions.Modules.FlightLogModule.Navdata;
using ESystem.Miscelaneous;

namespace Eng.EfsExtensions.Modules.FlightLogModule
{
  /// <summary>
  /// Interaction logic for CtrInit.xaml
  /// </summary>
  public partial class CtrInit : UserControl
  {
    public class FilteredNavdata : NotifyPropertyChanged
    {
      private readonly List<Airport> allAirports;
      public FilteredNavdata(List<Airport> airportsList)
      {
        this.allAirports = airportsList;
        Filter = new();
        Filter.FilterUpdated += () => RefreshList();
        AirportList = new();
        RefreshList();
      }

      private void RefreshList()
      {
        this.AirportList.Clear();
        var filtered = Filter.Text.Length == 0
          ? this.allAirports
          : Filter.IcaoOnly
            ? this.allAirports.Where(q => q.ICAO.Contains(Filter.Text)).ToList()
            : this.allAirports.Where(q => q.Name.Contains(Filter.Text) || q.ICAO.Contains(Filter.Text) || q.City.Contains(Filter.Text)).ToList();
        filtered.ForEach(q => this.AirportList.Add(q));
      }

      public ObservableCollection<Airport> AirportList
      {
        get { return base.GetProperty<ObservableCollection<Airport>>(nameof(AirportList))!; }
        set { base.UpdateProperty(nameof(AirportList), value); }
      }

      public NavdataFilter Filter
      {
        get { return base.GetProperty<NavdataFilter>(nameof(Filter))!; }
        set { base.UpdateProperty(nameof(Filter), value); }
      }
    }
    public class NavdataFilter : NotifyPropertyChanged
    {
      public delegate void FilterUpdatedDelegate();
      public event FilterUpdatedDelegate? FilterUpdated;
      public NavdataFilter()
      {
        Text = "";
        IcaoOnly = true;
      }
      public string Text
      {
        get { return base.GetProperty<string>(nameof(Text))!; }
        set { base.UpdateProperty(nameof(Text), value); FilterUpdated?.Invoke(); }
      }

      public bool IcaoOnly
      {
        get { return base.GetProperty<bool>(nameof(IcaoOnly))!; }
        set { base.UpdateProperty(nameof(IcaoOnly), value); FilterUpdated?.Invoke(); }
      }
    }

    private readonly Context Context = null!;
    private readonly FilteredNavdata Filtered;
    private bool isInitializing = false;

    public CtrInit()
    {
      isInitializing = true;
      InitializeComponent();
      isInitializing = false;
      Filtered = null!;
    }

    public CtrInit(Context context) : this()
    {
      this.Context = context;
      this.DataContext = context;
      this.pnlFilteredAirportList.DataContext = Filtered = new FilteredNavdata(context.AirportsList);
    }

    private void tabMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (tabMain.SelectedIndex == 1)
        lblTakeLong.Visibility = Visibility.Collapsed;
    }

    private void cmbProfile_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (isInitializing) return;
      Context.IsReady = cmbProfile.SelectedIndex != 0;
    }
  }
}
