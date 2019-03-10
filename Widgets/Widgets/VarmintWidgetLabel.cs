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
        /// UpdateFormatting_Internal
        /// </summary>
        //--------------------------------------------------------------------------------------
        protected override void UpdateFormatting_Internal(Vector2 updatedSize, bool updateChildren = true)
        {
            var width = updatedSize.X;
            var height = updatedSize.Y;

            var textSize = Renderer.MeasureText(Content?.ToString(), FontName, FontSize, WrapContent ? width : 0);
            if (SpecifiedSize?.Item1 == null && MyAlignment.X != Alignment.Stretch)
            {
                width = textSize.X;
            }
            if (SpecifiedSize?.Item2 == null && MyAlignment.Y != Alignment.Stretch)
            {
                height = textSize.Y;
            }

            Size = new Vector2(width, height);

            base.UpdateFormatting_Internal(updatedSize, updateChildren);
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Render
        /// </summary>
        //--------------------------------------------------------------------------------------
        void Render(GameTime gameTime, VarmintWidget widget)
        {
            var textToDisplay = (Content == null) ? "" : Content.ToString();

            Renderer.DrawBox(base.AbsoluteOffset, Size, base.RenderBackgroundColor);
            Vector2 alignedOffset = AbsoluteOffset;
            var margin = 0f;
            if (WrapContent) margin = Size.X;
            Vector2 textSize = Renderer.MeasureText(textToDisplay, FontName, FontSize, margin);
            switch (ContentAlignment?.X)
            {
                case Alignment.Left: break;
                case Alignment.Center: alignedOffset.X += (Size.X - textSize.X) / 2; break;
                case Alignment.Right: alignedOffset.X += (Size.X - textSize.X); break;
            }

            switch(ContentAlignment?.Y)
            {
                case Alignment.Top: break;
                case Alignment.Center: alignedOffset.Y += (Size.Y - textSize.Y) / 2; break;
                case Alignment.Bottom: alignedOffset.Y += (Size.Y - textSize.Y);  break;
            }

            Renderer.DrawText(textToDisplay, FontName, FontSize, alignedOffset, RenderForegroundColor, margin);
        }
    }
}