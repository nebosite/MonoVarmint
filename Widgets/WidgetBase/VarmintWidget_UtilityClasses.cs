using System;

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

                float? Parse(int index)
                {
                    if (index >= parts.Length) return null;
                    if (float.TryParse(parts[index], out var output))
                        return output;
                    return null;
                }

                Horizontal = Parse(0);
                Vertical = Parse(1);
            }

            public override string ToString()
            {
                return $"{Horizontal},{Vertical}";
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

                float? Parse(int index)
                {
                    if (index >= parts.Length) return null;
                    if (float.TryParse(parts[index], out var output)) return output;
                    return null;
                }

                Left = Parse(0);
                Top = Parse(1);
                Right = Parse(2);
                Bottom = Parse(3);
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
                return $"{Left},{Top},{Right},{Bottom}";
            }
        }

    }
}