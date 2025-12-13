using Eng.EFsExtensions.EFsExtensionsModuleBase;
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
      private void UpdatePropertyAndChangedFlag<T>(string propertyName, T value)
      {
        base.UpdateProperty<T>(propertyName, value);
        base.UpdateProperty<bool>(propertyName + "Changed", true);
      }

      public List<string> Models
      {
        get => base.GetProperty<List<string>>(nameof(Models)) ?? [];
        set => this.UpdatePropertyAndChangedFlag<List<string>>(nameof(Models), value);
      }

      public bool ModelsChanged
      {
        get => base.GetProperty<bool?>(nameof(ModelsChanged)) ?? false;
      }

      public List<string> Registrations
      {
        get => base.GetProperty<List<string>>(nameof(Registrations)) ?? [];
        set => this.UpdatePropertyAndChangedFlag<List<string>>(nameof(Registrations), value);
      }

      public bool RegistrationsChanged
      {
        get => base.GetProperty<bool?>(nameof(RegistrationsChanged)) ?? false;
      }

      public string? Model
      {
        get => base.GetProperty<string>(nameof(Model));
        set => this.UpdatePropertyAndChangedFlag<string?>(nameof(Model), value);
      }

      public bool ModelChanged
      {
        get => base.GetProperty<bool?>(nameof(ModelChanged)) ?? false;
      }

      public string? Registration
      {
        get => base.GetProperty<string?>(nameof(Registration));
        set => this.UpdatePropertyAndChangedFlag<string?>(nameof(Registration), value);
      }

      public bool RegistrationChanged
      {
        get => base.GetProperty<bool?>(nameof(RegistrationChanged)) ?? false;
      }

      public string? DepartureICAO
      {
        get => base.GetProperty<string?>(nameof(DepartureICAO));
        set => this.UpdatePropertyAndChangedFlag<string?>(nameof(DepartureICAO), value);
      }

      public bool DepartureICAOChanged
      {
        get => base.GetProperty<bool?>(nameof(DepartureICAOChanged)) ?? false;
      }

      public string? DestinationICAO
      {
        get => base.GetProperty<string?>(nameof(DestinationICAO));
        set => this.UpdatePropertyAndChangedFlag<string?>(nameof(DestinationICAO), value);
      }

      public bool DestinationICAOChanged
      {
        get => base.GetProperty<bool?>(nameof(DestinationICAOChanged)) ?? false;
      }

      public DateTime? FromDate
      {
        get => base.GetProperty<DateTime?>(nameof(FromDate));
        set => this.UpdatePropertyAndChangedFlag<DateTime?>(nameof(FromDate), value);
      }

      public bool FromDateChanged
      {
        get => base.GetProperty<bool?>(nameof(FromDateChanged)) ?? false;
      }

      public DateTime? ToDate
      {
        get => base.GetProperty<DateTime?>(nameof(ToDate));
        set => this.UpdatePropertyAndChangedFlag<DateTime?>(nameof(ToDate), value);
      }

      public bool ToDateChanged
      {
        get => base.GetProperty<bool?>(nameof(ToDateChanged)) ?? false;
      }

      public void ResetChangedFlags()
      {
        try
        {
          this.GetType()
            .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Where(q => q.Name.EndsWith("Changed"))
            .ForEach(q => base.UpdateProperty<bool>(q.Name, false));
        }
        catch (Exception ex)
        {
          throw new ApplicationException("Error resetting changed flags in FlightLogFilterViewModel.", ex);
        }
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
      if (!string.IsNullOrWhiteSpace(this.vm.DestinationICAO))
      {
        query = query.Where(f => f.DestinationICAO != null && f.DestinationICAO.Contains(this.vm.DestinationICAO!));
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
      this.vm.ResetChangedFlags();
      this.FilterChanged?.Invoke();
    }
  }
}
