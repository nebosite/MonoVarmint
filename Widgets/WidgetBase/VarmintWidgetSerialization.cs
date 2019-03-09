using Microsoft.Xna.Framework;
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
        internal static Type GetWidgetType(string typeName)
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
                if(propertyName == "Size")
                {
                    var value = UIHelpers.GetValueFromText(typeof(Tuple<float?,float?>), valueText);
                    SpecifiedSize = (Tuple<float?, float?>)value;
                }
                else
                {
                    var value = UIHelpers.GetValueFromText(propertyInfo.PropertyType, valueText);
                    propertyInfo.SetValue(this, value);
                }
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
        internal void AddSetting(string name, string value)
        {
            _declaredSettings[name] = value;
        }
    }
}