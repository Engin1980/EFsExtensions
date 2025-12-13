using Eng.EFsExtensions.Modules.FlightLogModule.LogModel;
using Eng.EFsExtensions.Modules.FlightLogModule.Models.LogModel;
using ESystem;
using ESystem.Miscelaneous;
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

namespace Eng.EFsExtensions.Modules.FlightLogModule.Controls.Shared
{
  /// <summary>
  /// Interaction logic for CtrFlightLogFilter.xaml
  /// </summary>
  public partial class FlightLogFilter : UserControl
  {
    public class FlightLogFilterViewModel : NotifyPropertyChanged
    {
      public List<string> Models
      {
        get => base.GetProperty<List<string>>(nameof(Models)) ?? [];
        set => base.UpdateProperty<List<string>>(nameof(Models), value);
      }

      public List<string> Registrations
      {
        get => base.GetProperty<List<string>>(nameof(Registrations)) ?? [];
        set => base.UpdateProperty<List<string>>(nameof(Registrations), value);
      }

      public string? Model
      {
        get => base.GetProperty<string>(nameof(Model));
        set => base.UpdateProperty<string>(nameof(Model), value);
      }
      public string? Registration
      {
        get => base.GetProperty<string?>(nameof(Registration));
        set => base.UpdateProperty<string?>(nameof(Registration), value);
      }
      public string? DepartureICAO
      {
        get => base.GetProperty<string?>(nameof(DepartureICAO));
        set => base.UpdateProperty<string?>(nameof(DepartureICAO), value);
      }
      public string? ArrivalICAO
      {
        get => base.GetProperty<string?>(nameof(ArrivalICAO));
        set => base.UpdateProperty<string?>(nameof(ArrivalICAO), value);
      }
      public DateTime? FromDate
      {
        get => base.GetProperty<DateTime?>(nameof(FromDate));
        set => base.UpdateProperty<DateTime?>(nameof(FromDate), value);
      }
      public DateTime? ToDate
      {
        get => base.GetProperty<DateTime?>(nameof(ToDate));
        set => base.UpdateProperty<DateTime?>(nameof(ToDate), value);
      }
    }

    public event Action? FilterChanged;

    private readonly FlightLogFilterViewModel vm;

    public FlightLogFilter()
    {
      InitializeComponent();
      this.plnMain.DataContext = this.vm = new();
    }

    public void SetUpFilter(List<LoggedFlight> flighs)
    {
      this.vm.Models = flighs
        .Where(q => q.AircraftModel != null)
        .Select(q => q.AircraftModel!)
        .Distinct()
        .ToList()
        .Tap(q => q.Insert(0, ""));

      this.vm.Registrations = flighs
        .Where(q => q.AircraftRegistration != null)
        .Select(q => q.AircraftRegistration!)
        .Distinct()
        .ToList()
        .Tap(q => q.Insert(0, ""));
    }

    public List<LoggedFlight> ApplyFilter(List<LoggedFlight> flights)
    {
      var query = flights.AsQueryable();
      if (!string.IsNullOrWhiteSpace(this.vm.Model))
      {
        query = query.Where(f => f.AircraftModel != null && f.AircraftModel.Contains(this.vm.Model!));
      }
      if (!string.IsNullOrWhiteSpace(this.vm.Registration))
      {
        query = query.Where(f => f.AircraftRegistration != null && f.AircraftRegistration.Contains(this.vm.Registration!));
      }
      if (!string.IsNullOrWhiteSpace(this.vm.DepartureICAO))
      {
        query = query.Where(f => f.DepartureICAO != null && f.DepartureICAO.Contains(this.vm.DepartureICAO!));
      }
      if (!string.IsNullOrWhiteSpace(this.vm.ArrivalICAO))
      {
        query = query.Where(f => f.DestinationICAO != null && f.DestinationICAO.Contains(this.vm.ArrivalICAO!));
      }
      if (this.vm.FromDate.HasValue)
      {
        query = query.Where(f => f.TakeOffScheduledDateTime >= this.vm.FromDate.Value);
      }
      if (this.vm.ToDate.HasValue)
      {
        query = query.Where(f => f.LandingScheduledDateTime <= this.vm.ToDate.Value);
      }
      return query.ToList();
    }

    private void btnApply_Click(object sender, RoutedEventArgs e)
    {
      this.FilterChanged?.Invoke();
    }
  }
}
