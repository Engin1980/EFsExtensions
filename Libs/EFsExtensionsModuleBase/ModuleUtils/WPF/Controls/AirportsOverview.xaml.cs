using Eng.EFsExtensions.Libs.AirportsLib;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.WPF.Controls
{
  /// <summary>
  /// Interaction logic for AirportsOverview.xaml
  /// </summary>
  public partial class AirportsOverview : UserControl
  {
    private static readonly DependencyProperty AirportsProperty = DependencyProperty.Register(
      nameof(Airports), typeof(List<Airport>), typeof(AirportsOverview), new PropertyMetadata(null, OnPropertyChanged));

    private static readonly DependencyProperty VisibleAirportsProperty = DependencyProperty.Register(
      nameof(VisibleAirports), typeof(List<Airport>), typeof(AirportsOverview));

    private static readonly DependencyProperty FilterRegexProperty = DependencyProperty.Register(
      nameof(FilterRegex), typeof(string), typeof(AirportsOverview), new PropertyMetadata(null, OnPropertyChanged));

    public string? FilterRegex
    {
      get => (string?)GetValue(FilterRegexProperty);
      set => SetValue(FilterRegexProperty, value);
    }

    public List<Airport> Airports
    {
      get => (List<Airport>)GetValue(AirportsProperty);
      set => SetValue(AirportsProperty, value);
    }

    public List<Airport> VisibleAirports
    {
      get => (List<Airport>)GetValue(VisibleAirportsProperty);
      set => SetValue(VisibleAirportsProperty, value);
    }

    public AirportsOverview()
    {
      InitializeComponent();
    }

    private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (d is AirportsOverview ao)
        ao.UpdateVisibleList();
    }

    private void UpdateVisibleList()
    {
      if (Airports == null || Airports.Count == 0)
        VisibleAirports = new List<Airport>();
      else if (FilterRegex == null || FilterRegex.Length == 0)
        VisibleAirports = Airports ?? new List<Airport>();
      else
      {
        string fs = this.FilterRegex ?? "";
        var tmp = Airports.Where(q => Regex.IsMatch(q.ICAO, fs) || Regex.IsMatch(q.Name, fs) || Regex.IsMatch(q.City, fs)).ToList();
        VisibleAirports = tmp;
      }
    }
  }
}
