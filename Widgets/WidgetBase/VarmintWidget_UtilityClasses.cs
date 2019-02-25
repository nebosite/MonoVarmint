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
                if(parts.Length == 1)
                {
                    Top = Right = Bottom = Left;
                }
                else
                {
                    Top = Parse(1);
                    if(parts.Length == 2)
                    {
                        Right = Left;
                        Bottom = Top;
                    }
                    else
                    {
                        Right = Parse(2);
                        Bottom = Parse(3);
                    }
                }
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