using Microsoft.Xna.Framework;

namespace MonoVarmint.Widgets
{
    //--------------------------------------------------------------------------------------
    /// <summary>
    /// VarmintWidgetGrid
    /// </summary>
    //--------------------------------------------------------------------------------------
    [VarmintWidgetShortName("Grid")]
    public class VarmintWidgetGrid : VarmintWidget
    {
        //--------------------------------------------------------------------------------------
        /// <summary>
        /// ctor
        /// </summary>
        //--------------------------------------------------------------------------------------
        public VarmintWidgetGrid()
        {
            this.OnRender += Render;
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Render
        /// </summary>
        //--------------------------------------------------------------------------------------
        void Render(GameTime gameTime, VarmintWidget widget)
        {
           Renderer.DrawBox(AbsoluteOffset, Size, RenderBackgroundColor);
        }
    }
}