using Microsoft.Xna.Framework;

namespace MonoVarmint.Widgets
{
    //--------------------------------------------------------------------------------------
    /// <summary>
    /// VarmintWidgetImage
    /// </summary>
    //--------------------------------------------------------------------------------------
    [VarmintWidgetShortName("Image")]
    public class VarmintWidgetImage : VarmintWidget
    {
        //--------------------------------------------------------------------------------------
        /// <summary>
        /// ctor
        /// </summary>
        //--------------------------------------------------------------------------------------
        public VarmintWidgetImage() : base()
        {
            SetCustomRender(Render);
        }


        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Render
        /// </summary>
        //--------------------------------------------------------------------------------------
        private void Render(GameTime gameTime, VarmintWidget widget)
        {
            if (Content == null) return;
            Renderer.DrawGlyph(Content.ToString(), AbsoluteOffset, Size, RenderGraphicColor);
        }
    }

}