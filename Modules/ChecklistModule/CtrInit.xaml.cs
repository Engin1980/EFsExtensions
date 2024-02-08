using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.Playing;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.Storable;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.TTSs;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.TTSs.ElevenLabs;
using Eng.Chlaot.Modules.ChecklistModule;
using Eng.Chlaot.Modules.ChecklistModule.Types;
using Eng.Chlaot.Modules.ChecklistModule.Types.VM;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace ChecklistModule
{
  /// <summary>
  /// Interaction logic for UserControl1.xaml
  /// </summary>
  public partial class CtrInit : UserControl
  {
    private readonly InitContext context;
    private readonly AutoPlaybackManager autoPlaybackManager;
    private string recentXmlFile = "";

    public CtrInit()
    {
      InitializeComponent();
      this.context = null!;
      this.autoPlaybackManager = null!;      
    }

    public CtrInit(InitContext context) : this()
    {
      this.autoPlaybackManager = new();
      this.context = context;
      this.DataContext = context;
    }

    private void btnLoadChecklistFile_Click(object sender, RoutedEventArgs e)
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
      recentXmlFile = dialog.FileName;

      this.context.LoadFile(recentXmlFile);
    }

    private void btnSettings_Click(object sender, RoutedEventArgs e)
    {
      new CtrSettings(context.Settings).ShowDialog();
    }

    private void lblChecklist_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
      Label lbl = (Label)sender;
      CheckListVM vm = (CheckListVM)lbl.Tag;
      this.autoPlaybackManager.ClearQueue();
      this.autoPlaybackManager.Enqueue(vm.CheckList.EntrySpeechBytes);
      this.autoPlaybackManager.Enqueue(vm.CheckList.ExitSpeechBytes);
    }

    private void lblItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
      Label lbl = (Label)sender;
      CheckItemVM vm = (CheckItemVM)lbl.Tag;
      this.autoPlaybackManager.ClearQueue();
      this.autoPlaybackManager.Enqueue(vm.CheckItem.Call.Bytes);
      this.autoPlaybackManager.Enqueue(vm.CheckItem.Confirmation.Bytes);
    }
  }
}