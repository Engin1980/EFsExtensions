using ELogging;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Chlaot
{
  [Serializable]
  public class Settings
  {
    [Serializable]
    public class LogRule
    {
      [XmlAttribute]
      public string Regex { get; set; } = "";
      [XmlAttribute] public bool Verbose { get; set; }
      [XmlAttribute] public bool Info { get; set; }
      [XmlAttribute] public bool Warning { get; set; }
      [XmlAttribute] public bool Error { get; set; }

      internal ELogging.LogRule ToELogRule() //TODO update according new implementation of ELogging
      {
        LogLevel ll = this.Error ? LogLevel.ERROR
          : this.Warning ? LogLevel.WARNING
          : this.Info ? LogLevel.INFO
          : this.Verbose ? LogLevel.DEBUG
          : LogLevel.TRACE;

        ELogging.LogRule ret = new(this.Regex, ll);
        return ret;
      }
    }

    [Serializable]
    public class ModuleRule
    {
      [XmlAttribute] public string Name { get; set; } = "";
      [XmlAttribute] public bool Disabled { get; set; }
      [XmlAttribute] public bool SaveModuleConfig { get; set; }
    }

    public List<LogRule> LogFileLogRules { get; set; } = new();
    public List<LogRule> WindowLogRules { get; set; } = new();
    public List<ModuleRule> ModuleRules { get; set; } = new();

    public static Settings CreateDefault()
    {
      Settings ret = new();
      ret.WindowLogRules.Add(new LogRule()
      {
        Regex = ".+",
        Verbose = false,
        Info = false,
        Error = true,
        Warning = true
      });
      ret.LogFileLogRules.Add(new LogRule()
      {
        Regex = ".+",
        Verbose = true,
        Info = true,
        Error = true,
        Warning = true
      });
      return ret;
    }

    public static Settings Load(string fileName, out string? errorText)
    {
      Settings ret;
      try
      {
        XmlSerializer ser = new(typeof(Settings));
        using FileStream fs = new(fileName, FileMode.Open);
        object obj = ser.Deserialize(fs) ?? throw new NullReferenceException("Deserialization of settings returned null.");
        ret = (Settings)obj;
        errorText = null;
      }
      catch (Exception ex)
      {
        errorText = ex.Message;
        ret = Settings.CreateDefault();
      }
      return ret;
    }
  }
}
