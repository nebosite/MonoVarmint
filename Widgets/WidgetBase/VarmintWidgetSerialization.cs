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
        private readonly Dictionary<string, string> _declaredSettings = new Dictionary<string, string>();

        private static readonly Dictionary<string, Type> CachedWidgetTypes = new Dictionary<string, Type>();
        private static readonly List<Assembly> KnownAssemblies = new List<Assembly>();

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// DeclareAssembly - Let the widget system know about an assembly that provides types
        ///                   needed by the Widget space
        /// </summary>
        //--------------------------------------------------------------------------------------
        public static void DeclareAssembly(Assembly assembly)
        {
            if (!KnownAssemblies.Contains(assembly))
            {
                KnownAssemblies.Add(assembly);
            }
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// GetWidgetType - Find and cache the widget type
        /// </summary>
        //--------------------------------------------------------------------------------------
        private static Type GetWidgetType(string typeName)
        {
            if (CachedWidgetTypes.ContainsKey(typeName)) return CachedWidgetTypes[typeName];

            Type probableMatch = null;
            foreach (var assembly in KnownAssemblies)
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

                    if ((!foundName.EndsWith(typeName) || foundName[foundName.Length - typeName.Length - 1] != '.') &&
                        !typeName.StartsWith(".")) continue;
                    if (probableMatch != null)
                    {
                        throw new ApplicationException("Ambiguous Widget Type: " + typeName);
                    }
                    probableMatch = type;
                }
            }

            if (probableMatch != null)
            {
                if (!probableMatch.IsSubclassOf(typeof(VarmintWidget)))
                {
                    throw new ApplicationException("Widget node '" + typeName + "' is not a VarmintWidget. (Make sure your shortcut attribute does not match a real type.");
                }
                CachedWidgetTypes[typeName] = probableMatch;
                return probableMatch;
            }
            else
            {
                throw new ApplicationException("Could not find widget type: " + typeName);
            }
        }

        private readonly Dictionary<string, string> _bindingTemplates = new Dictionary<string, string>();
        private readonly BindingFlags _publicInstance = BindingFlags.Public | BindingFlags.Instance;
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
                throw new ApplicationException($"Property or event name '{propertyName}' is not part of {GetType().Name}");

            _bindingTemplates.Add(propertyName, bindingSpecifier);
        }

        //--------------------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------------------
        private void ThrowIfPropertyNotValid(string propertyName)
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
                    var valueParts = keyValuePair.Split(new[] { '=' }, 2);
                    if (valueParts.Length != 2) throw new ApplicationException("Expected format of Name=Value");
                    var types = propertyInfo.PropertyType.GenericTypeArguments;
                    var key = UIHelpers.GetValueFromText(types[0], valueParts[0]);
                    var value = UIHelpers.GetValueFromText(types[1], valueParts[1]);

                    // Create dictionary object if not there
                    var dictionary = propertyInfo.GetValue(this);
                    if(dictionary == null)
                    {
                        dictionary = Activator.CreateInstance(propertyInfo.PropertyType);
                        propertyInfo.SetValue(this, dictionary);
                    }

                    // Add to the dictionary
                    var add = propertyInfo.PropertyType.GetMethod("Add", types);
                    add.Invoke(dictionary, new[] { key, value });
                }
            }
            else
            {
                propertyInfo.SetValue(this, UIHelpers.GetValueFromText(propertyInfo.PropertyType, valueText));
            }
        }

        
        //--------------------------------------------------------------------------------------
        /// <summary>
        /// IsBinding - true if the text is a binding specifier
        /// </summary>
        //--------------------------------------------------------------------------------------
        private static bool IsBinding(string bindingSpecifier)
        {
            return bindingSpecifier.StartsWith("{") && bindingSpecifier.EndsWith("}");
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
            VarmintWidget output;

            void ApplyLayout(LayoutItem layout)
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
                    if (childItem.VwmlTag.Contains("."))
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
                            if (propertyType != null && !propertyType.PropertyType.IsInstanceOfType(hydratedLayout))
                            {
                                throw new ApplicationException("Property " + childItem.VwmlTag + " cannot be assigned Type " + hydratedLayout.GetType().Name);
                            }
                            propertyType?.SetValue(output, hydratedLayout);
                        }
                        else if (childItem.Children.Count > 1)
                        {
                            throw new ApplicationException("Too many child nodes on a property setter.  You only get one! (" + childItem.VwmlTag + ")");
                        }
                    }
                    else output.AddChild(HydrateLayout(injector, childItem, controlLibrary), true);
                }
            }


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
                ApplyLayout(controlLayout);
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

            ApplyLayout(widgetLayout);

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

                        var hasAttribute = reader.MoveToFirstAttribute();
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

            if (output != null && output.Name == null) output.Name = defaultName;
            return output;
        }


    }
}