using Microsoft.Xna.Framework;

namespace MonoVarmint.Widgets
{
    //--------------------------------------------------------------------------------------
    /// <summary>
    /// VarmintWidgetScrollView 
    /// 
    /// You place the objects where you want using their offsets.  The view will 
    /// automatically set the bounds based on the objects you add and their margins
    /// 
    /// </summary>
    //--------------------------------------------------------------------------------------
    [VarmintWidgetShortName("ScrollView")]
    public class VarmintWidgetScrollView : VarmintWidget
    {
        /// <summary>
        /// Upper left corner of the scroll view in virtual space
        /// </summary>
        public Vector2 ScrollOffset { get { return _innerContent.Offset; } set { _innerContent.Offset = value; } }

        /// <summary>
        /// AbsoluteOffset
        /// </summary>
        public override Vector2 AbsoluteOffset
        {
            get
            {
                if (Parent == null) return Offset;
                else return Parent.AbsoluteOffset + Offset;
            }
        }

        PlainFormatter _innerContent;
        
        //--------------------------------------------------------------------------------------
        /// <summary>
        /// ctor
        /// </summary>
        //--------------------------------------------------------------------------------------
        public VarmintWidgetScrollView()
        {
            ClipToBounds = true;
            this.SetCustomRender((gt, w) => {
                ScrollBy(_momentum);
                _momentum *= .9f;
                Renderer.DrawBox(AbsoluteOffset, Size, RenderBackgroundColor);
            });
            this.OnDrag += VarmintWidgetScrollView_OnDrag;
            this.OnFlick += VarmintWidgetScrollView_OnFlick;

            _innerContent = new PlainFormatter();
            _innerContent.Renderer = new NullRenderer();
            _innerContent.Name = "InnerFrame_" + this.Name;
            base.AddChild(_innerContent);
        }

        Vector2 _momentum;
        //--------------------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------------------
        private EventHandledState VarmintWidgetScrollView_OnFlick(VarmintFlickData flick)
        {
            _momentum = flick.Delta * .2f;
            return EventHandledState.Handled;
        }

        //--------------------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------------------
        protected override void UpdateFormatting_Internal(Vector2 updatedSize, bool updateChildren = true)
        {
            // child formatting is static - the scrollview does not update it
            _innerContent.RecalculateExtremes();
        }

        //--------------------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------------------
        public override void AddChild(VarmintWidget widget, bool suppressChildUpdate = false)
        {
            _innerContent.AddChild(widget, suppressChildUpdate);
        }

        //--------------------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------------------
        public override void RemoveChild(VarmintWidget childToRemove, bool suppressChildUpdate = false)
        {
            _innerContent.RemoveChild(childToRemove, suppressChildUpdate);
        }

        //--------------------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------------------
        public override void ClearChildren()
        {
            _innerContent.ClearChildren();
        }

        //--------------------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------------------
        private void KeepInBounds(Vector2 previousDelta)
        {
            var correction = Vector2.Zero;
            if(previousDelta.X != 0)
            {
                if (_innerContent.ExtremeRight + ScrollOffset.X < Size.X)
                {
                    correction.X = Size.X - (_innerContent.ExtremeRight + ScrollOffset.X);
                }

                else if (_innerContent.ExtremeLeft + ScrollOffset.X > 0)
                {
                    correction.X = -(_innerContent.ExtremeLeft + ScrollOffset.X);
                }

            }

            if (previousDelta.Y != 0)
            {
                if (_innerContent.ExtremeTop + ScrollOffset.Y > 0)
                {
                    correction.Y = -(_innerContent.ExtremeTop + ScrollOffset.Y);
                }

                else if (_innerContent.ExtremeBottom + ScrollOffset.Y < Size.Y)
                {
                    correction.Y = Size.Y - (_innerContent.ExtremeBottom + ScrollOffset.Y);
                }
            }

            ScrollOffset += correction;
        }

        //--------------------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------------------
        private EventHandledState VarmintWidgetScrollView_OnDrag(VarmintWidget source, Vector2 location, Vector2 delta)
        {
            ScrollBy(delta);
            return EventHandledState.Handled;
        }

        //--------------------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------------------
        void ScrollBy(Vector2 delta)
        {
            if (delta.Length() == 0) return;

            if (_innerContent.ExtremeLeft + ScrollOffset.X >= 0
                && _innerContent.ExtremeRight + ScrollOffset.X <= Size.X)
            {
                // Don't try to scroll if we are inside the display area
                delta.X = 0;
            }

            if (_innerContent.ExtremeTop + ScrollOffset.Y >= 0
                && _innerContent.ExtremeBottom + ScrollOffset.Y <= Size.Y)
            {
                // Don't try to scroll if we are inside the display area
                delta.Y = 0;
            }
            ScrollOffset += delta;
            KeepInBounds(delta);

        }

        //--------------------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------------------
        class PlainFormatter : VarmintWidget
        {
            public float ExtremeLeft { get; set; }
            public float ExtremeTop { get; set; }
            public float ExtremeRight { get; set; }
            public float ExtremeBottom { get; set; }
            public PlainFormatter()
            {
                this.SetCustomRender((gt, w) => { });
            }

            internal void RecalculateExtremes()
            {
                ExtremeLeft = float.MaxValue;
                ExtremeTop = float.MaxValue;
                ExtremeRight = float.MinValue;
                ExtremeBottom = float.MinValue;

                foreach(var child in Children)
                { 
                    var left = child.Offset.X - (child.Margin.Left ?? 0);
                    var top = child.Offset.Y - (child.Margin.Top ?? 0);
                    var right = child.Offset.X + child.Size.X + (child.Margin.Right ?? 0);
                    var bottom = child.Offset.Y + child.Size.Y +( child.Margin.Bottom ?? 0);

                    if (left < ExtremeLeft) ExtremeLeft = left;
                    if (top < ExtremeTop) ExtremeTop = top;
                    if (right > ExtremeRight)  ExtremeRight = right;
                    if (bottom > ExtremeBottom) ExtremeBottom = bottom;
                }

                Size = new Vector2(ExtremeRight - ExtremeLeft, ExtremeBottom - ExtremeTop);
            }

            public override void AddChild(VarmintWidget widget, bool suppressChildUpdate = false)
            {
                base.AddChild(widget, suppressChildUpdate);
                RecalculateExtremes();
            }

            public override void RemoveChild(VarmintWidget childToRemove, bool suppressChildUpdate = false)
            {
                base.RemoveChild(childToRemove, suppressChildUpdate);
                RecalculateExtremes();
            }

            protected override void UpdateFormatting_Internal(Vector2 updatedSize, bool updateChildren = true) { }
        }
    }
}