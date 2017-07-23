using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace MonoVarmint.Widgets
{
    public partial class VarmintWidget
    {
        bool _applyingStyles = false;

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
                throw new InvalidOperationException("The Renderer property is null on " + this.GetType().Name + " with Name=" + Name);
            }

            Update();
            if (!IsVisible) return;
            if (Size.X > 0 && Size.Y > 0)
            {
                Renderer.BeginInnerCoordinateSpace(this, Size);

                OnRender?.Invoke(gameTime, this);

                Renderer.EndInnerCoordinateSpace();
            }

            RenderChildren(gameTime);
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
                child.RenderMe(gameTime);
            }
        }

        public virtual void Compose(GameTime gameTime)
        {
            ComposeInternal(Matrix.Identity);
        }

        private void ComposeInternal(Matrix transform)
        {
            // TODO:HANS - Varmint widgets should not know about radians
            if (FlipHorizontal) transform *= Matrix.CreateScale(-1, 1, 1);
            if (FlipVertical) transform *= Matrix.CreateScale(1, -1, 1);
            transform *= Matrix.CreateTranslation(-Size.X / 2, -Size.Y / 2, 0);
            transform *= Matrix.CreateRotationZ(Rotate * MathHelper.Pi / 180);
            transform *= Matrix.CreateTranslation(Size.X / 2, Size.Y / 2, 0);
            transform *= Matrix.CreateTranslation(Offset.X, Offset.Y, 0);
            Renderer.DrawCachedWidget(this, transform);
            foreach (var child in Children)
            {
                child.ComposeInternal(transform);
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
            Action<VarmintWidgetStyle> applyStyle = (style) =>
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
            };

            if (Style != null)
            {
                if (styleLibrary == null || !styleLibrary.ContainsKey(Style))
                {
                    throw new ApplicationException("Style not found: " + Style);
                }

                applyStyle(styleLibrary[Style]);
            }

            if (styleLibrary != null && styleLibrary.Count > 0)
            {
                foreach (var style in styleLibrary.Values)
                {
                    if (style.AppliesToMe(this))
                    {
                        applyStyle(style);
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
        public void UpdateChildFormatting(Vector2? updatedSize = null)
        {
            if (_updating || _applyingStyles || !_prepared) return;
            _updating = true;
            // recurse first to ensure children that have a size determined by content
            // update their size.
            foreach (var child in Children)
            {
               child.UpdateChildFormatting();
            }
            UpdateChildFormatting_Internal(updatedSize);
            _updating = false;
        }

        protected virtual void UpdateChildFormatting_Internal(Vector2? updatedSize)
        {
            if (updatedSize != null) Size = updatedSize.Value;
            foreach (var child in Children)
            {
                var newSize = child.IntendedSize;
                if (child.Stretch.Horizontal != null) newSize.X = Size.X - ((child.Margin.Left ?? 0) + (child.Margin.Right ?? 0));
                if (child.Stretch.Vertical != null) newSize.Y = Size.Y - ((child.Margin.Top ?? 0) + (child.Margin.Bottom ?? 0));
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