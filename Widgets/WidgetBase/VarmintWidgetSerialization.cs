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
        Dictionary<string, string> _declaredSettings = new Dictionary<string, string>();

        private static Dictionary<string, Type> _cachedWidgetTypes = new Dictionary<string, Type>();
        private static List<Assembly> _knownAssemblies = new List<Assembly>();

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// DeclareAssembly - Let the widget system know about an assembly that provides types
        ///                   needed by the Widget space
        /// </summary>
        //--------------------------------------------------------------------------------------
        public static void DeclareAssembly(Assembly assembly)
        {
            if (!_knownAssemblies.Contains(assembly))
            {
                _knownAssemblies.Add(assembly);
            }
        }

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
        //
        //--------------------------------------------------------------------------------------
        void ThrowIfPropertyNotValid(string propertyName)
        {
            var eventInfo = GetType().GetTypeInfo().GetEvent(propertyName, _publicInstance);
            var propertyInfo = GetType().GetTypeInfo().GetProperty(propertyName);
            if (propertyInfo == null && eventInfo == null)
            {
                throw new ApplicationException("Cannot find property " + propertyName + " on type " + GetType().Name);
            }
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// SetValue - Set a property to a literal value
        /// </summary>
        //--------------------------------------------------------------------------------------
        internal void SetValue(string propertyName, string valueText, bool throwOnMissingProperty)
        {
            var eventInfo = GetType().GetTypeInfo().GetEvent(propertyName, _publicInstance);
            if (eventInfo != null)
            {
                AddBinding(propertyName, valueText);
                return;
            }
            var propertyInfo = GetType().GetTypeInfo().GetProperty(propertyName);
            if (propertyInfo == null)
            {
                if (throwOnMissingProperty) throw new ApplicationException("Cannot find property " + propertyName + " on type " + GetType().Name);
                return;
            }

            // If the property is a dictionary, then we'll try to add this value, otherwise, we set the property
            if (propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.Name == "Dictionary`2")
            {
                // Break up string into separate key/Value pairs
                var keyValuePairs = valueText.Split('`');
                foreach(var keyValuePair in keyValuePairs)
                {
                    // Break up name=value into actual key and value objects
                    var valueParts = keyValuePair.Split(new char[] { '=' }, 2);
                    if (valueParts.Length != 2) throw new ApplicationException("Expected format of Name=Value");
                    var types = propertyInfo.PropertyType.GenericTypeArguments;
                    var key = GetValueFromText(types[0], valueParts[0]);
                    var value = GetValueFromText(types[1], valueParts[1]);

                    // Create dictionary object if not there
                    var dictionary = propertyInfo.GetValue(this);
                    if(dictionary == null)
                    {
                        dictionary = Activator.CreateInstance(propertyInfo.PropertyType);
                        propertyInfo.SetValue(this, dictionary);
                    }

                    // Add to the dictionary
                    var add = propertyInfo.PropertyType.GetMethod("Add", types);
                    add.Invoke(dictionary, new object[] { key, value });
                }
            }
            else
            {
                propertyInfo.SetValue(this, GetValueFromText(propertyInfo.PropertyType, valueText));
            }
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// GetValueFromText - Convert text from a vwml file into an value
        /// </summary>
        //--------------------------------------------------------------------------------------
        private object GetValueFromText(Type type, string valueText)
        {
            switch (type.Name)
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
                    if (type.IsEnum) return Enum.Parse(type, valueText);
                    else if (type.Name == "Object") return valueText;
                    else if (type.IsClass) return Activator.CreateInstance(type, valueText);
                    else throw new ApplicationException("Don't know create a " + type.Name);
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
        /// AddSetting - remember a name/value setting used to define this object
        /// </summary>
        //--------------------------------------------------------------------------------------
        private void AddSetting(string name, string value)
        {
            _declaredSettings[name] = value;
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// HydrateLayout - Take a layout item and turn it into a VarmintWidget
        /// </summary>
        //--------------------------------------------------------------------------------------
        public static VarmintWidget HydrateLayout(IVarmintWidgetInjector injector, LayoutItem widgetLayout, Dictionary<string, LayoutItem> controlLibrary)
        {
            VarmintWidget output = null;

            Action<LayoutItem> applyLayout = (layout) =>
            {
                foreach (var name in layout.Settings.Keys)
                {
                    switch (name)
                    {
                        case "Style":
                        case "ApplyTo":
                            output.SetValue(name, layout.Settings[name], true);
                            break;
                        default:
                            output.AddSetting(name, layout.Settings[name]);
                            break;
                    }
                }
                foreach (var childItem in layout.Children)
                {
                    if(childItem.VwmlTag.Contains("."))
                    {
                        var parts = childItem.VwmlTag.Split('.');
                        if (parts.Length > 2)
                        {
                            throw new ApplicationException("Property setter specification is too deep.  Only one dot allowed! (" + childItem.VwmlTag + ")");
                        }

                        var propertyType = output.GetType().GetProperty(parts[1]);
                        if (childItem.Children.Count == 1) // Only add items with content
                        {
                            var hydratedLayout = HydrateLayout(injector, childItem.Children[0], controlLibrary);
                            if (!propertyType.PropertyType.IsAssignableFrom(hydratedLayout.GetType()))
                            {
                                throw new ApplicationException("Property " + childItem.VwmlTag + " cannot be assigned Type " + hydratedLayout.GetType().Name);
                            }
                            propertyType.SetValue(output, hydratedLayout);
                        }
                        else if(childItem.Children.Count > 1)
                        {
                            throw new ApplicationException("Too many child nodes on a property setter.  You only get one! (" + childItem.VwmlTag + ")");
                        }
                    }
                    else output.AddChild(HydrateLayout(injector, childItem, controlLibrary), true);
                }
            };


            if(controlLibrary.ContainsKey(widgetLayout.VwmlTag))
            {
                var controlLayout = controlLibrary[widgetLayout.VwmlTag];

                if(controlLayout.Settings.ContainsKey("Class"))
                {
                    var controlType = GetWidgetType(controlLayout.Settings["Class"]);
                    if(!controlType.IsSubclassOf(typeof(VarmintWidgetControl)))
                    {
                        throw new ApplicationException("The Class attribute must point to a VarmintWidgetControl");
                    }
                    output = (VarmintWidget)Activator.CreateInstance(controlType);
                    output.EventBindingContext = output;
                }
                else
                {
                    output = new VarmintWidgetControl();
                }

                // Controls will get properties from the control layout, but these can 
                // be overridden later by the local instace of the control
                applyLayout(controlLayout);
            }
            else
            {
                var nodeType = GetWidgetType(widgetLayout.VwmlTag);
                output = (VarmintWidget)Activator.CreateInstance(nodeType);
            }
            if (widgetLayout.Name != null) output.Name = widgetLayout.Name;

            foreach (var propertyType in output.GetType().GetProperties())
            {
                var injectAttribute = (VarmintWidgetInjectAttribute)propertyType.GetCustomAttribute(typeof(VarmintWidgetInjectAttribute));
                if (injectAttribute != null)
                {
                    propertyType.SetValue(output,
                        injector.GetInjectedValue(injectAttribute, propertyType));
                }
            }

            applyLayout(widgetLayout);

            return output;
        }


        public class LayoutItem
        {
            public string VwmlTag { get; set; }
            public string Name { get; set; }
            public string Style { get; set; }
            public string ApplyTo { get; set; }
            public string LocationText { get; set; }

            public List<LayoutItem> Children { get; set; }

            public Dictionary<string, string> Settings = new Dictionary<string, string>();

            public LayoutItem(string tagName)
            {
                VwmlTag = tagName;
                Children = new List<LayoutItem>();
            }
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// LoadLayoutFrom - Load a visual element tree from a raw vwml file
        ///                  
        ///                 The provided assembly will be searched for any symbols
        ///                 specified in the vwml 
        /// </summary>
        //--------------------------------------------------------------------------------------
        public static LayoutItem PreloadFromVwml(Stream vwmlStream, string defaultName)
        {
            using (var reader = XmlReader.Create(vwmlStream))
            {
                var lineInfo = (IXmlLineInfo)reader;
                try
                {
                    return GetNextNode(reader, defaultName, defaultName);
                }
                catch (Exception e)
                {
                    throw new ApplicationException("VWML parse error on line " + lineInfo.LineNumber
                        + ": " + e.Message, e);
                }
            }
        }

        //--------------------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------------------
        private static LayoutItem GetNextNode(XmlReader reader, string defaultName, string entityName)
        {
            var nodeName = "";
            LayoutItem output = null;

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

                        output = new LayoutItem(nodeName);
                        var lineInfo = (IXmlLineInfo)reader;
                        output.LocationText = entityName + ", Line: " + lineInfo.LineNumber;

                        bool hasAttribute = reader.MoveToFirstAttribute();
                        while (hasAttribute)
                        {
                            // Some properties are special, and need to be set now.  The rest we set
                            // at the last moment when we have all the styles to inform us.
                            switch (reader.Name)
                            {
                                case "Name": output.Name = reader.Value; break;
                                default:
                                    output.Settings.Add(reader.Name, reader.Value);
                                    break;
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
                        output.Children.Add(GetNextNode(reader, null, entityName));
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

            if (output.Name == null) output.Name = defaultName;
            return output;
        }


    }
}