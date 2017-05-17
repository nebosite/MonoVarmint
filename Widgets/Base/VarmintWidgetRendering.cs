using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace MonoVarmint.Widgets
{
    public partial class VarmintWidget
    {
        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Render
        /// </summary>
        //--------------------------------------------------------------------------------------
        public virtual void RenderMe(GameTime gameTime, Dictionary<string, VarmintWidgetStyle> styleLibrary = null)
        {
            Update(styleLibrary);
            if (!IsVisible) return;
            var localAnimations = _animations.ToArray();
            foreach (var animation in localAnimations) animation.Update(this, gameTime);
            _animations.RemoveAll(a => a.IsComplete);
            OnRender?.Invoke(gameTime, this);

            RenderChildren(gameTime, styleLibrary);
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Render
        /// </summary>
        //--------------------------------------------------------------------------------------
        public virtual void RenderChildren(GameTime gameTime, Dictionary<string, VarmintWidgetStyle> styleLibrary = null)
        {
            if (children.Count > 0)
            {
                // Make a local copy because children can modify parent/child relationships
                var localChildren = new List<VarmintWidget>(children);
                foreach (var child in localChildren)
                {
                    child.RenderMe(gameTime, styleLibrary);
                }
            }
        }


        //--------------------------------------------------------------------------------------
        /// <summary>
        /// UpdateChildFormatting - the size has changed, so update the offsets and sizes of
        ///                         the children of this widget
        /// </summary>
        //--------------------------------------------------------------------------------------
        public virtual void UpdateChildFormatting()
        {
            UpdateChildFormatting(false);
        }
        public virtual void UpdateChildFormatting(bool recurse)
        {
            if (_updating) return;
            if (recurse)
            {
                foreach (var child in children)
                {
                    child.UpdateChildFormatting(true);
                }
            }

            foreach (var child in children)
            {
                var newSize = child.IntendedSize;
                var newOffset = Vector2.Zero;
                var availableSize = Size - newSize;
                availableSize.X -= ((child.Margin.Left ?? 0) + (child.Margin.Right ?? 0));
                availableSize.Y -= ((child.Margin.Top ?? 0) + (child.Margin.Bottom ?? 0));

                switch (HorizontalContentAlignment)
                {
                    case HorizontalContentAlignment.Left:
                        if (child.Margin.Left != null)
                        {
                            newOffset.X = child.Margin.Left.Value;
                            if (child.Margin.Right != null) newSize.X += availableSize.X;
                        }
                        else
                        {
                            newOffset.X = child.Margin.Right == null ? 0 : availableSize.X;
                        }

                        break;
                    case HorizontalContentAlignment.Center:
                        var width = newSize.X + (child.Margin.Left ?? 0) + (child.Margin.Right ?? 0);
                        newOffset.X = (Size.X - width) / 2 + (child.Margin.Left ?? 0);
                        break;
                    case HorizontalContentAlignment.Right:
                        if (child.Margin.Left != null) newSize.X += availableSize.X;
                        else
                        {
                            newOffset.X = availableSize.X;
                        }
                        break;
                }

                switch (VerticalContentAlignment)
                {
                    case VerticalContentAlignment.Top:
                        if (child.Margin.Top != null)
                        {
                            newOffset.Y = child.Margin.Top.Value;
                            if (child.Margin.Bottom != null) newSize.Y += availableSize.Y;
                        }
                        else
                        {
                            newOffset.Y = child.Margin.Bottom == null? 0 : availableSize.Y;
                        }
                        break;
                    case VerticalContentAlignment.Center:
                        var height = newSize.Y + (child.Margin.Top ?? 0) + (child.Margin.Bottom ?? 0);
                        newOffset.Y = (Size.Y - height) / 2 + (child.Margin.Top ?? 0);
                        break;
                    case VerticalContentAlignment.Bottom:
                        if (child.Margin.Top != null) newSize.Y += availableSize.Y;
                        else
                        {
                            newOffset.Y = availableSize.Y;
                        }
                        break;
                }

                child.Offset = newOffset;
                child.Size = newSize;

            }
        }

    }
}