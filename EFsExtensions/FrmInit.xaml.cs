using ESystem.Logging;
using Eng.EFsExtensions.EFsExtensionsModuleBase;
using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.Storable;
using ESystem;
using ESystem.Asserting;
using Microsoft.VisualBasic.ApplicationServices;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
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
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

using ModuleRestoreDict = System.Collections.Generic.Dictionary<string, string>;
using ModulesRestoreData = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, string>>;
using ESystem.Exceptions;

namespace Eng.EFsExtensions.App
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class FrmInit : Window
  {
    private readonly Context context = new Context();
    private Settings? appSettings;
    private static ModulesRestoreData lastRunModuleRestoreData = new ModulesRestoreData();

    public FrmInit()
    {
      InitializeComponent();
      this.Title = "E-FS-Extensions - Initialization - " + Assembly.GetExecutingAssembly().GetName().Version;
    }

    [SuppressMessage("", "IDE1006")]
    private void btnRun_Click(object sender, RoutedEventArgs e)
    {
      if (this.context.Modules.None(q => q.IsReady))
      {
        Logger.Log(this, LogLevel.ERROR, "Any module ready, cannot start.");
      }

      SaveModulesResetData();

      FrmRun frmRun = new(this.context, appSettings ?? throw new UnexpectedNullException());
      Logger.UnregisterLogAction(this);
      this.Close();
      frmRun.Show();
    }

    private void SaveModulesResetData()
    {
      lastRunModuleRestoreData = CreateModulesRestoreData();
    }

    [SuppressMessage("", "IDE1006")]
    private void lstModules_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      IModule module = (IModule)lstModules.SelectedItem;
      if (module == null) return;
      pnlContent.Children.Clear();
      pnlContent.Children.Add(module.InitControl);
    }

    private void Window_Initialized(object sender, EventArgs e)
    {
      LoadSettings();
      RegisterLogListeners();
      this.context.SetUpModules();

      this.DataContext = this.context;
      if (lstModules.Items.Count > 0) lstModules.SelectedIndex = 0;

      this.context.InitModules();
    }

    private void RegisterLogListeners()
    {
      EAssert.IsNotNull(this.appSettings, "Cannot register log listeners - app settings not loaded.");
      LogHelper.RegisterGlobalLogListener(this.appSettings.LogFileLogRules);
      LogHelper.RegisterWindowLogListener(this.appSettings.WindowLogRules, this, this.txtConsole);
    }

    private const string APP_SETTINGS_FILE = "appConfig.xml";
    private void LoadSettings()
    {
      this.appSettings = Settings.Load(APP_SETTINGS_FILE, out string? err);
      if (err != null)
      {
        LogToConsole("Failed to load stored settings. Default ones will be used. Reason: " + err + "\n");
      }
    }

    private void LogToConsole(string s)
    {
      txtConsole.AppendText(s);
      txtConsole.ScrollToEnd();
    }

    private void btnLoadSet_Click(object sender, RoutedEventArgs e)
    {
      const string DEFAULT_EXT = "storeset.xml";
      var dialog = new CommonOpenFileDialog()
      {
        AddToMostRecentlyUsedList = true,
        Multiselect = false,
        Title = "Select XML file with stored data..."
      };
      dialog.Filters.Add(StorableUtils.CreateCommonFileDialogFilter("Stored set", DEFAULT_EXT));
      dialog.Filters.Add(StorableUtils.CreateCommonFileDialogFilter("XML files", "xml"));
      dialog.Filters.Add(StorableUtils.CreateCommonFileDialogFilter("All files", "*"));
      if (dialog.ShowDialog() != CommonFileDialogResult.Ok || dialog.FileName == null) return;

      XDocument doc;
      try
      {
        doc = XDocument.Load(dialog.FileName);
      }
      catch (Exception ex)
      {
        // TODO better handling
        throw new ApplicationException("Failed to load Xml document from " + dialog.FileName, ex);
      }

      ModulesRestoreData modulesRestoreData = new();
      try
      {
        XElement root = doc.Root!;

        foreach (XElement moduleElement in root.Elements())
        {
          var curr = modulesRestoreData[moduleElement.Name.LocalName] = new();
          foreach (XElement elm in moduleElement.Elements())
          {
            string key = elm.Name.LocalName;
            string value = elm.Value;
            curr[key] = value;
          }
        }
      }
      catch (Exception ex)
      {
        // TODO better handling
        throw new ApplicationException("Failed to extract data from XML Document - probably invalid content?", ex);
      }

      ApplyModuleRestoreData(modulesRestoreData);

      MessageBox.Show("Loaded.");
    }

    private void ApplyModuleRestoreData(ModulesRestoreData modulesRestoreData)
    {
      foreach (var entry in modulesRestoreData)
      {
        var module = this.context.Modules.FirstOrDefault(q => q.Name == entry.Key);
        if (module == null) continue;
        try
        {
          module.Restore(entry.Value);
        }
        catch (Exception ex)
        {
          // TODO better error handling
          throw new ApplicationException("Failed to restore module from data.", ex);
        }
      }
    }

    private ModulesRestoreData CreateModulesRestoreData()
    {
      ModulesRestoreData ret = this.context.Modules
        .Select(q => new { Key = q.Name.Replace(" ", "_"), Value = q.TryGetRestoreData() })
        .Where(q => q.Value != null)
        .ToDictionary(q => q.Key, q => q.Value!);
      return ret;
    }

    private void btnSaveSet_Click(object sender, RoutedEventArgs e)
    {
      const string DEFAULT_EXT = "storeset.xml";
      var dialog = new CommonSaveFileDialog()
      {
        AddToMostRecentlyUsedList = true,
        DefaultFileName = "default." + DEFAULT_EXT,
        Title = "Select XML file to store data...",
        DefaultExtension = DEFAULT_EXT,
      };
      dialog.Filters.Add(StorableUtils.CreateCommonFileDialogFilter("Stored set", DEFAULT_EXT));
      dialog.Filters.Add(StorableUtils.CreateCommonFileDialogFilter("XML files", "xml"));
      dialog.Filters.Add(StorableUtils.CreateCommonFileDialogFilter("All files", "*"));
      if (dialog.ShowDialog() != CommonFileDialogResult.Ok || dialog.FileName == null) return;


      ModulesRestoreData modulesRestoreData = CreateModulesRestoreData();

      XDocument doc;
      try
      {
        XElement root = new(XName.Get("restoreData"));
        foreach (var restoreEntry in modulesRestoreData)
        {
          XElement moduleElement = new(XName.Get(restoreEntry.Key));
          foreach (var entry in restoreEntry.Value)
          {
            XElement elm = new(XName.Get(entry.Key));
            elm.Value = entry.Value;
            moduleElement.Add(elm);
          }
          root.Add(moduleElement);
        }

        doc = new XDocument(
            new XDeclaration("1.0", "utf-8", "yes"),
            root
        );
      }
      catch (Exception ex)
      {
        //TODO better handling
        throw new ApplicationException("Failed to create Xml document with data.", ex);
      }

      try
      {
        doc.Save(dialog.FileName);
      }
      catch (Exception ex)
      {
        //TODO better handling
        throw new ApplicationException("Failed to save Xml document to " + dialog.FileName, ex);
      }

      MessageBox.Show("Saved.");
    }

    internal void ResetModules()
    {
      ApplyModuleRestoreData(lastRunModuleRestoreData);
    }
  }
}
