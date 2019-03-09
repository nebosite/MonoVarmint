using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MonoVarmint.Widgets
{
    //--------------------------------------------------------------------------------------
    /// <summary>
    /// VarmintWidgetNineSlice - a dummy widget type to make it easy to specify nineslices
    /// </summary>
    //--------------------------------------------------------------------------------------
    [VarmintWidgetShortName("NineSlice")]
    public class VarmintWidgetNineSlice: VarmintWidget
    {
        public int[] XSlices { get; set; }
        public int[] YSlices { get; set; }
        public float OnScreenWidth { get; set; }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// ctor
        /// </summary>
        //--------------------------------------------------------------------------------------
        public VarmintWidgetNineSlice() : base()
        {
            SetCustomRender((gt, w) => {  throw new ApplicationException("Should not try to render NineSlice widgets");  });
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Get the 
        /// </summary>
        //--------------------------------------------------------------------------------------
        public SlicedTexture GetSlicedTexture(Texture2D texture)
        {
            return new SlicedTexture(texture, XSlices[0], XSlices[1], YSlices[0], YSlices[1], OnScreenWidth);
        }
    }

}