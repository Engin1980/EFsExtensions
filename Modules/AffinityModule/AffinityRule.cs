using AffinityModule;
using ELogging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Eng.EFsExtensions.Modules.AffinityModule
{
  public class AffinityRule : AbstractRule
  {
    public const string ROLL_REGEX = "^((\\d+)(-(\\d+))?)(,(\\d+)(-(\\d+))?)*$";

    public List<bool> CoreFlags { get; set; }

    private string _Roll = "";
    public string Roll
    {
      get => this._Roll;
      set
      {
        this._Roll = value ?? throw new ArgumentNullException("Value 'Roll' cannot be null. Use empty string instead.");
        ExpandToCores();
      }
    }

    public AffinityRule()
    {
      this.CoreFlags = AffinityUtils.ToEmptyArray(false);
      _Roll = Roll = "";
      Regex = ".+";
    }

    private void ExpandToCores()
    {
      List<int> includedIndices = new();

      if (Roll.Length > 0 || Roll == "*")
      {
        if (System.Text.RegularExpressions.Regex.IsMatch(Roll, ROLL_REGEX) == false)
        {
          Logger.Log(this, LogLevel.WARNING, $"CoresPatter '{Roll}' is not in valid format.");
          return;
        }

        string[] pts = this.Roll.Split(';');
        foreach (string pt in pts)
        {
          if (pt.Contains('-'))
          {
            string[] tms = pt.Split('-');
            int fromIndex = int.Parse(tms[0]);
            int toIndex = int.Parse(tms[1]);
            for (int i = fromIndex; i <= toIndex; i++)
              includedIndices.Add(i);

          }
          else
          {
            int index = int.Parse(pt);
            includedIndices.Add(index);
          }
        }

        for (int i = 0; i < CoreFlags.Count; i++)
        {
          CoreFlags[i] = includedIndices.Contains(i);
        }
      }
      else
      {
        for (int i = 0; i < CoreFlags.Count; i++)
        {
          CoreFlags[i] = true;
        }
      }
    }

  }
}
