using ELogging;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.Storable;
using Eng.Chlaot.Modules.AffinityModule;
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

namespace AffinityModule
{
  /// <summary>
  /// Interaction logic for UserControl1.xaml
  /// </summary>
  public partial class CtrInit : System.Windows.Controls.UserControl
  {
    public readonly Logger logger;
    public readonly Context context;
    private string recentXmlFile = "";

    public CtrInit()
    {
      InitializeComponent();
      this.logger = null!;
      this.context = null!;
    }

    public CtrInit(Context context) : this()
    {
      this.logger = Logger.Create(this);
      this.context = context;
      this.DataContext = context;
    }


    private void btnLoad_Click(object sender, RoutedEventArgs e)
    {
      var dialog = new CommonOpenFileDialog()
      {
        AddToMostRecentlyUsedList = true,
        EnsureFileExists = true,
        DefaultFileName = recentXmlFile,
        Multiselect = false,
        Title = "Select XML file with copilot speeches data..."
      };
      dialog.Filters.Add(StorableUtils.CreateCommonFileDialogFilter("Affinity rule-base files", "affi.xml"));
      dialog.Filters.Add(StorableUtils.CreateCommonFileDialogFilter("XML files", "xml"));
      dialog.Filters.Add(StorableUtils.CreateCommonFileDialogFilter("All files", "*"));
      if (dialog.ShowDialog() != CommonFileDialogResult.Ok || dialog.FileName == null) return;

      recentXmlFile = dialog.FileName;
      this.context.LoadRuleBase(recentXmlFile);
    }
  }
}