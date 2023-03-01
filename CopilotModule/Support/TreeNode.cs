using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CopilotModule.Support
{
  public class TreeNode
  {
    public string Text { get; set; }
    public List<TreeNode> Items { get; set; }
    public TreeNode(string text, List<TreeNode>? items = null)
    {
      this.Text = text;
      this.Items = items ?? new();
    }
  }
}
