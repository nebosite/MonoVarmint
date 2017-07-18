using Microsoft.Xna.Framework;

namespace MonoVarmint.Widgets
{
    //--------------------------------------------------------------------------------------
    /// <summary>
    /// VarmintWidgetSprite
    /// </summary>
    //--------------------------------------------------------------------------------------
    [VarmintWidgetShortName("Sprite")]
    public class VarmintWidgetSprite : VarmintWidget
    {
        public int Frame { get; set; }
        //--------------------------------------------------------------------------------------
        /// <summary>
        /// ctor
        /// </summary>
        //--------------------------------------------------------------------------------------
        public VarmintWidgetSprite() : base()
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
            Renderer.DrawSprite(Content.ToString(), Frame, Offset, Size, Color.White);
        }
    }

}