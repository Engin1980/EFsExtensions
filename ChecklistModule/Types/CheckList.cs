using ChlaotModuleBase;
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
    public List<CheckItem> Items { get; set; }
    public string NextChecklistId { get; set; }
    public CheckList NextChecklist { get; set; }
    public CheckListMetaInfo MetaInfo { get; set; }
    public byte[] EntrySpeechBytes { get; set; } //xml-ignored
    public byte[] ExitSpeechBytes { get; set; } // xml-ignored
  }
}
