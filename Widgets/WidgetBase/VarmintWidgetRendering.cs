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
            if (Renderer == null)
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

            if (VisualDebuggingEnabled)
            {
                Renderer.DrawRectangle(AbsoluteOffset, Size, .003f, Color.Red);
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
                    style = (VarmintWidgetStyle)style.Parent;
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
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// GetMaxDimentsions - figure out the max size dimensions using margin and max
        /// exterior size.  Also figure out initial values for width and height that are
        /// cropped my the max size.
        /// </summary>
        //--------------------------------------------------------------------------------------
        protected Vector2 GetMaxDimentsions(Vector2 maxSizeExtent, out float croppedWidth, out float croppedHeight)
        {
            if (SpecifiedSize == null) SpecifiedSize = new Tuple<float?, float?>(null, null);
            croppedWidth = SpecifiedSize.Item1 ?? Size.X;
            var maxWidth = maxSizeExtent.X - (Margin.Left ?? 0) - (Margin.Right ?? 0);
            if (maxWidth < 0) maxWidth = 0;
            if (croppedWidth > maxWidth) croppedWidth = maxWidth;

            croppedHeight = SpecifiedSize.Item2 ?? Size.Y;
            var maxheight = maxSizeExtent.Y - (Margin.Top ?? 0) - (Margin.Bottom ?? 0);
            if (maxheight < 0) maxheight = 0;
            if (croppedHeight > maxheight) croppedHeight = maxheight;

            return new Vector2(maxWidth, maxheight);
        }


        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Standard widget formatting to set size and offset values
        /// </summary>
        //--------------------------------------------------------------------------------------
        protected virtual void UpdateFormatting_Internal(Vector2 maxSizeExtent, bool updateChildren = true)
        {
            var maxSize = GetMaxDimentsions(maxSizeExtent, out var width, out var height);
            var left = 0f;
            var top = 0f;

            var alignment = MyAlignment;

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
                        throw new ArgumentException($"Cannot specify Size.X with horizontal stretch alignment.   Widget: {Name}");
                    }
                    width = maxSize.X;
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
                    throw new ArgumentException($"Alignment Value {alignment.X} is not valid with Horizontal alignment.   Widget: {Name}" );

            }

            // Vertical Alignment
            switch (alignment.Y)
            {
                case Alignment.Stretch:
                    if (SpecifiedSize?.Item2 != null)
                    {
                        throw new ArgumentException($"Cannot specify Size.Y with vertical stretch alignment.   Widget: {Name}");
                    }
                    height = maxSize.Y;
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
                    throw new ArgumentException($"Alignment Value {alignment.Y} is not valid with Vertical alignment.  Widget: {Name}");
            }

            Size = new Vector2(width, height);
            Offset = new Vector2(left, top);

            if(updateChildren)
            {
                foreach (var child in Children)
                {
                    child.UpdateFormatting(this.Size);
                }
            }
        }
    }
}