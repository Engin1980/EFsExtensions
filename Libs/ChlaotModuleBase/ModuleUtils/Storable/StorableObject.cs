using ESystem.Miscelaneous;
using Microsoft.FlightSimulator.SimConnect;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.Storable
{
  public abstract class StorableObject : NotifyPropertyChanged
  {
    public void Load(string fileName)
    {
      object tmp;
      try
      {
        using FileStream fs = new(fileName, FileMode.Open);
        XmlSerializer ser = new(this.GetType());
        tmp = ser.Deserialize(fs)!;
      }
      catch (Exception ex)
      {
        throw new ApplicationException($"Failed to deserialize settings from {fileName}.", ex);
      }

      CopyProperties(tmp, this);
    }

    public void Save(string fileName)
    {
      try
      {
        string file = Path.GetTempFileName();
        using (FileStream fs = new(file, FileMode.Truncate))
        {
          XmlSerializer ser = new(this.GetType());
          ser.Serialize(fs, this);
        }
        File.Copy(file, fileName, true);
        File.Delete(file);
      }
      catch (Exception ex)
      {
        throw new ApplicationException($"Failed to serialize settings to {fileName}.", ex);
      }
    }

    private static void CopyProperties(object source, object target)
    {
      if (source.GetType() != target.GetType())
      {
        throw new ApplicationException(
          $"The type of loaded and current object must match " +
          $"(found {source.GetType().Name} and {target.GetType().Name}.)");
      }

      var props = source.GetType().GetProperties().ToList();
      props = props.Where(q => q.CanWrite && q.CanRead).ToList();
      foreach (var prop in props)
      {
        object? val;
        try
        {
          val = prop.GetValue(source);
        }
        catch (Exception ex)
        {
          throw new ApplicationException($"Failed to read {source.GetType().Name}.{prop.Name}.", ex);
        }
        try
        {
          prop.SetValue(target, val);
        }
        catch (Exception ex)
        {
          throw new ApplicationException($"Failed to write {source.GetType().Name}.{prop.Name} = {val}.", ex);
        }
      }
    }
  }
}
