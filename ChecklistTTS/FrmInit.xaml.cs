using ELogging;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.SimObjects;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.Storable;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.TTSs;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.TTSs.ElevenLabs;
using EXmlLib;
using Microsoft.WindowsAPICodePack.Dialogs;
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
using System.Xml.Linq;
using Eng.Chlaot.Modules.ChecklistModule.Types;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.IO;
using System.Windows.Media.TextFormatting;

namespace ChecklistTTSNew
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class FrmInit : Window
  {
    private string recentXmlFile = string.Empty;
    private InitVM vm;

    public FrmInit()
    {
      InitializeComponent();
      try
      {
        LoadVM();
      }
      catch (Exception ex)
      {
        this.DataContext = this.vm = new InitVM();
      }

      var ttsModules = GetTtsModules();
      this.ctrTtss.Init(ttsModules);
    }

    private IEnumerable<ITtsModule> GetTtsModules()
    {
      List<ITtsModule> ret = new()
      {
        new ElevenLabsTtsModule()
      };
      return ret;
    }

    private void btnSelectChecklistFile_Click(object sender, RoutedEventArgs e)
    {
      var dialog = new CommonOpenFileDialog()
      {
        AddToMostRecentlyUsedList = true,
        EnsureFileExists = true,
        DefaultFileName = recentXmlFile,
        Multiselect = false,
        Title = "Select XML file with checklist data..."
      };
      dialog.Filters.Add(StorableUtils.CreateCommonFileDialogFilter("Checklist files", "checklist.xml"));
      dialog.Filters.Add(StorableUtils.CreateCommonFileDialogFilter("XML files", "xml"));
      dialog.Filters.Add(StorableUtils.CreateCommonFileDialogFilter("All files", "*"));
      if (dialog.ShowDialog() != CommonFileDialogResult.Ok) return;
      recentXmlFile = dialog.FileName!;

      try
      {
        LoadChecklists();
      }
      catch (Exception ex)
      {
        ShowError("Failed to load file. Reason:" + ex);
        return;
      }
    }

    private void LoadChecklists()
    {
      var tmp = LoadChecklistFromFile(recentXmlFile);
      lblLoadingResult.Content = $"Loaded file with {tmp.Count} checklists.";
      this.vm.Checklists = tmp;
    }

    private void ShowError(string msg)
    {
      System.Windows.MessageBox.Show(msg, "Checklist TTS", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private List<CheckList> LoadChecklistFromFile(string xmlFile)
    {
      List<CheckList> ret;
      try
      {
        XDocument doc = XDocument.Load(xmlFile);
        var tmpMeta = MetaInfo.Deserialize(doc);
        var tmp = Eng.Chlaot.Modules.ChecklistModule.Types.Xml.Deserializer.Deserialize(doc);
        ret = tmp.Checklists;
      }
      catch (Exception ex)
      {
        throw new ApplicationException("Unable to read/deserialize checklist-set from '{xmlFile}'. Invalid file content?", ex);
      }
      return ret;
    }

    private void btnOutputFolder_Click(object sender, RoutedEventArgs e)
    {
      var dialog = new CommonOpenFileDialog
      {
        AllowNonFileSystemItems = true,
        Multiselect = false,
        IsFolderPicker = true,
        Title = "Select folders with jpg files"
      };
      if (dialog.ShowDialog() == CommonFileDialogResult.Ok && dialog.FileName != null)
      {
        this.vm.OutputPath = dialog.FileName;
      }
    }

    private void btnContinue_Click(object sender, RoutedEventArgs e)
    {
      if (vm.Checklists == null || vm.Checklists.Count == 0)
      {
        ShowError("Empty or not loaded checklist list.");
        return;
      }

      if (ctrTtss.SelectedModule == null || ctrTtss.SelectedModule.IsReady == false)
      {
        ShowError("Empty TTS module or module is not ready.");
        return;
      }

      if (this.vm.OutputPath == null)
      {
        ShowError("Empty outputh path.");
        return;
      }

      SaveVM();

      FrmTTS frm = new FrmTTS();
      frm.Init(this.vm);
      frm.WindowStartupLocation = this.WindowStartupLocation;
      frm.Show();
      this.Hide();
    }

    private static string VMFileName => "vm.xml";

    private void SaveVM()
    {
      try
      {
        XmlSerializer ser = new XmlSerializer(typeof(InitVM));
        using FileStream fs = new System.IO.FileStream(VMFileName, FileMode.OpenOrCreate);
        ser.Serialize(fs, this.vm);
      }
      catch (Exception ex)
      {
        //TODO
        throw ex;
      }
    }

    public void LoadVM()
    {
      try
      {
        if (System.IO.File.Exists(VMFileName))
        {
          XmlSerializer ser = new XmlSerializer(typeof(InitVM));
          using FileStream fs = new System.IO.FileStream(VMFileName, FileMode.OpenOrCreate);
          var tmp = (InitVM?)ser.Deserialize(fs);
          if (tmp != null) this.DataContext = this.vm = tmp;
          LoadChecklists();
        }
        else
          this.DataContext = this.vm = new();
      }
      catch (Exception ex)
      {
        // TODO
        throw ex;
      }
    }
  }
}