using ESystem.Asserting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESystem
{
  public static class Functions
  {
    public static void TryOr(Action tryAction, Action<Exception> errorAction)
    {
      EAssert.Argument.IsNotNull(tryAction);
      EAssert.Argument.IsNotNull(errorAction);

      try
      {
        tryAction();
      }
      catch (Exception ex)
      {
        errorAction(ex);
      }
    }

    public static T TryOr<T>(Func<T> tryFunction, Func<Exception, T> errorAction)
    {
      EAssert.Argument.IsNotNull(tryFunction);
      EAssert.Argument.IsNotNull(errorAction);
      T ret;
      try
      {
        ret = tryFunction();
      }
      catch (Exception ex)
      {
        ret = errorAction(ex);
      }
      return ret;
    }

    public static void Try(Action tryAction, Func<Exception, Exception> exceptionProducer)
    {
      try
      {
        tryAction.Invoke();
      }
      catch (Exception ex)
      {
        Exception newException = exceptionProducer.Invoke(ex);
        throw newException;
      }
    }

    public static T Try<T>(Func<T> tryFunc, Func<Exception, Exception> exceptionProducer)
    {
      T ret;
      try
      {
        ret = tryFunc.Invoke();
      }
      catch (Exception ex)
      {
        Exception newException = exceptionProducer.Invoke(ex);
        throw newException;
      }
      return ret;
    }
  }
}
