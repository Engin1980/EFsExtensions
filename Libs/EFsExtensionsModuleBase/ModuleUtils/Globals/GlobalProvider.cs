using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.SimObjects;
using Eng.EFsExtensions.Libs.AirportsLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.Globals
{
  public class GlobalProvider
  {
    private readonly static Lazy<GlobalProvider> _instance = new Lazy<GlobalProvider>(() =>
    {
      GlobalProvider ret = new();
      ret.Init();
      return ret;
    });

    public static GlobalProvider Instance => _instance.Value;

    public SimPropertyGroup SimPropertyGroup { get; private set; } = null!;
    public NavData NavData { get; private set; } = null!;

    private void Init()
    {
      InitSimPropertyGroup();
      InitNavData();
    }

    private void InitNavData()
    {
      const string FILE_NAME = @"Xmls\Airports.xml";
      List<Airport> airports;
      try
      {
        airports = XmlLoader.Load(FILE_NAME, true).OrderBy(q => q.ICAO).ToList();
      }
      catch (Exception ex)
      {
        throw new Exception($"Error loading airports from '{FILE_NAME}'", ex);
      }

      this.NavData = new NavData()
      {
        Airports = new AirportList(airports),
      };
    }

    private void InitSimPropertyGroup()
    {
      const string FILE_NAME = @"Xmls\SimProperties.xml";
      SimPropertyGroup ret;
      try
      {
        XDocument doc = XDocument.Load(FILE_NAME, LoadOptions.SetLineInfo);
        ret = SimPropertyGroup.Deserialize(doc.Root!);
      }
      catch (Exception ex)
      {
        throw new ApplicationException($"Failed to load global sim properties from {FILE_NAME}.", ex);
      }

      this.SimPropertyGroup = ret;
    }
  }
}
