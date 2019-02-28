using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace MonoVarmint.Widgets
{
    public partial class VarmintWidget
    {
        public static bool VisualDebuggingEnabled { get; set; }
        private bool _applyingStyles;

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// AdvanceAnimations
        /// </summary>
        //--------------------------------------------------------------------------------------
        public void AdvanceAnimations(GameTime gameTime)
        {
            var localAnimations = _animations.ToArray();
            foreach (var animation in localAnimations) animation.Update(this, gameTime);
            _animations.RemoveAll(a => a.IsComplete);

            foreach (var child in ChildrenCopy) child.AdvanceAnimations(gameTime);
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Render
        /// </summary>
        //--------------------------------------------------------------------------------------
        public virtual void RenderMe(GameTime gameTime)
        {
            if(Renderer == null)
            {
                throw new InvalidOperationException("The Renderer property is null on " + GetType().Name + " with Name=" + Name);
            }

            Update();
            if (!IsVisible) return;

            var shouldClip = ClipToBounds
                || Rotate != 0 || FlipHorizontal || FlipVertical;
            if (shouldClip) Renderer.BeginClipping(AbsoluteOffset, Size);
            OnRender?.Invoke(gameTime, this);

            RenderChildren(gameTime);

            if(VisualDebuggingEnabled)
            {
                Renderer.DrawRectangle(Offset, Size, .003f, Color.Red);  
            }

            if (!shouldClip) return;
            Renderer.EndClipping(
                (float)(Rotate / 180.0 * Math.PI), 
                new Vector2(.5f),
                new Vector2(1),
                FlipHorizontal,
                FlipVertical);
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Render
        /// </summary>
        //--------------------------------------------------------------------------------------
        public virtual void RenderChildren(GameTime gameTime)
        {
            if (!HasChildren) return;
            // Make a local copy because children can modify parent/child relationships
            var localChildren = new List<VarmintWidget>(Children);
            foreach (var child in localChildren)
            {
                if (Renderer.IsInRenderingWindow(child.AbsoluteOffset, child.Size))
                {
                    child.RenderMe(gameTime);
                }
            }
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// ApplyStyles 
        /// </summary>
        //--------------------------------------------------------------------------------------
        private void ApplyStyles(Dictionary<string, VarmintWidgetStyle> styleLibrary)
        {
            _applyingStyles = true;
            var finalValues = new Dictionary<string, string>(_declaredSettings);
            foreach (var name in finalValues.Keys)
            {
                ThrowIfPropertyNotValid(name);
            }


            // Helper to apply a style and it's parent styles
            void ApplyStyle(VarmintWidgetStyle style)
            {
                while (style != null)
                {
                    foreach (var stylePropertyName in style._declaredSettings.Keys)
                    {
                        if (!finalValues.ContainsKey(stylePropertyName))
                        {
                            finalValues.Add(stylePropertyName, style._declaredSettings[stylePropertyName]);
                        }
                    }
                    style = (VarmintWidgetStyle) style.Parent;
                }
            }

            if (Style != null)
            {
                if (styleLibrary == null || !styleLibrary.ContainsKey(Style))
                {
                    throw new ApplicationException("Style not found: " + Style);
                }

                ApplyStyle(styleLibrary[Style]);
            }

            if (styleLibrary != null && styleLibrary.Count > 0)
            {
                foreach (var style in styleLibrary.Values)
                {
                    if (style.AppliesToMe(this))
                    {
                        ApplyStyle(style);
                    }
                }
            }

            foreach (var name in finalValues.Keys)
            {
                if (IsBinding(finalValues[name]))
                {
                    AddBinding(name, finalValues[name]);
                }
                else
                {
                    SetValue(name, finalValues[name], false);
                }
            }

            _applyingStyles = false;
        }


        //--------------------------------------------------------------------------------------
        /// <summary>
        /// UpdateChildFormatting - the size has changed, so update the offsets and sizes of
        ///                         the children of this widget
        /// </summary>
        //--------------------------------------------------------------------------------------
        public void UpdateFormatting(Vector2 maxSizeExtent)
        {
            UpdateFormatting_Internal(maxSizeExtent);

            //if (_updating || _applyingStyles || !_prepared) return;
            //_updating = true;
            //// recurse first to ensure children that have a size determined by content
            //// update their size.
            //foreach (var child in Children)
            //{
            //   child.UpdateFormatting();
            //}
            //UpdateFormatting_Internal(updatedSize);
            //_updating = false;
        }

        protected virtual void UpdateFormatting_Internal(Vector2 maxSizeExtent)
        {
            if (SpecifiedSize == null) SpecifiedSize = new Tuple<float?, float?>(null, null);
            var width = SpecifiedSize.Item1 ?? Size.X;
            var maxWidth = maxSizeExtent.X - (Margin.Left ?? 0) - (Margin.Right ?? 0);
            if (maxWidth < 0) maxWidth = 0;
            if (width > maxWidth) width = maxWidth;

            var height = SpecifiedSize.Item2 ?? Size.Y;
            var maxheight = maxSizeExtent.Y - (Margin.Top ?? 0) - (Margin.Bottom ?? 0);
            if (maxheight < 0) maxheight = 0;
            if (height > maxheight) height = maxheight;

            var left = 0f;
            var top = 0f;

            var alignment = WidgetAlignment;

            // If alignment is not specified, use the margins and specified size as hints
            if (alignment.X == null)
            {
                if(SpecifiedSize.Item1 == null)
                {
                    alignment.X = Alignment.Stretch;
                }
                else
                {
                    if(Margin.Right == null)
                    {
                        alignment.X = Alignment.Left;
                    }
                    else if(Margin.Left == null)
                    {
                        alignment.X = Alignment.Right;
                    }
                    else
                    {
                        // This is bad, but we don't want to throw an exception here since 
                        // layered styles could likely cause this
                        alignment.X = Alignment.Left;
                    }
                }
            }
            if (alignment.Y == null)
            {
                if (SpecifiedSize.Item2 == null)
                {
                    alignment.Y = Alignment.Stretch;
                }
                else
                {
                    if (Margin.Bottom == null)
                    {
                        alignment.Y = Alignment.Top;
                    }
                    else if (Margin.Top == null)
                    {
                        alignment.Y = Alignment.Bottom;
                    }
                    else
                    {
                        // This is bad, but we don't want to throw an exception here since 
                        // layered styles could likely cause this
                        alignment.Y = Alignment.Top;
                    }
                }
            }

            // Horizontal Alignment
            switch (alignment.X)
            {
                case Alignment.Stretch:
                    if(SpecifiedSize?.Item1 != null)
                    {
                        throw new ArgumentException("Cannot specify Size.X with horizontal stretch alignment");
                    }
                    width = maxWidth;
                    left = Margin.Left ?? 0;
                    break;
                case Alignment.Left:
                    left = Margin.Left ?? 0;
                    break;
                case Alignment.Center:
                    left = (maxSizeExtent.X - width - (Margin.Left ?? 0) - (Margin.Right ?? 0)) / 2 + (Margin.Left ?? 0);
                    break;
                case Alignment.Right:
                    left = maxSizeExtent.X - width - (Margin.Right ?? 0);
                    break;
                default:
                    throw new ArgumentException($"Alignment Value {alignment.X} is not valid with Horizontal alignment" );

            }

            // Vertical Alignment
            switch (alignment.Y)
            {
                case Alignment.Stretch:
                    if (SpecifiedSize?.Item2 != null)
                    {
                        throw new ArgumentException("Cannot specify Size.Y with vertical stretch alignment");
                    }
                    height = maxheight;
                    top = Margin.Top ?? 0;
                    break;
                case Alignment.Top:
                    top = Margin.Top ?? 0;
                    break;
                case Alignment.Center:
                    top = (maxSizeExtent.Y - height - (Margin.Top ?? 0) - (Margin.Bottom ?? 0)) / 2 + (Margin.Top ?? 0);
                    break;
                case Alignment.Bottom:
                    top = maxSizeExtent.Y - height - (Margin.Bottom ?? 0);
                    break;
                default:
                    throw new ArgumentException($"Alignment Value {alignment.Y} is not valid with Vertical alignment");
            }

            Size = new Vector2(width, height);
            Offset = new Vector2(left, top);

            foreach (var child in Children)
            {
                child.UpdateFormatting_Internal(this.Size);
            }

            //if (updatedSize != null) Size = updatedSize.Value;
            //foreach (var child in Children)
            //{
            //    var newSize = child.IntendedSize;
            //    if (child.Stretch.Horizontal != null)
            //    {
            //        newSize.X = Size.X - ((child.Margin.Left ?? 0) + (child.Margin.Right ?? 0));
            //    }
            //    if (child.Stretch.Vertical != null)
            //    {
            //        newSize.Y = Size.Y - ((child.Margin.Top ?? 0) + (child.Margin.Bottom ?? 0));
            //    }
            //    var newOffset = Vector2.Zero;
            //    var availableSize = Size - newSize;
            //    availableSize.X -= (child.Margin.Left ?? 0) + (child.Margin.Right ?? 0);
            //    availableSize.Y -= (child.Margin.Top ?? 0) + (child.Margin.Bottom ?? 0);

            //    switch (HorizontalContentAlignment)
            //    {
            //        case HorizontalContentAlignment.Left:
            //            if (child.Margin.Left != null)
            //            {
            //                newOffset.X = child.Margin.Left.Value;
            //                if (child.Margin.Right != null) newSize.X += availableSize.X;
            //            }
            //            else
            //            {
            //                newOffset.X = child.Margin.Right == null ? 0 : availableSize.X;
            //            }

            //            break;
            //        case HorizontalContentAlignment.Center:
            //            var width = newSize.X + (child.Margin.Left ?? 0) + (child.Margin.Right ?? 0);
            //            newOffset.X = (Size.X - width) / 2 + (child.Margin.Left ?? 0);
            //            break;
            //        case HorizontalContentAlignment.Right:
            //            if (child.Margin.Left != null) newSize.X += availableSize.X;
            //            else
            //            {
            //                newOffset.X = availableSize.X;
            //            }
            //            break;
            //    }

            //    switch (VerticalContentAlignment)
            //    {
            //        case VerticalContentAlignment.Top:
            //            if (child.Margin.Top != null)
            //            {
            //                newOffset.Y = child.Margin.Top.Value;
            //                if (child.Margin.Bottom != null) newSize.Y += availableSize.Y;
            //            }
            //            else
            //            {
            //                newOffset.Y = child.Margin.Bottom == null? 0 : availableSize.Y;
            //            }
            //            break;
            //        case VerticalContentAlignment.Center:
            //            var height = newSize.Y + (child.Margin.Top ?? 0) + (child.Margin.Bottom ?? 0);
            //            newOffset.Y = (Size.Y - height) / 2 + (child.Margin.Top ?? 0);
            //            break;
            //        case VerticalContentAlignment.Bottom:
            //            if (child.Margin.Top != null) newSize.Y += availableSize.Y;
            //            else
            //            {
            //                newOffset.Y = availableSize.Y;
            //            }
            //            break;
            //    }

            //    child.Offset = newOffset;
            //    child.Size = newSize;

            //}
        }

    }
}