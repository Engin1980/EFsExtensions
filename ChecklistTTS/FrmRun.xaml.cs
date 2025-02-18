using ChecklistTTSNew;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils;
using Eng.Chlaot.Modules.ChecklistModule.Types;
using ESystem.Miscelaneous;
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
using System.Windows.Shapes;
using System.Xml.Linq;

namespace ChecklistTTS
{
  /// <summary>
  /// Interaction logic for FrmRun.xaml
  /// </summary>
  public partial class FrmRun : Window
  {
    private readonly RunVM vm;
    public FrmRun()
    {
      InitializeComponent();
      this.DataContext = this.vm = new RunVM();
    }

    internal void Init(InitVM initVm)
    {
      MetaInfo? m;
      List<CheckList> checklists;
      (m, checklists) = LoadChecklistFromFile(initVm.ChecklistFileName);
      this.vm.CheckLists = checklists
        .Select(q => new CheckListVM(q))
        .ToList();
      this.vm.MetaInfo = m;
    }

    private (MetaInfo?, List<CheckList>) LoadChecklistFromFile(string xmlFile)
    {
      //TODO this method is duplicit with the one in FrmInit
      List<CheckList> ret;
      MetaInfo? metaInfo = null;
      try
      {
        XDocument doc = XDocument.Load(xmlFile);
        metaInfo = MetaInfo.Deserialize(doc);
        var tmp = Eng.Chlaot.Modules.ChecklistModule.Types.Xml.Deserializer.Deserialize(doc);
        ret = tmp.Checklists;
      }
      catch (Exception ex)
      {
        throw new ApplicationException("Unable to read/deserialize checklist-set from '{xmlFile}'. Invalid file content?", ex);
      }
      return (metaInfo, ret);
    }
  }
}
