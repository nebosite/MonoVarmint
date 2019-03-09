using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

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
        public Dictionary<string, VarmintWidgetStyle> StyleLibrary => _styleLibrary;
        public List<VarmintWidgetNineSlice> NineSlices = new List<VarmintWidgetNineSlice>();

        private Dictionary<string, VarmintWidgetStyle> _styleLibrary = new Dictionary<string, VarmintWidgetStyle>();
        private Dictionary<string, VarmintWidget.LayoutItem> _controlLibrary = new Dictionary<string, VarmintWidget.LayoutItem>();
        private Dictionary<string, VarmintWidget> _screensByName = new Dictionary<string, VarmintWidget>();
        private Dictionary<string, VarmintWidget> _widgetsByName = new Dictionary<string, VarmintWidget>();
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
                    System.Diagnostics.Debug.WriteLine("---- " + resourceName);

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
            if(!_widgetsByName.TryGetValue(widgetName, out var widget))
            {
                foreach(var screen in _screensByName.Values)
                {
                    widget = screen.FindWidgetByName(widgetName);
                    if(widget != null)
                    {
                        break;
                    }
                }

                if(widget != null)
                {
                    _widgetsByName[widgetName] = widget;
                }
            }

            return widget;
        }

        //-----------------------------------------------------------------------------------------------
        /// <summary>
        /// AddContent - Add or replace a vwml item in the widget space.  Use this method to add
        ///              content you might want to change dynamically
        /// </summary>
        //-----------------------------------------------------------------------------------------------
        public void AddContent(string name, Stream vwmlStream)
        {
            var layout = VarmintWidget.PreloadFromVwml(vwmlStream, name);

            if(layout.VwmlTag == "Style")
            {
                var rootStyle = (VarmintWidgetStyle)VarmintWidget.HydrateLayout(_injector, layout, _controlLibrary);
                foreach (var style in rootStyle.FindWidgetsByType<VarmintWidgetStyle>())
                {
                    if (_styleLibrary.ContainsKey(style.Name))
                    {
                        Debug.WriteLine("Warning: Added duplicate style: " + style.Name);
                    }
                    _styleLibrary[style.Name] = style;
                }
            }
            else if(layout.VwmlTag == "Control")
            {
                _controlLibrary.Add(layout.Name, layout);
            }
            else
            {
                var screen = VarmintWidget.HydrateLayout(_injector, layout, _controlLibrary);
                if(layout.VwmlTag == "NineSlice")
                {
                    NineSlices.Add(screen as VarmintWidgetNineSlice);
                }
                else _screensByName[screen.Name] = screen;
            }
        }

    }
}
