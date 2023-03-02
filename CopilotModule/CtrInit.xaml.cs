using ChlaotModuleBase.ModuleUtils.Playing;
using CopilotModule.Types;
using Eng.Chlaot.Modules.CopilotModule;
using Microsoft.WindowsAPICodePack.Dialogs;
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

namespace CopilotModule
{
  /// <summary>
  /// Interaction logic for CtrInit.xaml
  /// </summary>
  public partial class CtrInit : UserControl
  {
    private InitContext context;
    private readonly Player player;

    public CtrInit()
    {
      InitializeComponent();
      this.player = new Player();
    }

    internal CtrInit(InitContext initContext) : this()
    {
      this.context = initContext;
      this.DataContext = context;
    }

    private void btnSettings_Click(object sender, RoutedEventArgs e)
    {
      new CtrSettings(context.Settings).ShowDialog();
    }

    private string recentXmlFile;

    private void btnLoadChecklistFile_Click(object sender, RoutedEventArgs e)
    {
      var dialog = new CommonOpenFileDialog()
      {
        AddToMostRecentlyUsedList = true,
        EnsureFileExists = true,
        DefaultFileName = recentXmlFile,
        Multiselect = false,
        Title = "Select XML file with copilot speeches data..."
      };
      dialog.Filters.Add(CreateCommonFileDialogFilter("Copilot files", "copilot.xml"));
      dialog.Filters.Add(CreateCommonFileDialogFilter("XML files", "xml"));
      dialog.Filters.Add(CreateCommonFileDialogFilter("All files", "*"));
      if (dialog.ShowDialog() != CommonFileDialogResult.Ok) return;
      recentXmlFile = dialog.FileName;

      this.context.LoadFile(recentXmlFile);
    }

    private CommonFileDialogFilter CreateCommonFileDialogFilter(string title, string extension)
    {
      CommonFileDialogFilter cfdf = new CommonFileDialogFilter(title, extension);
      Type t = cfdf.GetType();
      var fieldInfo = t.GetField("_extensions",
        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
      System.Collections.ObjectModel.Collection<string?> col =
        (System.Collections.ObjectModel.Collection<string?>)fieldInfo.GetValue(cfdf)!;
      col.Clear();
      col.Add(extension);
      return cfdf;
    }

    private void pnlSpeech_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
      StackPanel panel = (StackPanel)sender;
      SpeechDefinition sd = (SpeechDefinition)panel.Tag;
      this.player.ClearQueue();
      this.player.PlayAsync(sd.Speech.Bytes);
    }
  }
}
