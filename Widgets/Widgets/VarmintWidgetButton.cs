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
            if (Content is VarmintWidgetImage) { return; }

            Renderer.DrawBox(AbsoluteOffset, Size, BackgroundColor);

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