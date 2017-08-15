using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace MonoVarmint.Widgets
{


    //--------------------------------------------------------------------------------------
    /// <summary>
    /// VarmintWidget - A very simple widget class for MonoGame
    /// </summary>
    //--------------------------------------------------------------------------------------
    public partial class VarmintWidget
    {
        //--------------------------------------------------------------------------------------
        /// <summary>
        /// StretchParameter
        /// </summary>
        //--------------------------------------------------------------------------------------
        public class StretchParameter
        {
            public float? Horizontal { get; set; }
            public float? Vertical { get; set; }

            public StretchParameter() { }

            public StretchParameter(string valueText)
            {
                var parts = valueText.Split(',');
                if (parts.Length > 2) throw new ApplicationException("Too many values in stretch parameter specification.");

                Func<int, float?> parse = (index) =>
                {
                    if (index < parts.Length)
                    {
                        float output;
                        if (float.TryParse(parts[index], out output)) return output;
                    }
                    return null;
                };

                Horizontal = parse(0);
                Vertical = parse(1);
            }

            public override string ToString()
            {
                return string.Format("{0},{1}", Horizontal, Vertical);
            }
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// WidgetMargin
        /// </summary>
        //--------------------------------------------------------------------------------------
        public class WidgetMargin
        {
            public float? Left { get; set; }
            public float? Top { get; set; }
            public float? Right { get; set; }
            public float? Bottom { get; set; }

            public WidgetMargin() { }

            public WidgetMargin(string marginText)
            {
                var parts = marginText.Split(',');
                if (parts.Length > 4) throw new ApplicationException("Too many values in margin specification.");

                Func<int, float?> parse = (index) =>
                {
                    if (index < parts.Length)
                    {
                        float output;
                        if (float.TryParse(parts[index], out output)) return output;
                    }
                    return null;
                };

                Left = parse(0);
                Top = parse(1);
                Right = parse(2);
                Bottom = parse(3);
            }

            public WidgetMargin(float? left, float? top, float? right, float? bottom)
            {
                Left = left;
                Top = right;
                Right = top;
                Bottom = bottom;
            }

            public override string ToString()
            {
                return string.Format("{0},{1},{2},{3}", Left, Top, Right, Bottom);
            }
        }

    }
}