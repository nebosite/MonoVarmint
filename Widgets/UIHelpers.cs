using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonoVarmint.Widgets
{
    public class UIHelpers
    {
        //--------------------------------------------------------------------------------------
        /// <summary>
        /// GetValueFromText - Convert text from a vwml file into an value
        /// </summary>
        //--------------------------------------------------------------------------------------
        public static object GetValueFromText(Type type, string valueText)
        {
            try
            {
                return GetBasicValueFromText(type.Name, valueText);
            }
            catch (Exception error)
            {
                if (type.IsEnum) return Enum.Parse(type, valueText);
                else if (type.IsClass) return Activator.CreateInstance(type, valueText);
                else throw error;
            }
        }

        public static object GetBasicValueFromText(string type, string valueText)
        {
            switch (type)
            {
                case "String": return valueText;
                case "Vector2": return ParseVector(valueText);
                case "Point": return ParsePoint(valueText);
                case "Single":
                case "float": return float.Parse(valueText);
                case "double": return double.Parse(valueText);
                case "Int32":
                case "int": return int.Parse(valueText);
                case "Int64":
                case "long": return long.Parse(valueText);
                case "Boolean":
                case "bool": return Boolean.Parse(valueText);
                case "Color": return ParseColor(valueText);
                default:
                    if (type == "Object") return valueText;
                    else throw new ApplicationException("Don't know create a " + type);
            }
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// ParseVector
        /// </summary>
        //--------------------------------------------------------------------------------------
        public static Vector2 ParseVector(string text)
        {
            var parts = text.Split(',');
            if (parts.Length != 2)
            {
                throw new ApplicationException("Bad Vector Specification");
            }
            return new Vector2(float.Parse(parts[0].Trim()), float.Parse(parts[1].Trim()));
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Point
        /// </summary>
        //--------------------------------------------------------------------------------------
        public static Point ParsePoint(string text)
        {
            var parts = text.Split(',');
            if (parts.Length != 2)
            {
                throw new ApplicationException("Bad Point Specification");
            }
            return new Point(int.Parse(parts[0].Trim()), int.Parse(parts[1].Trim()));
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// ParseColor
        /// </summary>
        //--------------------------------------------------------------------------------------
        public static Color ParseColor(string text)
        {
            // #ARGB - formatted
            if (text.StartsWith("#"))
            {
                var rawColor = Convert.ToUInt32(text.Substring(1), 16);
                var blue = (int)(rawColor & 0xff);
                rawColor >>= 8;
                var green = (int)(rawColor & 0xff);
                rawColor >>= 8;
                var red = (int)(rawColor & 0xff);
                rawColor >>= 8;
                var alpha = (int)(rawColor & 0xff);
                if (text.Length <= 7) alpha = 255;
                return Color.FromNonPremultiplied(red, green, blue, alpha);
            }

            // Color name formatted
            var colorType = typeof(Color);
            var colorProperty = colorType.GetProperty(text);
            if (colorProperty == null) throw new ApplicationException("Unknown color: " + text);
            return (Color)colorProperty.GetValue(null, null);
        }
    }
}