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
        private VarmintWidget track;
        public VarmintWidget Track {
            get
            {
                return track;
            }
            set
            {
                track = value;
                if (track != null) track.Parent = this;
            }
        }
        private VarmintWidget thumb;
        public VarmintWidget Thumb {
            get
            {
                return thumb;
            }
            set
            {
                thumb = value;
                if (thumb != null) thumb.Parent = this;
            }
        }

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