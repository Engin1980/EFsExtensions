﻿using Eng.EFsExtensions.EFsExtensionsModuleBase.ModuleUtils.WPF.Converters;
using ESystem.Exceptions;
using ESystem.Structs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.FlightLogModule.Controls.FlightLog
{
  public class LongDistanceConverter : TypedConverter<Distance, string>
  {
    public static DistanceUnit DefaultUnit { get; set; } = DistanceUnit.Kilometers;

    protected override string Convert(Distance value, object parameter, CultureInfo culture)
    {
      string numberFormat = (string)parameter ?? "N3";
      string ret = value.To(DefaultUnit).Value.ToString(numberFormat) + " " + DefaultUnit.GetDisplayString();
      return ret;
    }

    protected override Distance ConvertBack(string value, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
