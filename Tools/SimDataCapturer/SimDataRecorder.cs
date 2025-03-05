using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimDataCapturer
{
  public class SimDataRecorder
  {
    private string fileName;
    private int counter = 0;

    public SimDataRecorder(string fileName)
    {
      this.fileName = fileName;
    }

    internal void SaveData(MockPlaneData rawPlaneData)
    {
      StringBuilder sb = new();
      sb.AppendLine($"[{counter++} / {DateTime.Now.ToString()}]");
      foreach (var field in rawPlaneData.GetType().GetFields())
      {
        object val = field.GetValue(rawPlaneData) ?? "(null)";
        sb.AppendLine($"{field.Name}={val}");
      }

      System.IO.File.AppendAllText(fileName, sb.ToString());
    }
  }
}
