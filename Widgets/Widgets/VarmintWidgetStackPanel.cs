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
            Renderer.DrawBox(AbsoluteOffset, Size, RenderBackgroundColor);
        }

        bool isUpdating = false;
        //--------------------------------------------------------------------------------------
        /// <summary>
        /// UpdateChildFormatting
        /// </summary>
        //--------------------------------------------------------------------------------------
        protected override void UpdateFormatting_Internal(Vector2 maxExtent)
        {
            //var maxSize = GetMaxDimentsions(maxExtent, out var width, out var height);
            //if (Orientation == Orientation.Horizontal) UpdateHorizontal(maxSize);
            //else UpdateVertical(maxSize);

            base.UpdateFormatting_Internal(maxExtent);

        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// UpdateVertical
        /// </summary>
        //--------------------------------------------------------------------------------------
        //private void UpdateVertical(Vector2 updatedSize)
        //{
        //    Vector2 newStackSize = updatedSize;

        //    var intendedSpace = newStackSize.Y;

        //    if(!HasChildren)
        //    {
        //        this.Size = newStackSize;
        //        return;
        //    }
        //    newStackSize.Y = 0;
        //    var maxWidth = 0f;
        //    var stretchBudget = 0f;
        //    float spaceFromStretchables = 0;
        //    foreach (var child in Children)
        //    {
        //        child.UpdateFormatting(newStackSize);

        //        newStackSize.Y += childSize.Y;
        //        if (childSize.X > maxWidth) maxWidth = childSize.X;
        //    }

        //    if (newStackSize.X == 0) newStackSize.X = maxWidth;
        //    Size = newStackSize;
        //    float nextY = 0;

        //    foreach (var child in Children)
        //    {
        //        var newSize = child.IntendedSize;
        //        if (child.WidgetAlignment.X == Alignment.Stretch)
        //        {
        //            newSize.X = Size.X - ((child.Margin.Left ?? 0) + (child.Margin.Right ?? 0));
        //        }
        //        if (child.WidgetAlignment.Y == Alignment.Stretch)
        //        {
        //            spaceFromStretchables += newSize.Y;
        //        }
        //        nextY += child.Margin.Top ?? 0;
        //        var newOffset = new Vector2(0, nextY);
        //        nextY += newSize.Y + (child.Margin.Bottom ?? 0);
        //        var availableWidth = Size.X - newSize.X - (child.Margin.Right ?? 0) - (child.Margin.Left ?? 0);

        //        switch (ContentAlignment?.X)
        //        {
        //            case Alignment.Left:
        //                newOffset.X = (child.Margin.Left ?? 0);
        //                if (child.Margin.Right != null) newSize.X += availableWidth;
        //                break;
        //            case Alignment.Center:
        //                var width = newSize.X + (child.Margin.Left ?? 0) + (child.Margin.Right ?? 0);
        //                newOffset.X = (Size.X - width) / 2 + (child.Margin.Left ?? 0);
        //                break;
        //            case Alignment.Right:
        //                if (child.Margin.Left != null) newSize.X += availableWidth;
        //                else
        //                {
        //                    newOffset.X = availableWidth;
        //                }
        //                break;
        //        }

        //        child.Offset = newOffset;
        //        child.Size = newSize;
        //    }

        //    // If there is stretching to do, we need another pass
        //    var remainingSpace = intendedSpace - nextY;
        //    if(stretchBudget > 0 && remainingSpace > 0)
        //    {
        //        remainingSpace += spaceFromStretchables;
        //        nextY = 0;
        //        foreach (var child in Children)
        //        {
        //            var newSize = child.Size;
        //            nextY += child.Margin.Top ?? 0;
        //            child.Offset = new Vector2(child.Offset.X, nextY);
        //            if(child.WidgetAlignment.Y == Alignment.Stretch)
        //            {
        //                float newHeight = (1 / stretchBudget) * remainingSpace;
        //                newSize = new Vector2(child.Size.X, newHeight);
        //            }
        //            nextY += newSize.Y + (child.Margin.Bottom ?? 0);

        //            child.Size = newSize;
        //        }
        //        newStackSize.Y = nextY;
        //        Size = newStackSize;
        //    }
        //}


        //--------------------------------------------------------------------------------------
        /// <summary>
        /// UpdateHorizontal
        /// </summary>
        //--------------------------------------------------------------------------------------
        //private void UpdateHorizontal(Vector2 updatedSize)
        //{
        //    Vector2 newStackSize = updatedSize;
        //    var intendedSpace = newStackSize.X;

        //    newStackSize.X = 0;
        //    var maxHeight = 0f;
        //    var stretchBudget = 0f;
        //    float spaceFromStretchables = 0;
        //    foreach (var child in Children)
        //    {
        //        var childSize = child.IntendedSize;
        //        childSize.X += (child.Margin.Left ?? 0) + (child.Margin.Right ?? 0);
        //        childSize.Y += (child.Margin.Top ?? 0) + (child.Margin.Bottom ?? 0);
        //        //stretchBudget += child.Stretch.Horizontal ?? 0;

        //        newStackSize.X += childSize.X;
        //        if (childSize.Y > maxHeight) maxHeight = childSize.Y;
        //    }

        //    if (newStackSize.Y == 0) newStackSize.Y = maxHeight;
        //    Size = newStackSize;
        //    float nextX = 0;

        //    foreach (var child in Children)
        //    {
        //        var newSize = child.IntendedSize;
        //        if (child.WidgetAlignment.X == Alignment.Stretch)
        //        {
        //            spaceFromStretchables += newSize.X;
        //        }
        //        if (child.WidgetAlignment.Y == Alignment.Stretch)
        //        {
        //             newSize.Y = Size.Y - ((child.Margin.Top ?? 0) + (child.Margin.Bottom ?? 0));
        //        }
        //        nextX += (child.Margin.Left ?? 0);
        //        var newOffset = new Vector2(nextX, 0);
        //        nextX += newSize.X + (child.Margin.Right ?? 0);
        //        var availableHeight = Size.Y - newSize.Y - (child.Margin.Top ?? 0) - (child.Margin.Bottom ?? 0);

        //        switch (ContentAlignment?.Y)
        //        {
        //            case Alignment.Top:
        //                newOffset.Y = (child.Margin.Top ?? 0);
        //                if (child.Margin.Bottom != null) newSize.Y += availableHeight;
        //                break;
        //            case Alignment.Center:
        //                var height = newSize.Y + (child.Margin.Top ?? 0) + (child.Margin.Bottom ?? 0);
        //                newOffset.Y = (Size.Y - height) / 2 + (child.Margin.Top ?? 0);
        //                break;
        //            case Alignment.Bottom:
        //                if (child.Margin.Top != null) newSize.Y += availableHeight;
        //                else
        //                {
        //                    newOffset.Y = availableHeight;
        //                }
        //                break;
        //        }

        //        child.Offset = newOffset;
        //        child.Size = newSize;
        //    }

        //    // If there is stretching to do, we need another pass
        //    var remainingSpace = intendedSpace - nextX;
        //    if (stretchBudget > 0 && remainingSpace > 0)
        //    {
        //        remainingSpace += spaceFromStretchables;
        //        nextX = 0;
        //        foreach (var child in Children)
        //        {
        //            var newSize = child.Size;
        //            nextX += child.Margin.Left ?? 0;
        //            child.Offset = new Vector2(nextX, child.Offset.Y);
        //            if (child.WidgetAlignment.X == Alignment.Stretch)
        //            {
        //                float newWidth = (1 / stretchBudget) * remainingSpace;
        //                newSize = new Vector2(newWidth, child.Size.Y);
        //            }
        //            nextX += newSize.X + (child.Margin.Right ?? 0);

        //            child.Size = newSize;
        //        }
        //        newStackSize.X = nextX;
        //        Size = newStackSize;
        //    }
        //}
    }
}