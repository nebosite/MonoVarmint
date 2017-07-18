using Microsoft.Xna.Framework;

namespace MonoVarmint.Widgets
{
    //--------------------------------------------------------------------------------------
    /// <summary>
    /// Button - Basic button
    /// </summary>
    //--------------------------------------------------------------------------------------
    [VarmintWidgetShortName("Button")]
    public class VarmintWidgetButton : VarmintWidgetLabel
    {
        public float LineWidth { get; set; }
        public bool HasBorder { get; set; }
        public string BackgroundImage { get; set; }
        //--------------------------------------------------------------------------------------
        /// <summary>
        /// ctor
        /// </summary>
        //--------------------------------------------------------------------------------------
        public VarmintWidgetButton(): base() 
        {
            LineWidth = 0.006f;
            FontSize = .05f;
            HasBorder = true;

            SetCustomRender(Render);
            OnFlick += Ignore_OnFlick;
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Ignore_OnFlick - Button should always ignore flicks
        /// </summary>
        //--------------------------------------------------------------------------------------
        private EventHandledState Ignore_OnFlick(VarmintWidget arg1, Vector2 arg2, Vector2 arg3)
        {
            return EventHandledState.Handled;
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Render
        /// </summary>
        //--------------------------------------------------------------------------------------
        private void Render(GameTime gameTime, VarmintWidget widget)
        {
            var textToDisplay = (Content == null) ? "" : Content.ToString();

            if (BackgroundImage != null)
            {
                LineWidth = 0;
                Renderer.DrawGlyph(BackgroundImage, AbsoluteOffset, Size, BackgroundColor);
            }
            else
            {
                Renderer.DrawBox(AbsoluteOffset, Size, BackgroundColor);
            }

            Vector2 alignedOffset = AbsoluteOffset;
            Vector2 textSize = Renderer.MeasureText(textToDisplay, FontName, FontSize);
            alignedOffset.X += (Size.X - textSize.X) / 2;
            alignedOffset.Y += (Size.Y - textSize.Y) / 2;

            Renderer.DrawText(textToDisplay, FontName, FontSize, alignedOffset, ForegroundColor, WrapContent ? Size.X : 0);

            if (HasBorder)
            {
                Renderer.DrawLine(AbsoluteOffset, AbsoluteOffset + new Vector2(Size.X, 0), LineWidth, ForegroundColor);
                Renderer.DrawLine(AbsoluteOffset, AbsoluteOffset + new Vector2(0, Size.Y), LineWidth, ForegroundColor);
                Renderer.DrawLine(AbsoluteOffset + Size, AbsoluteOffset + new Vector2(Size.X, 0), LineWidth, ForegroundColor);
                Renderer.DrawLine(AbsoluteOffset + Size, AbsoluteOffset + new Vector2(0, Size.Y), LineWidth, ForegroundColor);
            }
        }
    }
}