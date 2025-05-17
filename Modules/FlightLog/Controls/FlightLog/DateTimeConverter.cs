using ESystem.WPF;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eng.EFsExtensions.Modules.FlightLogModule.Controls.FlightLog
{
    public class DateTimeConverter : TypedConverter<DateTime?, string>
    {
        public static string DefaultFormat { get; set; } = "yyyy-MM-dd HH:mm:ss";

        protected override string Convert(DateTime? value, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;
            string format = parameter as string ?? DefaultFormat;
            return value.Value.ToString(format, culture);
        }

        protected override DateTime? ConvertBack(string value, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
