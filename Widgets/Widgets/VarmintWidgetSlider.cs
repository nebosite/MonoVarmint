using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MonoVarmint.Widgets
{
    //--------------------------------------------------------------------------------------
    /// <summary>
    /// VarmintWidgetPanel - Generic widget intended for custom drawing
    /// </summary>
    //--------------------------------------------------------------------------------------
    [VarmintWidgetShortName("Slider")]
    public class VarmintWidgetPanelSlider : VarmintWidget
    {
        public VarmintWidget Track { get; set; }
        public VarmintWidget Thumb { get; set; }

        public override IEnumerable<VarmintWidget> Children
        {
            get
            {
                yield return Track;
                foreach(var child in base.Children)
                {
                    yield return child;
                }
                yield return Thumb;
            }
        }   

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// ctor
        /// </summary>
        //--------------------------------------------------------------------------------------
        public VarmintWidgetPanelSlider()
        {
        }
    }
}