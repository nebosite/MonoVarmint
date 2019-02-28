using Microsoft.Xna.Framework;

namespace MonoVarmint.Widgets
{
    //--------------------------------------------------------------------------------------
    /// <summary>
    /// VarmintWidgetLabel - simple text widget
    /// </summary>
    //--------------------------------------------------------------------------------------
    [VarmintWidgetShortName("Label")]
    public class VarmintWidgetLabel : VarmintWidget
    {

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// ctor
        /// </summary>
        //--------------------------------------------------------------------------------------
        public VarmintWidgetLabel()
        {
            this.OnRender += Render;
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Render
        /// </summary>
        //--------------------------------------------------------------------------------------
        protected override void UpdateFormatting_Internal(Vector2 updatedSize)
        {
            var width = updatedSize.X;
            var height = updatedSize.Y;

            var textSize = Renderer.MeasureText(Content.ToString(), FontName, FontSize, WrapContent ? width : 0);
            if (SpecifiedSize?.Item1 == null && WidgetAlignment.X != Alignment.Stretch)
            {
                width = textSize.X;
            }
            if (SpecifiedSize?.Item2 == null && WidgetAlignment.Y != Alignment.Stretch)
            {
                height = textSize.Y;
            }

            Size = new Vector2(width, height);

            base.UpdateFormatting_Internal(updatedSize);
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Render
        /// </summary>
        //--------------------------------------------------------------------------------------
        void Render(GameTime gameTime, VarmintWidget widget)
        {
            var textToDisplay = (Content == null) ? "" : Content.ToString();
            var adjustedSize = Size;
            if(!HasSize)
            {
                adjustedSize = Renderer.MeasureText(textToDisplay, FontName, FontSize, WrapContent ? Parent.Size.X : 0 );
            }

            Renderer.DrawBox(AbsoluteOffset, adjustedSize, RenderBackgroundColor);
            Vector2 alignedOffset = AbsoluteOffset;
            var margin = 0f;
            if (WrapContent) margin = adjustedSize.X;
            Vector2 textSize = Renderer.MeasureText(textToDisplay, FontName, FontSize, margin);
            switch (HorizontalContentAlignment)
            {
                case HorizontalContentAlignment.Left: break;
                case HorizontalContentAlignment.Center: alignedOffset.X += (adjustedSize.X - textSize.X) / 2; break;
                case HorizontalContentAlignment.Right: alignedOffset.X += (adjustedSize.X - textSize.X); break;
            }

            switch(VerticalContentAlignment)
            {
                case VerticalContentAlignment.Top: break;
                case VerticalContentAlignment.Center: alignedOffset.Y += (adjustedSize.Y - textSize.Y) / 2; break;
                case VerticalContentAlignment.Bottom: alignedOffset.Y += (adjustedSize.Y - textSize.Y);  break;
            }

            Renderer.DrawText(textToDisplay, FontName, FontSize, alignedOffset, RenderForegroundColor, margin);
        }
    }
}