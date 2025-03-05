using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.SimObjects;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.Storable;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.TTSs;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.TTSs.ElevenLabs;
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
using Eng.EFsExtensions.Modules.ChecklistModule.Types;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.IO;
using System.Windows.Media.TextFormatting;
using ESystem.Exceptions;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.TTSs.MSAPI;
using ChecklistTTS;
using ChecklistTTS.Model;

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
    private readonly InitVM vm;

    public FrmInit()
    {
      InitializeComponent();

      this.DataContext = this.vm = new();
      ApplyAppSettings();

      this.ctrTtss.Init(ttsModules);
    }

    private void ApplyAppSettings()
    {
      var sett = ChecklistTTS.Properties.Settings.Default;
      this.vm.ChecklistFileName = sett.RecentChecklistFile;
      this.vm.OutputPath = sett.RecentOutputPath;

      var module = ttsModules.FirstOrDefault(q => q.Name == sett.RecentModuleName);
      if (module != null)
      {
        this.ctrTtss.SelectedModule = module;
        string? moduleSettingsStr = null;
        foreach (var item in sett.ModuleSettings)
        {
          if (item != null && item.StartsWith(sett.RecentModuleName + ";"))
          {
            moduleSettingsStr = item[(sett.RecentModuleName.Length + 1)..];
            break;
          }
          if (moduleSettingsStr != null)
          {
            var moduleSettings = ctrTtss.GetSettingsForModule(module);
            moduleSettings.LoadFromSettingsString(moduleSettingsStr);
          }
        }
      }
    }

    private void SaveAppSettings()
    {
      var sett = ChecklistTTS.Properties.Settings.Default;
      sett.RecentChecklistFile = this.vm.ChecklistFileName;
      sett.RecentOutputPath = this.vm.OutputPath;
      sett.RecentModuleName = this.ctrTtss.SelectedModule.Name;
      if (sett.RecentModuleName != null)
      {
        var moduleSettings = ctrTtss.GetSettingsForModule(ctrTtss.SelectedModule);
        var moduleSettingsStr = moduleSettings.CreateSettingsString();

        //removes existing
        if (sett.ModuleSettings != null)
        {
          List<string?> toRemItems = new();
          foreach (var item in sett.ModuleSettings)
          {
            if (item == null || item.StartsWith(ctrTtss.SelectedModule.Name + ";"))
            {
              toRemItems.Add(item);
            }
          }
          toRemItems.ForEach(q => sett.ModuleSettings.Remove(q));
        }
        else
          sett.ModuleSettings = new();

        sett.ModuleSettings.Add(moduleSettingsStr);
      }

      sett.Save();
    }

    private void btnSelectChecklistFile_Click(object sender, RoutedEventArgs e)
    {
      var dialog = new CommonOpenFileDialog()
      {
        AddToMostRecentlyUsedList = true,
        EnsureFileExists = true,
        DefaultFileName = this.vm.ChecklistFileName,
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
        this.vm.ChecklistFileName = dialog.FileName!;
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
        var tmp = Eng.EFsExtensions.Modules.ChecklistModule.Types.Xml.Deserializer.Deserialize(doc);
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

      SaveAppSettings();

      var ttsModule = ctrTtss.SelectedModule;
      var ttsModuleSettings = ctrTtss.GetSettingsForModule(ttsModule);
      var ttsProvider = ttsModule.GetProvider(ttsModuleSettings);

      FrmRun frm = new()
      {
        WindowStartupLocation = this.WindowStartupLocation
      };
      frm.Init(this.vm, ttsProvider);
      frm.Show();
      this.Hide();
      frm.Run();
    }
  }
}