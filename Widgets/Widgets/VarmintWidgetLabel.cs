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
        public float FontSize { get; set; }
        public string FontName { get; set; }
        public Vector2 TextOffset { get; set; }
        //--------------------------------------------------------------------------------------
        /// <summary>
        /// ctor
        /// </summary>
        //--------------------------------------------------------------------------------------
        public VarmintWidgetLabel()
        {
            this.FontSize = 0.1f;
            this.OnRender += Render;
            this.TextOffset = new Vector2();
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Render
        /// </summary>
        //--------------------------------------------------------------------------------------
        void Render(GameTime gameTime, VarmintWidget widget)
        {
            var textToDisplay = (Content == null) ? "" : Content.ToString();

            Renderer.DrawBox(AbsoluteOffset, Size, BackgroundColor);
            Vector2 alignedOffset = AbsoluteOffset;
            var margin = 0f;
            if (WrapContent) margin = Size.X;
            Vector2 textSize = Renderer.MeasureText(textToDisplay, FontName, FontSize, margin);
            switch (HorizontalContentAlignment)
            {
                case HorizontalContentAlignment.Left: break;
                case HorizontalContentAlignment.Center: alignedOffset.X += (Size.X - textSize.X) / 2; break;
                case HorizontalContentAlignment.Right: alignedOffset.X += (Size.X - textSize.X); break;
            }

            switch(VerticalContentAlignment)
            {
                case VerticalContentAlignment.Top: break;
                case VerticalContentAlignment.Center: alignedOffset.Y += (Size.Y - textSize.Y) / 2; break;
                case VerticalContentAlignment.Bottom: alignedOffset.Y += (Size.Y - textSize.Y);  break;
            }

            Renderer.DrawText(textToDisplay, FontName, FontSize, alignedOffset + TextOffset, ForegroundColor, margin);
        }
    }
}