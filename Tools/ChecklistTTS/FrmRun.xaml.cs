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
using System.Text.RegularExpressions;
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
using static System.Net.Mime.MediaTypeNames;

namespace ChecklistTTS
{
  /// <summary>
  /// Interaction logic for FrmRun.xaml
  /// </summary>
  public partial class FrmRun : Window
  {
    private readonly RunVM vm;
    private string outputPath = null!;
    private string soundSubPath = null!;
    private string checklistOutputFile = null!;
    private string checklistInputFile = null!;
    private ITtsProvider ttsProvider = null!;

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
      this.soundSubPath = System.IO.Path.GetFileNameWithoutExtension(initVm.ChecklistFileName) + "\\";
      System.IO.Directory.CreateDirectory(
        System.IO.Path.Combine(outputPath, soundSubPath));
      this.checklistOutputFile = System.IO.Path.Combine(
        this.outputPath,
        System.IO.Path.GetFileName(initVm.ChecklistFileName));
      this.checklistInputFile = initVm.ChecklistFileName;
    }

    internal async Task RunAsync()
    {
      Log(0, "Starting conversion...");
      Dictionary<string, string> dct = new();

      foreach (var checklist in vm.CheckLists)
      {
        checklist.State = ProcessState.Active;
        try
        {
          await ConvertChecklistAsync(checklist, dct);
          checklist.State = ProcessState.Processed;
        }
        catch (Exception ex)
        {
          Log(0, $"Failed to convert checklist '{checklist.CheckList.Id}':{ex.GetFullMessage()}");
          checklist.State = ProcessState.Failed;
        }
      }

      await SaveChecklistFromFileAsync(checklistInputFile, checklistOutputFile, dct);

      Log(0, "Conversion completed.");
    }



    internal void Run()
    {
      Action a = async () => await RunAsync();
      Task t = new(a);
      t.Start();
    }

    private async Task ConvertChecklistAsync(CheckListVM checklist, Dictionary<string, string> dct)
    {
      foreach (var item in checklist.CheckItems)
      {
        item.State = ProcessState.Active;
        try
        {
          await ConvertChecklistItemAsync(item, dct);
          item.State = ProcessState.Processed;
        }
        catch (Exception ex)
        {
          Log(0, $"Failed to convert checklist item " +
            $"'{item.CheckItem.Call.Value} - {item.CheckItem.Confirmation.Value}': " +
            $"{ex.GetFullMessage()}");
          item.State = ProcessState.Failed;
        }
      }
    }

    private async Task ConvertChecklistItemAsync(CheckItemVM item, Dictionary<string, string> dct)
    {
      Log(1, $"Converting '{item.CheckItem.Call.Value} - {item.CheckItem.Confirmation.Value}'");
      var call = item.CheckItem.Call;
      var conf = item.CheckItem.Confirmation;

      await ConvertSpeechAsync(call, dct);
      await ConvertSpeechAsync(conf, dct);
    }

    private async Task ConvertSpeechAsync(CheckDefinition speech, Dictionary<string, string> dct)
    {
      if (speech.Type == CheckDefinition.CheckDefinitionType.File)
      {
        Log(2, $"Speech '{speech.Value}' is of type file, skipping.");
      }
      else if (speech.Type == CheckDefinition.CheckDefinitionType.Speech)
      {
        var text = speech.Value;

        if (dct.ContainsKey(text))
        {
          Log(2, $"Speech '{speech.Value}' already converted, skipping.");
        }
        else
        {
          string fileName = SanitizeFileName(text) + ".mp3";
          string fullFileName = System.IO.Path.Join(
            this.outputPath,
            soundSubPath,
            fileName);

          Log(2, $"Converting speech '{speech.Value}' ");
          byte[] bytes = await ttsProvider.ConvertAsync(text);
          await System.IO.File.WriteAllBytesAsync(fullFileName, bytes);
          dct[text] = System.IO.Path.Join(soundSubPath, fileName);
        }
      }
      else
      {
        throw new UnexpectedEnumValueException(speech.Type);
      }
    }

    private static string SanitizeFileName(string input)
    {
      if (string.IsNullOrWhiteSpace(input))
        throw new ArgumentException("Input cannot be null or whitespace.", nameof(input));

      char replacement = '_';
      char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();

      return new string(input.Select(c => invalidChars.Contains(c) ? replacement : c).ToArray());
    }

    private void Log(int level, string msg)
    {
      if (System.Windows.Application.Current.Dispatcher.CheckAccess())
      {
        string s = string.Concat("\n", string.Concat(Enumerable.Repeat("    ", level)), msg);
        txtOut.Text += s;
        txtOut.ScrollToEnd();
      }
      else
        System.Windows.Application.Current.Dispatcher.Invoke(() => Log(level, msg));
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

    private async Task SaveChecklistFromFileAsync(string checklistInputFile, string checklistOutputFile, Dictionary<string, string> dct)
    {
      string xml = await System.IO.File.ReadAllTextAsync(checklistInputFile);

      string pattern = @"type=""(speech)"" value=""(.+)""";

      Regex regex = new(pattern, RegexOptions.Multiline);
      Match match = regex.Match(xml);

      string newXml = Regex.Replace(
        xml,
        pattern,
        match =>
      {
        string key = match.Groups[2].Value;
        string replacement = (dct.ContainsKey(key))
        ? dct[key]
        : match.Groups[2].Value;

        return $"type=\"file\" value=\"{replacement}\"";
      });

      await System.IO.File.WriteAllTextAsync(checklistOutputFile, newXml);
      Log(0, "Final checklist saved to file " + checklistOutputFile);
    }
  }
}
