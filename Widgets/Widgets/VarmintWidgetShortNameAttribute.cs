using System;
using System.Collections.Generic;
using System.Text;

namespace MonoVarmint.Widgets
{
    public class VarmintWidgetShortNameAttribute : Attribute
    {
        public readonly string ShortName;

        public VarmintWidgetShortNameAttribute(string shortName)
        {
            this.ShortName = shortName;
        }
    }
}
