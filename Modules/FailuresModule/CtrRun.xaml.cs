using Eng.Chlaot.Modules.FailuresModule.Model.Incidents;
using Eng.Chlaot.Modules.FailuresModule.Model.Failures;
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
using Eng.Chlaot.Modules.FailuresModule.Converters;
using Eng.Chlaot.Modules.FailuresModule.Model.VMs;
using Eng.Chlaot.Modules.FailuresModule.Model.Sustainers;

namespace Eng.Chlaot.Modules.FailuresModule
{
    /// <summary>
    /// Interaction logic for CtrRun.xaml
    /// </summary>
    public partial class CtrRun : UserControl
  {
    private RunContext context = null!;

    public CtrRun()
    {
      InitializeComponent();
    }

    public CtrRun(RunContext context) : this()
    {
      this.context = context;
      this.DataContext = context;
      FailureDefinitionActiveToBoolConverter.SetActiveSustainers(context.Sustainers);
    }

    private void btnToggleSustainerActive_Click(object sender, RoutedEventArgs e)
    {
      Button btn = (Button)sender;
      FailureDefinition fd = (FailureDefinition)btn.Tag;
      FailureSustainer? fs = context.Sustainers.FirstOrDefault(q => q.Failure.Equals(fd));
      if (fs != null)
      {
        fs.Reset();
        context.Sustainers.Remove(fs);
      }
      else
      {
        fs = FailureSustainerFactory.Create(fd);
        context.Sustainers.Add(fs);
        fs.Start();
      }
    }

    private void btnFireIncident_Click(object sender, RoutedEventArgs e)
    {
      Button btn = (Button)sender;
      IncidentDefinitionVM runIncidentDefinition = (IncidentDefinitionVM)btn.Tag;
      this.context.FireIncidentDefinition(runIncidentDefinition);
    }

    private void btnFireFailure_Click(object sender, RoutedEventArgs e)
    {
      Button btn = (Button)sender;
      FailId f = (FailId)btn.Tag;
      context.FireFail(f);
    }

    private void btnFailureCancel_Click(object sender, RoutedEventArgs e)
    {
      Button btn = (Button)sender;
      FailureSustainer fs = (FailureSustainer)btn.Tag;
      context.CancelFailure(fs);
    }

  }
}
