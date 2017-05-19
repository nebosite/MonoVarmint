using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace MonoVarmint.Widgets
{
    //--------------------------------------------------------------------------------------
    /// <summary>
    /// VarmintWidgetStyle - A special widget to hold style data
    /// </summary>
    //--------------------------------------------------------------------------------------
    [VarmintWidgetShortName("Style")]
    public class VarmintWidgetStyle : VarmintWidget
    {
        HashSet<string> _applications = new HashSet<string>();
        public string ApplyTo
        {
            set
            {
                foreach(var app in value.Split('`'))
                {
                    _applications.Add(app);
                }
            }
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// ctor
        /// </summary>
        //--------------------------------------------------------------------------------------
        public VarmintWidgetStyle()
        {
            this.OnRender += (gt,w) => throw new ApplicationException("Styles cannot be rendered");
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// AppliesToMe - return true if this style applies to this type
        /// </summary>
        //--------------------------------------------------------------------------------------
        public bool AppliesToMe(object styleMe)
        {
            var type = styleMe.GetType();
            if (_applications.Contains(type.Name)) return true;

            var shortNameAttribute = (VarmintWidgetShortNameAttribute)type.GetCustomAttribute(typeof(VarmintWidgetShortNameAttribute));
            return (shortNameAttribute != null && _applications.Contains(shortNameAttribute.ShortName));
        }

    }
}