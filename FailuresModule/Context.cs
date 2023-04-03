using ELogging;
using Eng.Chlaot.ChlaotModuleBase;
using EXmlLib;
using EXmlLib.Deserializers;
using FailuresModule.Types;
using FailuresModule.Xmls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace FailuresModule
{
  public class Context : NotifyPropertyChangedBase
  {
    private readonly NewLogHandler logHandler;
    private readonly Action<bool> setIsReadyFlagAction;

    public Context(NewLogHandler logHandler, Action<bool> setIsReadyFlagAction)
    {
      this.logHandler = logHandler ?? throw new ArgumentNullException(nameof(logHandler));
      this.setIsReadyFlagAction = setIsReadyFlagAction ?? throw new ArgumentNullException(nameof(setIsReadyFlagAction));
      this.Failures = new();
      this.FailGroup = new("Root");
    }

    public FailGroup FailGroup
    {
      get => base.GetProperty<FailGroup>(nameof(FailGroup))!;
      set => base.UpdateProperty(nameof(FailGroup), value);
    }

    public List<Failure> Failures { get; set; }

    internal void BuildFailures()
    {
      this.Failures = FailureFactory.BuildFailures();
    }

    public void LoadFile(string xmlFile)
    {
      var factory = new XmlSerializerFactory();
      FailGroup tmp;
      XDocument doc;

      try
      {
        logHandler.Invoke(LogLevel.INFO, $"Loading file '{xmlFile}'");
        try
        {
          doc = XDocument.Load(xmlFile);
          EXml<FailGroup> exml = Deserialization.CreateDeserializer(this.Failures, this.logHandler);
          tmp = exml.Deserialize(doc);
        }
        catch (Exception ex)
        {
          throw new ApplicationException("Unable to read/deserialize copilot-set from '{xmlFile}'. Invalid file content?", ex);
        }

        logHandler.Invoke(LogLevel.INFO, $"Checking sanity");
        //try
        //{
        //  CheckSanity(tmp);
        //}
        //catch (Exception ex)
        //{
        //  throw new ApplicationException("Error loading checklist.", ex);
        //}

        this.FailGroup = tmp;
        UpdateReadyFlag();
        logHandler.Invoke(LogLevel.INFO, $"Copilot set file '{xmlFile}' successfully loaded.");

      }
      catch (Exception ex)
      {
        this.setIsReadyFlagAction(false);
        logHandler.Invoke(LogLevel.ERROR, $"Failed to load copilot set from '{xmlFile}'." + ex.GetFullMessage());
      }
    }

    private void UpdateReadyFlag()
    {
      logHandler.Invoke(LogLevel.WARNING, "UpdateReadyFlag() NotImplemented");
    }
  }
}
