using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.AffinityModule
{
  public abstract class AbstractRule
  {
    public string? Title { get; set; }
    private string _Regex = ".+";
    public string Regex { get => _Regex; set => _Regex = value ?? throw new ArgumentNullException($"Property '{nameof(Regex)}' cannot be null."); }
    public string TitleOrRegex =>this.Title == null ?
      this.Regex : $"{this.Title} ({this.Regex})";
  }
}
