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
        public Dictionary<string, VarmintWidgetStyle> StyleLibrary { get { return _styleLibrary; } }

        private Dictionary<string, VarmintWidgetStyle> _styleLibrary = new Dictionary<string, VarmintWidgetStyle>();
        private Dictionary<string, VarmintWidget.LayoutItem> _controlLibrary = new Dictionary<string, VarmintWidget.LayoutItem>();
        private Dictionary<string, VarmintWidget.LayoutItem> _screenLibrary = new Dictionary<string, VarmintWidget.LayoutItem>();
        private Dictionary<string, VarmintWidget> _screensByName = new Dictionary<string, VarmintWidget>();
        private IVarmintWidgetInjector _injector;


        //--------------------------------------------------------------------------------------
        /// <summary>
        /// ctor
        /// </summary>
        //--------------------------------------------------------------------------------------
        public VarmintWidgetSpace(IVarmintWidgetInjector injector, object bindingContext = null)
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
                    AddContent(defaultName, assembly.GetManifestResourceStream(resourceName), bindingContext);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------
        /// <summary>
        /// GetScreen - return the named screen hydrated with bound data 
        /// </summary>
        //-----------------------------------------------------------------------------------------------
        internal VarmintWidget GetScreen(string screenName, object bindingContext)
        {
            if (!_screensByName.ContainsKey(screenName))
            {
                if(_screenLibrary.ContainsKey(screenName))
                {
                    var screen = VarmintWidget.HydrateLayout(_injector, _screenLibrary[screenName], _controlLibrary);
                    if (bindingContext != null)
                    {
                        screen.BindingContext = bindingContext;
                    }
                    _screensByName[screen.Name] = screen;
                }
                else
                {
                    throw new ApplicationException("Unknown screen: " + screenName);
                }
            }

            var returnMe = _screensByName[screenName];
            returnMe.Prepare(_styleLibrary);
            return returnMe;
        }

        //-----------------------------------------------------------------------------------------------
        /// <summary>
        /// Find any widget by name
        /// </summary>
        //-----------------------------------------------------------------------------------------------
        internal VarmintWidget FindWidgetByName(string widgetName)
        {
            // Hydrate all the screens on startup and build a widget library
            // Creating/Disposing widgets should update a global widget list
            // GetScreen should not set bindings or call prepare with the style library
        }

        //-----------------------------------------------------------------------------------------------
        /// <summary>
        /// AddContent - Add or replace a vwml item in the widget space.  Use this method to add
        ///              content you might want to change dynamically
        /// </summary>
        //-----------------------------------------------------------------------------------------------
        public void AddContent(string name, Stream vwmlStream, object bindingContext)
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
                _screenLibrary.Add(layout.Name, layout);
            }
        }

    }
}
