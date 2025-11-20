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

    private const string AIRPORTS_FILE_NAME = @"Xmls\Airports.xml";
    private const string ADDITIONAL_AIRPORTS_FILE_NAME_PATTERN = @"Airports-*.xml";
    private void InitNavData()
    {
      List<Airport> airports;
      try
      {
        airports = XmlLoader.Load(AIRPORTS_FILE_NAME, true).ToList();
        ExtendNavaidData(airports);
        airports = airports.OrderBy(q => q.ICAO).ToList();
      }
      catch (Exception ex)
      {
        throw new Exception($"Error loading airports from '{AIRPORTS_FILE_NAME}'", ex);
      }

      this.NavData = new NavData()
      {
        Airports = new AirportList(airports),
      };
    }
    private void ExtendNavaidData(List<Airport> airports)
    {
      var tmp = System.IO.Path.GetFullPath(AIRPORTS_FILE_NAME);
      tmp = System.IO.Path.GetDirectoryName(tmp)!;
      var files = System.IO.Directory.GetFiles(tmp, ADDITIONAL_AIRPORTS_FILE_NAME_PATTERN);
      foreach (var file in files)
      {
        List<Airport> addAirports;
        try
        {
          addAirports = XmlLoader.Load(file, true).ToList();
        }
        catch (Exception ex)
        {
          throw new Exception($"Error loading additional airports from '{file}'", ex);
        }
        airports.AddRange(addAirports);
      }
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
