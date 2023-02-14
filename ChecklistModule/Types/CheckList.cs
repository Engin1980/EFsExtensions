using EXmlLib.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChecklistModule.Types
{
  public class CheckList
  {
    public string Id { get; set; }
    public string CallSpeech { get; set; }
    [EXmlFlat]
    public List<CheckItem> Items { get; set; }
  }
}
