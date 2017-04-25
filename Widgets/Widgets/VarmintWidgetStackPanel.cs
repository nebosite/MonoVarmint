using Microsoft.Xna.Framework;

namespace MonoVarmint.Widgets
{
    public enum Orientation
    {
        Vertical,
        Horizontal
    }

    //--------------------------------------------------------------------------------------
    /// <summary>
    /// StackPanel
    /// </summary>
    //--------------------------------------------------------------------------------------
    [VarmintWidgetShortName("StackPanel")]
    public class VarmintWidgetStackPanel : VarmintWidget
    {
        public Orientation Orientation { get; set; }

        private WidgetMargin _myMargin;
        public override WidgetMargin Margin
        {
            get { return _myMargin; }
            set
            {
                // Stack panels always stretch to fill their parent area
                _myMargin = value;
                if (_myMargin.Left == null) _myMargin.Left = 0;
                if (_myMargin.Top == null) _myMargin.Top = 0;
                if (_myMargin.Right == null) _myMargin.Right = 0;
                if (_myMargin.Bottom == null) _myMargin.Bottom = 0;
            }
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// ctor
        /// </summary>
        //--------------------------------------------------------------------------------------
        public VarmintWidgetStackPanel()
        {
            ChildrenAffectFormatting = true;
            this.OnRender += Render;
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Render
        /// </summary>
        //--------------------------------------------------------------------------------------
        void Render(GameTime gameTime, VarmintWidget widget)
        {
            Renderer.DrawBox(AbsoluteOffset, Size, BackgroundColor);
        }

        bool isUpdating = false;
        //--------------------------------------------------------------------------------------
        /// <summary>
        /// UpdateChildFormatting
        /// </summary>
        //--------------------------------------------------------------------------------------
        public override void UpdateChildFormatting(bool recurse = false)
        {
            if (isUpdating) return;
            isUpdating = true;
            if (recurse)
            {
                foreach (var child in Children)
                {
                    child.UpdateChildFormatting(true);
                }
            }
            if (Orientation == Orientation.Horizontal) UpdateHorizontal();
            else UpdateVertical();
            isUpdating = false;
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// UpdateVertical
        /// </summary>
        //--------------------------------------------------------------------------------------
        private void UpdateVertical()
        {
            Vector2 newStackSize = IntendedSize;
            if(Children.Count == 0)
            {
                this.Size = newStackSize;
                return;
            }
            newStackSize.Y = 0;
            var maxWidth = 0f;
            foreach (var child in Children)
            {
                var childSize = child.IntendedSize;
                childSize.X += (child.Margin.Left ?? 0) + (child.Margin.Right ?? 0);
                childSize.Y += (child.Margin.Top ?? 0) + (child.Margin.Bottom ?? 0);

                newStackSize.Y += childSize.Y;
                if (childSize.X > maxWidth) maxWidth = childSize.X;
            }

            if (newStackSize.X == 0) newStackSize.X = maxWidth;
            Size = newStackSize;
            float nextY = 0;

            foreach (var child in Children)
            {
                var newSize = child.IntendedSize;
                nextY += child.Margin.Top ?? 0;
                var newOffset = new Vector2(0, nextY);
                nextY += newSize.Y + (child.Margin.Bottom ?? 0);
                var availableWidth = Size.X - newSize.X - (child.Margin.Right ?? 0) - (child.Margin.Left ?? 0);

                switch (HorizontalContentAlignment)
                {
                    case HorizontalContentAlignment.Left:
                        newOffset.X = (child.Margin.Left ?? 0);
                        if (child.Margin.Right != null) newSize.X += availableWidth;
                        break;
                    case HorizontalContentAlignment.Center:
                        var width = newSize.X + (child.Margin.Left ?? 0) + (child.Margin.Right ?? 0);
                        newOffset.X = (Size.X - width) / 2 + (child.Margin.Left ?? 0);
                        break;
                    case HorizontalContentAlignment.Right:
                        if (child.Margin.Left != null) newSize.X += availableWidth;
                        else
                        {
                            newOffset.X = availableWidth;
                        }
                        break;
                }

                child.Offset = newOffset;
                child.Size = newSize;
            }
        }


        //--------------------------------------------------------------------------------------
        /// <summary>
        /// UpdateHorizontal
        /// </summary>
        //--------------------------------------------------------------------------------------
        private void UpdateHorizontal()
        {
            Vector2 newStackSize = IntendedSize;
            if (Children.Count == 0)
            {
                this.Size = newStackSize;
                return;
            }
            newStackSize.X = 0;
            var maxHeight = 0f;
            foreach (var child in Children)
            {
                var childSize = child.IntendedSize;
                childSize.X += (child.Margin.Left ?? 0) + (child.Margin.Right ?? 0);
                childSize.Y += (child.Margin.Top ?? 0) + (child.Margin.Bottom ?? 0);

                newStackSize.X += childSize.X;
                if (childSize.Y > maxHeight) maxHeight = childSize.Y;
            }

            if (newStackSize.Y == 0) newStackSize.Y = maxHeight;
            Size = newStackSize;
            float nextX = 0;

            foreach (var child in Children)
            {
                var newSize = child.IntendedSize;
                nextX += (child.Margin.Left ?? 0);
                var newOffset = new Vector2(nextX, 0);
                nextX += newSize.X + (child.Margin.Right ?? 0);
                var availableHeight = Size.Y - newSize.Y - (child.Margin.Top ?? 0) - (child.Margin.Bottom ?? 0);

                switch (VerticalContentAlignment)
                {
                    case VerticalContentAlignment.Top:
                        newOffset.Y = (child.Margin.Top ?? 0);
                        if (child.Margin.Bottom != null) newSize.Y += availableHeight;
                        break;
                    case VerticalContentAlignment.Center:
                        var height = newSize.Y + (child.Margin.Top ?? 0) + (child.Margin.Bottom ?? 0);
                        newOffset.Y = (Size.Y - height) / 2 + (child.Margin.Top ?? 0);
                        break;
                    case VerticalContentAlignment.Bottom:
                        if (child.Margin.Top != null) newSize.Y += availableHeight;
                        else
                        {
                            newOffset.Y = availableHeight;
                        }
                        break;
                }

                child.Offset = newOffset;
                child.Size = newSize;
            }
        }
    }
}