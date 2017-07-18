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
            Renderer.DrawBox(Offset, Size, BackgroundColor);
        }

        bool isUpdating = false;
        //--------------------------------------------------------------------------------------
        /// <summary>
        /// UpdateChildFormatting
        /// </summary>
        //--------------------------------------------------------------------------------------
        protected override void UpdateChildFormatting_Internal(Vector2? updatedSize)
        {
            if (Orientation == Orientation.Horizontal) UpdateHorizontal(updatedSize);
            else UpdateVertical(updatedSize);
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// UpdateVertical
        /// </summary>
        //--------------------------------------------------------------------------------------
        private void UpdateVertical(Vector2? updatedSize)
        {
            Vector2 newStackSize = IntendedSize;
            if(updatedSize != null)
            {
                newStackSize = updatedSize.Value;
            }
            var intendedSpace = newStackSize.Y;

            if(!HasChildren)
            {
                this.Size = newStackSize;
                return;
            }
            newStackSize.Y = 0;
            var maxWidth = 0f;
            var stretchBudget = 0f;
            float spaceFromStretchables = 0;
            foreach (var child in Children)
            {
                var childSize = child.IntendedSize;
                childSize.X += (child.Margin.Left ?? 0) + (child.Margin.Right ?? 0);
                childSize.Y += (child.Margin.Top ?? 0) + (child.Margin.Bottom ?? 0);
                stretchBudget += child.Stretch.Vertical ?? 0;

                newStackSize.Y += childSize.Y;
                if (childSize.X > maxWidth) maxWidth = childSize.X;
            }

            if (newStackSize.X == 0) newStackSize.X = maxWidth;
            Size = newStackSize;
            float nextY = 0;

            foreach (var child in Children)
            {
                var newSize = child.IntendedSize;
                if (child.Stretch.Horizontal != null)
                {
                    newSize.X = Size.X - ((child.Margin.Left ?? 0) + (child.Margin.Right ?? 0));
                }
                if (child.Stretch.Vertical != null)
                {
                    spaceFromStretchables += newSize.Y;
                }
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

            // If there is stretching to do, we need another pass
            var remainingSpace = intendedSpace - nextY;
            if(stretchBudget > 0 && remainingSpace > 0)
            {
                remainingSpace += spaceFromStretchables;
                nextY = 0;
                foreach (var child in Children)
                {
                    var newSize = child.Size;
                    nextY += child.Margin.Top ?? 0;
                    child.Offset = new Vector2(child.Offset.X, nextY);
                    if(child.Stretch.Vertical != null)
                    {
                        float newHeight = (child.Stretch.Vertical.Value / stretchBudget) * remainingSpace;
                        newSize = new Vector2(child.Size.X, newHeight);
                    }
                    nextY += newSize.Y + (child.Margin.Bottom ?? 0);

                    child.Size = newSize;
                }
                newStackSize.Y = nextY;
                Size = newStackSize;
            }
        }


        //--------------------------------------------------------------------------------------
        /// <summary>
        /// UpdateHorizontal
        /// </summary>
        //--------------------------------------------------------------------------------------
        private void UpdateHorizontal(Vector2? updatedSize)
        {
            Vector2 newStackSize = IntendedSize;
            if (updatedSize != null)
            {
                newStackSize = updatedSize.Value;
            }
            var intendedSpace = newStackSize.X;

            newStackSize.X = 0;
            var maxHeight = 0f;
            var stretchBudget = 0f;
            float spaceFromStretchables = 0;
            foreach (var child in Children)
            {
                var childSize = child.IntendedSize;
                childSize.X += (child.Margin.Left ?? 0) + (child.Margin.Right ?? 0);
                childSize.Y += (child.Margin.Top ?? 0) + (child.Margin.Bottom ?? 0);
                stretchBudget += child.Stretch.Horizontal ?? 0;

                newStackSize.X += childSize.X;
                if (childSize.Y > maxHeight) maxHeight = childSize.Y;
            }

            if (newStackSize.Y == 0) newStackSize.Y = maxHeight;
            Size = newStackSize;
            float nextX = 0;

            foreach (var child in Children)
            {
                var newSize = child.IntendedSize;
                if (child.Stretch.Horizontal != null)
                {
                    spaceFromStretchables += newSize.X;
                }
                if (child.Stretch.Vertical != null)
                {
                     newSize.Y = Size.Y - ((child.Margin.Top ?? 0) + (child.Margin.Bottom ?? 0));
                }
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

            // If there is stretching to do, we need another pass
            var remainingSpace = intendedSpace - nextX;
            if (stretchBudget > 0 && remainingSpace > 0)
            {
                remainingSpace += spaceFromStretchables;
                nextX = 0;
                foreach (var child in Children)
                {
                    var newSize = child.Size;
                    nextX += child.Margin.Left ?? 0;
                    child.Offset = new Vector2(nextX, child.Offset.Y);
                    if (child.Stretch.Horizontal != null)
                    {
                        float newWidth = (child.Stretch.Horizontal.Value / stretchBudget) * remainingSpace;
                        newSize = new Vector2(newWidth, child.Size.Y);
                    }
                    nextX += newSize.X + (child.Margin.Right ?? 0);

                    child.Size = newSize;
                }
                newStackSize.X = nextX;
                Size = newStackSize;
            }
        }
    }
}