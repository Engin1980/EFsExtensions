using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking.VariableModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.Chlaot.Modules.CopilotModule.Types
{
  public class SpeechDefinition
  {
    public string Title { get; set; } = null!;
    public Speech Speech { get; set; } = null!;
    public IStateCheckItem When { get; set; } = null!;
    public IStateCheckItem ReactivateWhen { get; set; } = null!;
    public List<UserVariable> Variables { get; set; } = new();

    /*
     * This is here because I need those values to be as collections for WPF TreeView
     */
    public Collection<IStateCheckItem> __WhenCollection => CreateCollectionWith(this.When);
    public Collection<IStateCheckItem> __ReactivateWhenCollection => CreateCollectionWith(this.ReactivateWhen);

    private Collection<IStateCheckItem> CreateCollectionWith(IStateCheckItem item)
    {
      Collection<IStateCheckItem> ret = new()
      {
        item
      };
      return ret;
    }
  }
}
