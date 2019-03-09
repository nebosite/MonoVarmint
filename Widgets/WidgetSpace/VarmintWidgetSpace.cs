using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

namespace MonoVarmint.Widgets
{
    //-----------------------------------------------------------------------------------------------
    /// <summary>
    /// VarmintWidgetSpace - This class is for loading and tracking a family of widgets to be used
    ///                      in a single application
    /// </summary>
    //-----------------------------------------------------------------------------------------------
    public class VarmintWidgetSpace
    {
        //public Dictionary<string, VarmintWidgetStyle> StyleLibrary => _layoutLibrary;
        public List<VarmintWidgetNineSlice> NineSlices = new List<VarmintWidgetNineSlice>();
        public Dictionary<string, VarmintWidgetStyle> StyleLibrary = new Dictionary<string, VarmintWidgetStyle>();

        private Dictionary<string, LayoutItem> _controlLibrary = new Dictionary<string, LayoutItem>();
        private Dictionary<string, VarmintWidget> _hydratedWidgets = new Dictionary<string, VarmintWidget>();
        private IVarmintWidgetInjector _injector;


        //--------------------------------------------------------------------------------------
        /// <summary>
        /// ctor
        /// </summary>
        //--------------------------------------------------------------------------------------
        public VarmintWidgetSpace(IVarmintWidgetInjector injector)
        {
            _injector = injector;
            var assembly = injector.GetType().GetTypeInfo().Assembly;
            VarmintWidget.DeclareAssembly(assembly);

            // automatically add embedded layout by preloading the raw layout without hydrating it
            foreach (var resourceName in assembly.GetManifestResourceNames())
            {
                if (resourceName.ToLower().EndsWith(".vwml"))
                {
                    Debug.WriteLine("---- Injesting " + resourceName);

                    var nameParts = resourceName.Split('.');
                    var defaultName = nameParts[nameParts.Length - 2];
                    AddContent(defaultName, assembly.GetManifestResourceStream(resourceName));
                }
            }
        }

        //-----------------------------------------------------------------------------------------------
        /// <summary>
        /// Find any widget by name
        /// </summary>
        //-----------------------------------------------------------------------------------------------
        public VarmintWidget FindWidgetByName(string widgetName)
        {
            if(!_hydratedWidgets.TryGetValue(widgetName, out var widget))
            {
                foreach(var topWidget in _hydratedWidgets.Values)
                {
                    widget = topWidget.FindWidgetByName(widgetName);
                    if (widget != null) break;
                }
            }

            return widget;
        }

