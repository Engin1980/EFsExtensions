using System.Text;

namespace ESystem
{
  public static class Extensions
  {
    public static StringBuilder AppendIf(this StringBuilder sb, bool condition, string text)
    {
      if (condition) 
        sb.Append(text);
      return sb;
    }
  }
}
