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
    public byte[] EntrySpeechBytes { get; set; }
    public byte[] ExitSpeechBytes { get; set; }
  }
}
