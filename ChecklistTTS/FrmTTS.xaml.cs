using ChecklistTTS;
using Eng.Chlaot.Modules.ChecklistModule.Types;
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
using static ChecklistTTS.TtsVM;

namespace ChecklistTTSNew
{
  /// <summary>
  /// Interaction logic for FrmTTS.xaml
  /// </summary>
  public partial class FrmTTS : Window
  {
    public FrmTTS()
    {
      InitializeComponent();
    }
    private TtsVM vm = null!;
    internal void Init(InitVM vm)
    {
      //this.vm = new TtsVM();
      //this.vm.OutputPath = vm.OutputPath;

      //this.vm.CheckListVMs = vm.Checklists!
      //  .Select(q => new CheckListVM()
      //  {
      //    CheckList = q,
      //    CheckItemVMs = q.Items.Select(p => new CheckItemVM() { CheckItem = p }).ToList()
      //  }).ToList();

      //this.DataContext = this.vm;
    }
  }
}
