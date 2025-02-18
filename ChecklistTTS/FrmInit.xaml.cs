using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.SimObjects;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.Storable;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.TTSs;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.TTSs.ElevenLabs;
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
using ESystem.Exceptions;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.TTSs.MSAPI;
using ChecklistTTS;

namespace ChecklistTTSNew
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class FrmInit : Window
  {
    private static readonly List<ITtsModule> ttsModules = new(){
      new MSapiModule(),
      new ElevenLabsTtsModule()
      };
    private string recentXmlFile = string.Empty;
    private readonly InitVM vm;

    public FrmInit()
    {
      InitializeComponent();
      try
      {
        this.DataContext = this.vm = LoadVM();
        LoadChecklists(this.recentXmlFile);
      }
      catch (Exception ex)
      {
        //TODO add logging
        this.DataContext = this.vm = new InitVM();
      }

      this.ctrTtss.Init(ttsModules);
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

      try
      {
        LoadChecklists(dialog.FileName!);
        this.vm.ChecklistFileName = recentXmlFile = dialog.FileName!;
      }
      catch (Exception ex)
      {
        System.Windows.MessageBox.Show("Failed to load file. Reason:" + ex);
        return;
      }
    }

    private void LoadChecklists(string xmlFile)
    {
      var tmp = LoadChecklistFromFile(xmlFile);
      lblLoadingResult.Content = $"Loaded file with {tmp.Count} checklists.";
      this.vm.Checklists = tmp;
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
        if (System.IO.Directory.GetFiles(dialog.FileName).Length > 0)
        {
          var msgRes = System.Windows.MessageBox.Show(
            "There are some files in the selected folder ''. They may be overwritten. Are you sure?",
            "Directory is not empty",
            MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Warning);
          if (msgRes == MessageBoxResult.Yes)
            this.vm.OutputPath = dialog.FileName;
        }
        else
          this.vm.OutputPath = dialog.FileName;
      }
    }
    private List<string> GetRunBlockingErrorsIfAny()
    {
      List<string> ret = new();
      if (vm.Checklists == null || vm.Checklists.Count == 0)
      {
        ret.Add("Empty or not loaded checklist list.");
      }

      if (ctrTtss.SelectedModule == null)
      {
        ret.Add("No module selected.");
      }
      else
      {
        var sett = ctrTtss.GetSettingsForModule(ctrTtss.SelectedModule);
        if (sett == null || sett.IsValid == false)
          ret.Add("Settings are null or not valid yet.");
      }

      if (this.vm.OutputPath == null || System.IO.Directory.Exists(this.vm.OutputPath) == false)
      {
        ret.Add("Empty outputh path or output path does not exist.");
      }
      return ret;
    }

    private void btnContinue_Click(object sender, RoutedEventArgs e)
    {
      var errors = GetRunBlockingErrorsIfAny();
      if (errors.Count > 0)
      {
        System.Windows.MessageBox.Show("Unable to start the process. There are issues: \n" +
          string.Join("\n  ", errors), "Error...", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        return;
      }

      //TODO implement somehow
      //SaveVM();

      FrmRun frm = new FrmRun();
      frm.WindowStartupLocation = this.WindowStartupLocation;
      frm.Init(this.vm);
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

    public InitVM LoadVM()
    {
      InitVM ret;
      try
      {
        if (System.IO.File.Exists(VMFileName))
        {
          XmlSerializer ser = new XmlSerializer(typeof(InitVM));
          using FileStream fs = new System.IO.FileStream(VMFileName, FileMode.Open);
          ret = (InitVM)(ser.Deserialize(fs) ?? throw new UnexpectedNullException());
        }
        else
          ret = new();
      }
      catch (Exception ex)
      {
        throw new ApplicationException("Unable to load VM set-up.", ex);
      }
      return ret;
    }
  }
}