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
        /// <summary>
        /// Track Widget
        /// </summary>
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

        /// <summary>
        /// Thumb Widget
        /// </summary>
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

        /// <summary>
        /// Value
        /// </summary>
        public float _value = 0;
        public float Value
        {
            get { return _value; }
            set {
                _value = value;
                if (_value < 0) _value = 0;
                if (_value > 1) _value = 1;
                FixThumbOffset();
                PushValueToBinding(nameof(Value));
            }
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// FixThumbOffset - Depends on the slider slide and value
        /// </summary>
        //--------------------------------------------------------------------------------------
        private void FixThumbOffset()
        {
            var slideWidth = Size.X - Thumb.Size.X;
            
            Thumb.Offset = new Vector2(slideWidth * Value, Thumb.Offset.Y);
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Thumb_OnDrag - Change the Value based on the drag
        /// </summary>
        //--------------------------------------------------------------------------------------
        private EventHandledState Thumb_OnDrag(VarmintWidget sender, Vector2 start, Vector2 end)
        {
            var slideWidth = Size.X - Thumb.Size.X;
            var xMove = (start.X - end.X)/slideWidth;
            Value += xMove;

            return EventHandledState.Handled;
        }

        /// <summary>
        /// Children should include the Thumb and Track
        /// </summary>
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
            this.OnRender += Render;
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Render
        /// </summary>
        //--------------------------------------------------------------------------------------
        void Render(GameTime gameTime, VarmintWidget widget)
        {
            Renderer.DrawBox(AbsoluteOffset, Size, RenderBackgroundColor);
        }
    }
}