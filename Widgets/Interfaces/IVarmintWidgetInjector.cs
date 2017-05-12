using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MonoVarmint.Widgets
{
    //--------------------------------------------------------------------------------------
    /// <summary>
    /// Use this to provide dependency injection when loading widgets from vwml files
    /// </summary>
    //--------------------------------------------------------------------------------------
    public interface IVarmintWidgetInjector
    {
        object GetInjectedValue(VarmintWidgetInjectAttribute attribute, PropertyInfo property);
    }
}
