using ChecklistModule.Support;
using ChecklistModule.Types;
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
    private readonly Context context;
    private readonly Player player;

    public CtrInit()
    {
      InitializeComponent();
    }

    public CtrInit(Context context) : this()
    {
      this.player = new();
      this.context = context;
      this.DataContext = context;
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
        Title = "Select XML file with checklist data..."
      };
      dialog.Filters.Add(new CommonFileDialogFilter("XMl files", "xml"));
      dialog.Filters.Add(new CommonFileDialogFilter("All files", "*"));
      if (dialog.ShowDialog() != CommonFileDialogResult.Ok) return;
      recentXmlFile = dialog.FileName;

      this.context.LoadFile(recentXmlFile);
    }

    private void lblChecklist_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
      Label lbl = (Label)sender;
      CheckList checkList = (CheckList)lbl.Tag;
      this.player.ClearQueue();
      this.player.PlayAsync(checkList.EntrySpeechBytes);
      this.player.PlayAsync(checkList.ExitSpeechBytes);
    }

    private void lblItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
      Label lbl = (Label)sender;
      CheckItem checkItem = (CheckItem)lbl.Tag;
      this.player.ClearQueue();
      this.player.PlayAsync(checkItem.Call.Bytes);
      this.player.PlayAsync(checkItem.Confirmation.Bytes);
    }
  }
}