using ChecklistTTS.Model;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils;
using Eng.Chlaot.ChlaotModuleBase.ModuleUtils.TTSs;
using Eng.Chlaot.Modules.ChecklistModule.Types;
using ESystem;
using ESystem.Exceptions;
using ESystem.Miscelaneous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Recognition;
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
    private string outputPath;
    private ITtsProvider ttsProvider;
    public FrmRun()
    {
      InitializeComponent();
      this.DataContext = this.vm = new RunVM();
    }

    internal void Init(InitVM initVm, ITtsProvider provider)
    {
      MetaInfo? m;
      List<CheckList> checklists;
      (m, checklists) = LoadChecklistFromFile(initVm.ChecklistFileName);
      this.vm.CheckLists = checklists
        .Select(q => new CheckListVM(q))
        .ToList();
      this.vm.MetaInfo = m;

      this.ttsProvider = provider;
      this.outputPath = initVm.OutputPath;
    }

    internal async Task RunAsync()
    {
      List<CheckList> newChecklist = new();

      foreach (var checklist in vm.CheckLists)
      {
        checklist.State = ProcessState.Active;
        try
        {
          CheckList ch = await ConvertChecklistAsync(checklist);
          newChecklist.Add(ch);

          checklist.State = ProcessState.Processed;
        }
        catch (Exception ex)
        {
          Log(0, $"Failed to convert checklist '{checklist.CheckList.Id}'.");
          checklist.State = ProcessState.Failed;
        }
      }
    }

    internal void Run()
    {
      Action a = async () => await RunAsync();
      Task t = new(a);
      t.Start();
    }

    private async Task<CheckList> ConvertChecklistAsync(CheckListVM checklist)
    {
      CheckList ret = new CheckList()
      {
        NextChecklistIds = checklist.CheckList.NextChecklistIds,
        Trigger = checklist.CheckList.Trigger,
        Variables = checklist.CheckList.Variables,
        CallSpeech = checklist.CheckList.CallSpeech,
        CustomEntrySpeech = checklist.CheckList.CustomEntrySpeech, //TODO convert also this!
        CustomExitSpeech = checklist.CheckList.CustomExitSpeech,
        Id = checklist.CheckList.Id,
        Items = new()
      };

      foreach (var item in checklist.CheckItems)
      {
        item.State = ProcessState.Active;
        try
        {
          CheckItem ci = await ConvertChecklistItemAsync(item);
          ret.Items.Add(ci);
          item.State = ProcessState.Processed;
        }
        catch (Exception ex)
        {
          Log(0, $"Failed to convert checklist item " +
            $"{item.CheckItem.Call.Value} - {item.CheckItem.Confirmation.Value}: " +
            $"{ex.GetFullMessage()}");
          item.State = ProcessState.Failed;
        }
      }

      return ret;
    }

    private async Task<CheckItem> ConvertChecklistItemAsync(CheckItemVM item)
    {
      Log(1, $"Converting {item.CheckItem.Call.Value} - {item.CheckItem.Confirmation.Value}");
      var call = item.CheckItem.Call;
      var conf = item.CheckItem.Confirmation;

      var newCall = await ConvertSpeechAsync(call);
      var newConf = await ConvertSpeechAsync(conf);

      CheckItem ret = new()
      {
        Call = newCall,
        Confirmation = newConf
      };
      return ret;
    }

    private async Task<CheckDefinition> ConvertSpeechAsync(CheckDefinition speech)
    {
      CheckDefinition ret;
      if (speech.Type == CheckDefinition.CheckDefinitionType.File)
      {
        Log(2, $"Speech '{speech.Value}' is of type file, skipping.");
        ret = new CheckDefinition()
        {
          Type = speech.Type,
          Value = speech.Value
        };
      }
      else if (speech.Type == CheckDefinition.CheckDefinitionType.Speech)
      {
        var text = speech.Value;
        string fileName = SanitizeFileName(text) + ".mp3";
        string fullFileName = SanitizeFileName(text) + ".mp3";

        ret = new CheckDefinition()
        {
          Type = CheckDefinition.CheckDefinitionType.File,
          Value = fileName
        };

        if (System.IO.File.Exists(fullFileName))
        {
          Log(2, $"Speech '{speech.Value}' already converted, skipping.");
        }
        else
        {
          Log(2, $"Converting speech '{speech.Value}' ");
          byte[] bytes = await ttsProvider.ConvertAsync(text);
          await System.IO.File.WriteAllBytesAsync(fullFileName, bytes);
        }
      }
      else
      {
        throw new UnexpectedEnumValueException(speech.Type);
      }

      return ret;
    }

    public static string SanitizeFileName(string input)
    {
      if (string.IsNullOrWhiteSpace(input))
        throw new ArgumentException("Input cannot be null or whitespace.", nameof(input));

      char replacement = '_';
      char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();

      return new string(input.Select(c => invalidChars.Contains(c) ? replacement : c).ToArray());
    }

    private void Log(int v1, string v2)
    {
      // TODO
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
