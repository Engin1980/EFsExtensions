using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AffinityModule
{
  internal class AffinityAdjuster
  {
    private int CalculateAffinity(FlowLayoutPanel flp)
    {
      List<bool> bits = new List<bool>();
      foreach (var ctr in flp.Controls)
      {
        CheckBox cb = (CheckBox)ctr;
        bits.Add(cb.Checked);
      }
      BitArray bitArray = new BitArray(bits.ToArray());
      int[] tmp = new int[1];
      bitArray.CopyTo(tmp, 0);
      int ret = tmp[0];
      return ret;
    }

    private async void btnApply_Click(object sender, EventArgs e)
    {
      slbl.Text = "Applying";
      btnApply.Enabled = false;
      processInfoBindingSource.DataSource = null;

      Task<List<ProcessInfo>> task = new Task<List<ProcessInfo>>(DoApplyAndEvaluate);
      task.Start();
      List<ProcessInfo> pis = await task;

      processInfoBindingSource.DataSource = pis;
      btnApply.Enabled = true;
      slbl.Text = $"Applied. {pis.Count} processes, {pis.Count(q => q.IsAccessible)} accessible, " +
        $"{pis.Count(q => q.IsAccessible == false)} unacessible, {pis.Count(q => q.IsSelected)} selected";
    }

    private List<ProcessInfo> DoApplyAndEvaluate()
    {
      int selected = CalculateAffinity(flpSelected);
      int other = CalculateAffinity(flpOther);
      List<ProcessInfo> pis = ApplyAffinity(txtProcessName.Text.Trim(), (IntPtr)selected, (IntPtr)other);
      pis = pis.OrderBy(q => q.Name).ToList();
      return pis;
    }

    private List<ProcessInfo> ApplyAffinity(string selectedProcess, IntPtr selectedAffinity, IntPtr otherAffinity)
    {
      List<ProcessInfo> ret = new List<ProcessInfo>();
      Process[] processes = Process.GetProcesses();
      int cnt = processes.Length;
      int cur = 0;
      foreach (Process process in processes.OrderBy(q => q.ProcessName))
      {
        UpdateProgress(cur, cnt);
        ProcessInfo pi = new ProcessInfo()
        {
          Id = process.Id,
          Name = process.ProcessName,
          WindowTitle = process.MainWindowTitle,
          IsSelected = false,
          ThreadCount = process.Threads.Count
        };
        ret.Add(pi);

        IntPtr trgAffinity;
        if (System.Text.RegularExpressions.Regex.IsMatch(process.ProcessName, selectedProcess))
        {
          trgAffinity = selectedAffinity;
          pi.IsSelected = true;
        }
        else
          trgAffinity = otherAffinity;

        try
        {
          process.ProcessorAffinity = trgAffinity;
          pi.IsAccessible = true;
        }
        catch (Exception)
        {
          pi.IsAccessible = false;
        }

        try
        {
          pi.Affinity = (int)process.ProcessorAffinity;
        }
        catch (Exception)
        {
          pi.Affinity = null;
        }
        cur++;
      }
      UpdateProgress(cnt, cnt);
      return ret;
    }
  }
}