        //-----------------------------------------------------------------------------------------------
        /// <summary>
        /// AddContent - Add layout content to widget space
        /// </summary>
        //-----------------------------------------------------------------------------------------------
        public void AddContent(string defaultName, string vwmlText)
        {
            using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(vwmlText)))
            {
                AddContent(defaultName, memoryStream);
            }
        }

        //-----------------------------------------------------------------------------------------------
        /// <summary>
        /// AddContent - Add layout content to widget space
        /// </summary>
        //-----------------------------------------------------------------------------------------------
        public void AddContent(string defaultName, Stream vwmlStream)
        {
            var layout = PreloadFromVwml(vwmlStream, defaultName);

            if (layout.VwmlTag == "Control")
            {
                _controlLibrary[layout.Name] = layout;
            }
            else if(layout.VwmlTag == "Style")
            {
                var hydratedStyle = HydrateLayout(layout);
                var queue = new Queue<VarmintWidget>();
                queue.Enqueue(hydratedStyle);
                while(queue.Count > 0)
                {
                    var item = queue.Dequeue();
                    StyleLibrary.Add(item.Name, item as VarmintWidgetStyle);
                    foreach(var child in item.Children)
                    {
                        if(child is VarmintWidgetStyle)
                        {
                            queue.Enqueue(child);
                        }
                    }
                }

            }
            else
            {
                var hydratedWidget = HydrateLayout(layout);
                if (hydratedWidget is VarmintWidgetNineSlice)
                {
                    NineSlices.Add(hydratedWidget as VarmintWidgetNineSlice);
                }
                else
                {
                    _hydratedWidgets.Add(hydratedWidget.Name, hydratedWidget);
                }
            }
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// HydrateLayout - Take a layout item and turn it into a VarmintWidget
        /// </summary>
        //--------------------------------------------------------------------------------------
        private VarmintWidget HydrateLayout(LayoutItem widgetLayout)
        {
            VarmintWidget newWidget = null;

            // Helper method to apply this layout to the widget we created.
            void ApplyLayout(LayoutItem layout)
            {
                // Get the settings explicitly specified in the layout
                foreach (var name in layout.Settings.Keys)
                {
                    switch (name)
                    {
                        case "Style":
                        case "ApplyTo":
                            newWidget.SetValue(name, layout.Settings[name], true);
                            break;
                        default:
                            newWidget.AddSetting(name, layout.Settings[name]);
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

                        var propertyType = newWidget.GetType().GetProperty(parts[1]);
                        if (childItem.Children.Count == 1) // Only add items with content
                        {
                            var hydratedLayout = HydrateLayout(childItem.Children[0]);
                            if (propertyType != null && !propertyType.PropertyType.IsInstanceOfType(hydratedLayout))
                            {
                                throw new ApplicationException("Property " + childItem.VwmlTag + " cannot be assigned Type " + hydratedLayout.GetType().Name);
                            }
                            propertyType?.SetValue(newWidget, hydratedLayout);
                        }
                        else if (childItem.Children.Count > 1)
                        {
                            throw new ApplicationException("Too many child nodes on a property setter.  You only get one! (" + childItem.VwmlTag + ")");
                        }
                    }
                    else newWidget.AddChild(HydrateLayout(childItem));
                }
            }

            // If the vwmlTag is in the layouts, it must be a control
            if (_controlLibrary.ContainsKey(widgetLayout.VwmlTag))
            {
                var controlLayout = _controlLibrary[widgetLayout.VwmlTag];

                // If there is a class that is bound to this control then we bind events to that class
                if (controlLayout.Settings.ContainsKey("Class"))
                {
                    var controlType = VarmintWidget.GetWidgetType(controlLayout.Settings["Class"]);
                    if (!controlType.IsSubclassOf(typeof(VarmintWidgetControl)))
                    {
                        throw new ApplicationException("The Class attribute must point to a VarmintWidgetControl");
                    }
                    newWidget = (VarmintWidget)Activator.CreateInstance(controlType);
                    newWidget.ControlHandler = newWidget;
                }
                else
                {
                    newWidget = new VarmintWidgetControl();
                }

                // Controls will get properties from the control layout, but these can 
                // be overridden later by the local instace of the control
                ApplyLayout(controlLayout);
            }
            else
            {
                var nodeType = VarmintWidget.GetWidgetType(widgetLayout.VwmlTag);
                newWidget = (VarmintWidget)Activator.CreateInstance(nodeType);
            }
            if (widgetLayout.Name != null) newWidget.Name = widgetLayout.Name;

            // Automatically inject property values if they are injectable
            foreach (var propertyType in newWidget.GetType().GetProperties())
            {
                var injectAttribute = (VarmintWidgetInjectAttribute)propertyType.GetCustomAttribute(typeof(VarmintWidgetInjectAttribute));
                if (injectAttribute != null)
                {
                    propertyType.SetValue(newWidget, _injector.GetInjectedValue(injectAttribute, propertyType));
                }
            }

            ApplyLayout(widgetLayout);
            return newWidget;
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Helper class for representing layout
        /// </summary>
        //--------------------------------------------------------------------------------------
        public class LayoutItem
        {
            public string VwmlTag { get; set; }
            public string Name { get; set; }
            public string Style { get; set; }
            public string ApplyTo { get; set; }
            public string LocationText { get; set; }

            public List<LayoutItem> Children { get; set; }
            public LayoutItem Root { get; internal set; }
            public bool IsHydrated { get; internal set; }

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
                    throw new ApplicationException($"VWML parse error in '{defaultName}' on line " + lineInfo.LineNumber
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
