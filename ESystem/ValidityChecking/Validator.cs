using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESystem.ValidityChecking
{
  public class Validator
  {
    private bool isActive = false;
    private readonly HashSet<object> checkedObjects = new HashSet<object>();
    public void Validate(object obj)
    {
      if (isActive)
        throw new ApplicationException("Validat cannot be started twice at once.");
      isActive = true;
      checkedObjects.Clear();

      CheckObject(obj);

      checkedObjects.Clear();
      isActive = false;
    }

    private void CheckObject(object obj)
    {
      var props = obj.GetType().GetProperties();
      foreach (var prop in props)
      {
        var propType = prop.GetType();
        var propValue = prop.GetValue(obj);
        if (propValue == null) continue;
        if (checkedObjects.Contains(propValue)) continue;
        checkedObjects.Add(propValue);

        if (propType.IsAssignableTo(typeof(IValidable)))
          CheckObjectValidity(propValue);
        CheckObject(propValue);
      }
    }

    private void CheckObjectValidity(object propValue)
    {
      IValidable val = (IValidable)propValue;
      val.CheckIsValid();
    }
  }
}
