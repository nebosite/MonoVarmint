using Microsoft.Xna.Framework;
using MonoVarmint.Widgets;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MonoVarmint.Tools.Tests
{
    /// <summary>
    /// Vanilla widget to test just VarmintWidget code
    /// </summary>
    [VarmintWidgetShortName("TW")]
    class TestWidget : VarmintWidget { }

    /// <summary>
    /// Vanilla widget to test just VarmintWidget code
    /// </summary>
    [VarmintWidgetShortName("TW2")]
    class TestWidget2 : VarmintWidget { }


    class BindingThing
    {
        public int FooCalls = 0; 
        public Vector2 SizeProperty { get { return new Vector2(2, 3); } }
        public VarmintWidget.EventHandledState HandleTap(VarmintWidget widget, Vector2 location)
        {
            FooCalls++;
            return VarmintWidget.EventHandledState.Handled;
        }
    }

    class TestUtils
    {
        public static VarmintWidget LoadFromText(IVarmintWidgetInjector injector, string vwml, string defaultName)
        {
            using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(vwml)))
            {
                var layout = VarmintWidget.PreloadFromVwml(memoryStream, defaultName);
                return VarmintWidget.HydrateLayout(injector, layout, new Dictionary<string, VarmintWidget.LayoutItem>());
            }
        }
    }
}
