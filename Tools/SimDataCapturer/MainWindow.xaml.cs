using System.ComponentModel;
using System.Data;
using System.Security.Policy;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SimDataCapturer
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private readonly ModelVM model = new ModelVM();

    private readonly SimConManager simConManager = new SimConManager();

    private SimDataRecorder? simDataRecorder = null;

    public MainWindow()
    {
      InitializeComponent();
      model.FileName = "E:\\default.fsr";
      model.RawPlaneData = new();
      this.DataContext = model;
      simConManager.OnData += SimConManager_OnData;
      simConManager.OnSecondElapsed += SimConManager_OnSecondElapsed;
    }

    private void btnOpen_Click(object sender, RoutedEventArgs e)
    {
      simConManager.Open();
      this.model.IsRunning = true;
    }

    private void btnStart_Click(object sender, RoutedEventArgs e)
    {
      lock (this)
      {
        this.simDataRecorder = new SimDataRecorder(model.FileName);
        this.model.IsRecording = true;
      }
    }

    private void btnStop_Click(object sender, RoutedEventArgs e)
    {
      lock (this)
      {
        this.simDataRecorder = null;
        this.model.IsRecording = false;
      }
    }

    private void SimConManager_OnData(MockPlaneData data)
    {
      this.model.RawPlaneData = data;
    }

    private void SimConManager_OnSecondElapsed()
    {
      SimDataRecorder? sdr;
      lock (this)
      {
        sdr = this.simDataRecorder;
      }
      if (sdr == null) return;

      sdr.SaveData(this.model.RawPlaneData);
    }

    public class ModelVM : NotifyPropertyChangedBase
    {
      public string FileName
      {
        get => base.GetProperty<string>(nameof(FileName))!;
        set => base.UpdateProperty(nameof(FileName), value);
      }
      public bool IsRecording
      {
        get => base.GetProperty<bool>(nameof(IsRecording))!;
        set => base.UpdateProperty(nameof(IsRecording), value);
      }
      public bool IsRunning
      {
        get => base.GetProperty<bool>(nameof(IsRunning))!;
        set => base.UpdateProperty(nameof(IsRunning), value);
      }
      public MockPlaneData RawPlaneData
      {
        get => base.GetProperty<MockPlaneData>(nameof(RawPlaneData))!;
        set
        {
          base.UpdateProperty(nameof(RawPlaneData), value);
          RebuildTablePlaneData();
        }
      }
      public DataView TablePlaneData
      {
        get => base.GetProperty<DataView>(nameof(TablePlaneData))!;
        set => base.UpdateProperty(nameof(TablePlaneData), value);
      }

      private void RebuildTablePlaneData()
      {
        DataTable dt = new DataTable();
        dt.Columns.Add("Name", typeof(string));
        dt.Columns.Add("Value", typeof(string));

        var fields = RawPlaneData.GetType().GetFields();
        foreach (var field in fields)
        {
          var row = dt.NewRow();
          row["Name"] = field.Name;
          row["Value"] = field.GetValue(RawPlaneData)?.ToString() ?? "(null)";
          dt.Rows.Add(row);
        }

        DataView dv = new(dt)
        {
          AllowDelete = false,
          AllowEdit = false,
          AllowNew = false
        };

        this.TablePlaneData = dv;
      }
    }

    private void btnFailEngine(object sender, RoutedEventArgs e)
    {
      simConManager.FailEngine();
    }

    private void btnSimVar_Click(object sender, RoutedEventArgs e)
    {
      simConManager.FailEngineFire();
    }

    private void btnLeak_Click(object sender, RoutedEventArgs e)
    {
      simConManager.FailLeak();
    }

    private void btnStuck_Click(object sender, RoutedEventArgs e)
    {
      simConManager.FailStuck();
    }

    private void btnExternalEvent_Click(object sender, RoutedEventArgs e)
    {
      simConManager.TestExternal();
    }

    private void btnExternalSet_Click(object sender, RoutedEventArgs e)
    {
      simConManager.TestExternalSet();
    }
  }
}