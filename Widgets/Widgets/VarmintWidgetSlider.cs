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
                if (thumb != null)
                {
                    thumb.OnDrag -= Thumb_OnDrag;
                }
                thumb = value;
                if (thumb != null)
                {
                    thumb.Parent = this;
                    thumb.OnDrag += Thumb_OnDrag;
                }
            }
        }

        private EventHandledState Thumb_OnDrag(VarmintWidget sender, Vector2 start, Vector2 end)
        {
            // TODO: Implement to allow dragging to work.
            // This should probably be implemented by changing the underlying slider value
            // according to the drag position. Data binding will then update the slider
            // thumb position accordingly, creating the effect of a drag. This will also cause
            // the slider to jump between valid positions in situations where only a few
            // integer values are accepted.
            throw new System.NotImplementedException();
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