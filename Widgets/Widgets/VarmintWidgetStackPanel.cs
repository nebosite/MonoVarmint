using Microsoft.Xna.Framework;
using System.Collections.Generic;

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
        protected override void UpdateFormatting_Internal(Vector2 maxExtent, bool updateChildren)
        {
            //if (Orientation == Orientation.Horizontal) UpdateHorizontal(maxExtent);
            //else
            UpdateVertical(maxExtent);

            // base.UpdateFormatting_Internal(maxExtent);

        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// UpdateVertical
        /// </summary>
        //--------------------------------------------------------------------------------------
        private void UpdateVertical(Vector2 maxExtent)
        {
            // Go through all the children and figure out known sizes.  Stretched sizes will be double.MaxVlue
            float childHeight = 0;
            float childWidth = 0;
            int stretchedChildren = 0;
            foreach(var child in Children)
            {
                child.UpdateFormatting(maxExtent);
                if (child.Size.X > childWidth)
                {
                    childWidth = child.Size.X;
                }

                if(child.Size.Y == maxExtent.Y)
                {
                    stretchedChildren++;
                }
                else
                {
                    childHeight += child.Size.Y + (child.Margin?.Top ?? 0) + (child.Margin?.Bottom ?? 0);
                }
            }

            var remainingHeight = maxExtent.Y - childHeight;
            if (remainingHeight < 0) remainingHeight = 0;

            // reformat all the children now that we know the available sizes
            float finalHeight = 0;
            foreach (var child in Children)
            {
                if(child.Size.Y == maxExtent.Y)
                {
                    child.UpdateFormatting(new Vector2(childWidth, remainingHeight / stretchedChildren));
                }
                else
                {
                    child.UpdateFormatting(new Vector2(childWidth, child.Size.Y + (child.Margin?.Top ?? 0) + (child.Margin?.Bottom ?? 0)));
                }

                finalHeight += child.Size.Y + (child.Margin?.Top ?? 0) + (child.Margin?.Bottom ?? 0);
            }


            this.Size = new Vector2(childWidth, finalHeight);
            base.UpdateFormatting_Internal(maxExtent, updateChildren: false);

            float verticalOffset = 0;
            foreach (var child in Children)
            {
                child.Offset = new Vector2(child.Offset.X, verticalOffset + (child.Margin?.Top ?? 0));
                verticalOffset += child.Size.Y + (child.Margin?.Top ?? 0) + (child.Margin?.Bottom ?? 0);
            }




           
        }


        //--------------------------------------------------------------------------------------
        /// <summary>
        /// UpdateHorizontal
        /// </summary>
        //--------------------------------------------------------------------------------------
        //private void UpdateHorizontal(Vector2 updatedSize)
        //{
    }
}