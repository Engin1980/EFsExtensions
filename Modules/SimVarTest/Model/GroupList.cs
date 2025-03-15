using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.SimVarTestModule.Model
{
  public interface IStringGroupItem { }
  public class StringGroupValue    : IStringGroupItem
  {
    public StringGroupValue(string value)
    {
      this.Value = value;
    }

    public string Value { get; set; }
  }

  public class StringGroupList : IStringGroupItem
  {
    public string Title { get; set; }
    public List<IStringGroupItem> Items { get; set; } = new();
  }
}
