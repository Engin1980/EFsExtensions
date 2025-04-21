using System.ComponentModel.DataAnnotations;

namespace Eng.EFsExtensions.Modules.FlightLogModule.LogModel
{
  public enum DivertReason
  {
    [Display(Name ="Not Diverted")]
    NotDiverted,
    [Display(Name = "Weather")]
    Weather,
    [Display(Name = "Medical Issue")]
    Medical,
    [Display(Name = "Mechanical Problem")]
    Mechanical,
    [Display(Name = "Unspecified Reason")]
    Other
  }

}
