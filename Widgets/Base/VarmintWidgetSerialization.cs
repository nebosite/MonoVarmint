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
    /// VarmintWidget 
    /// </summary>
    //--------------------------------------------------------------------------------------
    public partial class VarmintWidget
    {
        //--------------------------------------------------------------------------------------
        /// <summary>
        /// LoadLayout - Search the implicit assembly for all valid vwml files  and build a 
        ///              dictionary of the visual elements.   The assembly will be searched 
        ///              for any symbols specified in the vwml. 
        ///                 
        /// </summary>
        //--------------------------------------------------------------------------------------
        internal static Dictionary<string, VarmintWidget> LoadLayout(IVarmintWidgetInjector injector, object bindingContext = null)
        {
            var output = new Dictionary<string, VarmintWidget>();
            var assembly = injector.GetType().GetTypeInfo().Assembly;
            if (!_knownAssemblies.Contains(assembly))
            {
                _knownAssemblies.Add(assembly);
            }

            foreach (var resourceName in assembly.GetManifestResourceNames())
            {
                if (resourceName.ToLower().EndsWith(".vwml"))
                {
                    System.Diagnostics.Debug.WriteLine("---- " + resourceName);

                    var nameParts = resourceName.Split('.');
                    var defaultName = nameParts[nameParts.Length - 2];
                    var rootWidget = LoadLayoutFromVwml(injector, assembly.GetManifestResourceStream(resourceName), defaultName);
                    rootWidget.UpdateChildFormatting(true);
                    output.Add(rootWidget.Name, rootWidget);
                    if(bindingContext != null)
                    {
                        rootWidget.BindingContext = bindingContext;
                        rootWidget.Init();
                    }
                }
            }

            return output;
        }

        private static Dictionary<string, Type> _cachedWidgetTypes = new Dictionary<string, Type>();
        private static List<Assembly> _knownAssemblies = new List<Assembly>();

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// GetWidgetType - Find and cache the widget type
        /// </summary>
        //--------------------------------------------------------------------------------------
        private static Type GetWidgetType(string typeName)
        {
            if (_cachedWidgetTypes.ContainsKey(typeName)) return _cachedWidgetTypes[typeName];

            Type probableMatch = null;
            foreach (var assembly in _knownAssemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    var shortNameAttribute = (VarmintWidgetShortNameAttribute)type.GetCustomAttribute(typeof(VarmintWidgetShortNameAttribute));
                    if (shortNameAttribute != null && shortNameAttribute.ShortName == typeName)
                    {
                        probableMatch = type;
                        break;
                    }

                    var foundName = type.Name;
                    if (typeName == foundName)
                    {
                        probableMatch = type;
                        break;
                    }

                    if (foundName.EndsWith(typeName)
                        && (foundName[foundName.Length - typeName.Length - 1] == '.')
                            || typeName.StartsWith("."))
                    {
                        if (probableMatch != null)
                        {
                            throw new ApplicationException("Ambiguous Widget Type: " + typeName);
                        }
                        probableMatch = type;
                        return type;
                    }
                }
            }

            if (probableMatch != null)
            {
                if (!probableMatch.IsSubclassOf(typeof(VarmintWidget)))
                {
                    throw new ApplicationException("Widget node '" + typeName + "' is not a VarmintWidget. (Make sure your shortcut attribute does not match a real type.");
                }
                _cachedWidgetTypes[typeName] = probableMatch;
                return probableMatch;
            }
            else
            {
                throw new ApplicationException("Could not find widget type: " + typeName);
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
            return new Vector2(float.Parse(parts[0]), float.Parse(parts[1]));
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

        Dictionary<string, string> _bindingTemplates = new Dictionary<string, string>();
        BindingFlags _publicInstance = BindingFlags.Public | BindingFlags.Instance;
        //--------------------------------------------------------------------------------------
        /// <summary>
        /// AddBinding
        /// </summary>
        //--------------------------------------------------------------------------------------
        public void AddBinding(string propertyName, string bindingSpecifier)
        {
            var propertyType = GetType().GetProperty(propertyName, _publicInstance);
            var eventType = GetType().GetEvent(propertyName, _publicInstance);
            if (propertyType == null && eventType == null)
                throw new ApplicationException("Property or event name '" + propertyName + "' is not part of type " + propertyType.Name);

            _bindingTemplates.Add(propertyName, bindingSpecifier);
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// SetValue - Set a property to a literal value
        /// </summary>
        //--------------------------------------------------------------------------------------
        internal void SetValue(string propertyName, string valueText)
        {
            var eventInfo = GetType().GetTypeInfo().GetEvent(propertyName, _publicInstance);
            if (eventInfo != null)
            {
                AddBinding(propertyName, valueText);
                return;
            }
            var propertyInfo = GetType().GetTypeInfo().GetProperty(propertyName);
            if (propertyInfo == null)
                throw new ApplicationException("Property '" + propertyName + "' not found on type " + GetType());
            var targetTypeName = propertyInfo.PropertyType.Name;

            switch (targetTypeName)
            {
                case "String": propertyInfo.SetValue(this, valueText); break;
                case "Vector2": propertyInfo.SetValue(this, ParseVector(valueText)); break;
                case "Single":
                case "float": propertyInfo.SetValue(this, float.Parse(valueText)); break;
                case "double": propertyInfo.SetValue(this, double.Parse(valueText)); break;
                case "int": propertyInfo.SetValue(this, int.Parse(valueText)); break;
                case "Boolean":
                case "bool": propertyInfo.SetValue(this, Boolean.Parse(valueText)); break;
                case "Color": propertyInfo.SetValue(this, ParseColor(valueText)); break;
                default:
                    if (propertyInfo.PropertyType.IsEnum)
                    {
                        propertyInfo.SetValue(this, Enum.Parse(propertyInfo.PropertyType, valueText));
                    }
                    else if (propertyInfo.PropertyType.Name == "Object")
                    {
                        propertyInfo.SetValue(this, valueText);
                    }
                    else if (propertyInfo.PropertyType.IsClass)
                    {
                        propertyInfo.SetValue(this, Activator.CreateInstance(propertyInfo.PropertyType, valueText));
                    }
                    else throw new ApplicationException("Don't know how to set a " + targetTypeName);
                    break;
            }
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// IsBinding - true if the text is a binding specifier
        /// </summary>
        //--------------------------------------------------------------------------------------
        private static bool IsBinding(string bindingSpecifier)
        {
            return (bindingSpecifier.StartsWith("{") && bindingSpecifier.EndsWith("}"));
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// GetNextWidget - Recursively get the next widget from the xml reader
        /// </summary>
        //--------------------------------------------------------------------------------------
        private static VarmintWidget GetNextWidget(IVarmintWidgetInjector injector, XmlReader reader, string defaultName = null)
        {
            var nodeName = "";
            VarmintWidget output = null;
            List<VarmintWidget> childrenToAdd = new List<VarmintWidget>();

            do
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (output == null)
                    {
                        nodeName = reader.Name;
                        if (nodeName == "VarmintWidget")
                        {
                            throw new ApplicationException("You must specify a widget class derived from VarmintWidget");
                        }
                        var nodeType = GetWidgetType(nodeName);

                        output = (VarmintWidget)Activator.CreateInstance(nodeType);
                        if (defaultName != null) output.Name = defaultName;
                        foreach (var propertyType in nodeType.GetProperties())
                        {
                            var injectAttribute = (VarmintWidgetInjectAttribute)propertyType.GetCustomAttribute(typeof(VarmintWidgetInjectAttribute));
                            if (injectAttribute != null)
                            {
                                propertyType.SetValue(output,
                                    injector.GetInjectedValue(injectAttribute, propertyType));
                            }
                        }

                        bool hasAttribute = reader.MoveToFirstAttribute();
                        while (hasAttribute)
                        {
                            if (IsBinding(reader.Value))
                            {
                                output.AddBinding(reader.Name, reader.Value);
                            }
                            else
                            {
                                output.SetValue(reader.Name, reader.Value);
                            }

                            hasAttribute = reader.MoveToNextAttribute();
                        }
                        reader.MoveToElement();

                        if (reader.IsEmptyElement)
                        {
                            break;
                        }
                    }
                    else
                    {
                        childrenToAdd.Add(GetNextWidget(injector, reader));
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Name != nodeName)
                    {
                        throw new ApplicationException("Expected end of '" + nodeName + "' but got end of '" + reader.Name + "'");
                    }
                    break;
                }

                if (output == null) continue;
            } while (reader.Read());

            output.AddChildren(childrenToAdd.ToArray(), true);
            return output;
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// LoadLayoutFrom - Load a visual element tree from a raw vwml file
        ///                  
        ///                 The provided assembly will be searched for any symbols
        ///                 specified in the vwml 
        /// </summary>
        //--------------------------------------------------------------------------------------
        internal static VarmintWidget LoadLayoutFromVwml(IVarmintWidgetInjector injector, Stream vwmlStream, string defaultName)
        {
            using (var reader = XmlReader.Create(vwmlStream))
            {
                var lineInfo = (IXmlLineInfo)reader;
                try
                {
                    return GetNextWidget(injector, reader, defaultName);
                }
                catch (Exception e)
                {
                    throw new ApplicationException("VWML parse error on line " + lineInfo.LineNumber
                        + ": " + e.Message, e);
                }
            }
        }

    }
}