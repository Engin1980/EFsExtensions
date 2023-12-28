using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Eng.Chlaot.ChlaotModuleBase.ModuleUtils.WPF.Converters
{
  public abstract class TypedConverter<TSource, TTarget> : IValueConverter
  {
    protected abstract TTarget Convert(TSource value, object parameter, CultureInfo culture);
    protected abstract TSource ConvertBack(TTarget value, object parameter, CultureInfo culture);

    protected virtual TSource ConvertToSource(object value, object parameter, CultureInfo culture) => (TSource)value;
    protected virtual TTarget ConvertToTarget(object value, object parameter, CultureInfo culture) => (TTarget)value;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      TTarget? ret;
      TSource? source;
      try
      {
        source = ConvertToSource(value, parameter, culture);
      }
      catch (Exception ex)
      {
        ex = BuildException($"Failed to cast source value {value}", ex);
        throw ex;
      }
      try
      {
        ret = Convert(source, parameter, culture);
      }
      catch (Exception ex)
      {
        ex = BuildException($"Failed to internally convert value {value} to target.", ex);
        throw ex;
      }
      return ret!;
    }

    private Exception BuildException(string msg, Exception cause)
    {
      Exception ret = new ApplicationException($"Converter  {this.GetType().Name} ({typeof(TSource).Name}->{typeof(TTarget).Name}) failed. " + msg, cause);
      return ret;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      TSource? ret;
      TTarget? target;
      try
      {
        target = ConvertToTarget(value, parameter, culture);
      }
      catch (Exception ex)
      {
        ex = BuildException($"Failed to cast target value {value}", ex);
        throw ex;
      }
      try
      {
        ret = ConvertBack(target, parameter, culture);
      }
      catch (Exception ex)
      {
        ex = BuildException($"Failed to internally convert-back value {value} to source.", ex);
        throw ex;
      }
      return ret!;
    }
  }
}
