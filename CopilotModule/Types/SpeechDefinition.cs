using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.StateChecking;
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
#pragma warning disable CS8618
    public string Title { get; set; }
    public Speech Speech { get; set; }
    public IStateCheckItem When { get; set; }
    public IStateCheckItem ReactivateWhen { get; set; }
    public List<Variable> Variables { get; set; } = new();
#pragma warning restore CS8618

    /*
     * This is here because I need those values to be as collections for WPF TreeView
     */
    public Collection<IStateCheckItem> __WhenCollection => CreateCollectionWith(this.When);
    public Collection<IStateCheckItem> __ReactivateWhenCollection => CreateCollectionWith(this.ReactivateWhen);

    private Collection<IStateCheckItem> CreateCollectionWith(IStateCheckItem item)
    {
      Collection<IStateCheckItem> ret = new();
      ret.Add(item);
      return ret;
    }
  }
}
