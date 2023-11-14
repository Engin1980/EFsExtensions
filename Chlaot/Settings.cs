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

      internal ELogging.LogRule ToELogRule()
      {
        LogLevel ll = LogLevel.Unused;
        if (this.Verbose) ll |= LogLevel.VERBOSE;
        if (this.Info) ll |= LogLevel.INFO;
        if (this.Warning) ll |= LogLevel.WARNING;
        if (this.Error) ll |= LogLevel.ERROR;

        ELogging.LogRule ret = new ELogging.LogRule(this.Regex, ll);
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
      //Settings tmp = Settings.CreateDefault();
      //XmlSerializer tmps = new XmlSerializer(typeof(Settings));
      //FileStream tmpf = new FileStream(fileName, FileMode.Create);
      //tmps.Serialize(tmpf, tmp);
      //tmpf.Close();

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
