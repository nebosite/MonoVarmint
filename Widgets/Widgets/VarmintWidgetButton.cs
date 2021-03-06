using Microsoft.Xna.Framework;

namespace MonoVarmint.Widgets
{
    //--------------------------------------------------------------------------------------
    /// <summary>
    /// Button - Basic button
    /// </summary>
    //--------------------------------------------------------------------------------------
    [VarmintWidgetShortName("Button")]
    public class VarmintWidgetButton : VarmintWidget
    {
        /// <summary>
        /// LineWidth (Deprecated- use the content to do the drawing of borders)
        /// </summary>
        public float LineWidth { get; set; }

        /// <summary>
        /// HasBorder (Deprecated- use the content to do the drawing of borders)
        /// </summary>
        public bool HasBorder { get; set; }

        public override object Content
        {
            get => base.Content;
            set
            {
                if(value is string)
                {
                    var label = new VarmintWidgetLabel();
                    label.Content = value;
                    label.Renderer = this.Renderer;
                    base.Content = label;
                }
                else base.Content = value;
            }
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// ctor
        /// </summary>
        //--------------------------------------------------------------------------------------
        public VarmintWidgetButton(): base() 
        {
            LineWidth = 0.006f;
            SetCustomRender(Render);
            OnFlick += Ignore_OnFlick;
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Ignore_OnFlick - Button should always ignore flicks
        /// </summary>
        //--------------------------------------------------------------------------------------
        private EventHandledState Ignore_OnFlick(VarmintFlickData flick)
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
            if (Content is VarmintWidgetImage || Content is VarmintWidgetSprite) { return; }

            var textToDisplay = (Content == null) ? "" : Content.ToString();
            Renderer.DrawBox(AbsoluteOffset, Size, RenderBackgroundColor);

            if (HasBorder)
            {
                Renderer.DrawLine(AbsoluteOffset, AbsoluteOffset + new Vector2(Size.X, 0), LineWidth, RenderForegroundColor);
                Renderer.DrawLine(AbsoluteOffset, AbsoluteOffset + new Vector2(0, Size.Y), LineWidth, RenderForegroundColor);
                Renderer.DrawLine(AbsoluteOffset + Size, AbsoluteOffset + new Vector2(Size.X, 0), LineWidth, RenderForegroundColor);
                Renderer.DrawLine(AbsoluteOffset + Size, AbsoluteOffset + new Vector2(0, Size.Y), LineWidth, RenderForegroundColor);
            }
        }
    }
}