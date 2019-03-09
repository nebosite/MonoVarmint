using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonoVarmint.Widgets
{
    //---------------------------------------------------------------------------------
    /// <summary>
    /// Individual details for a singled sliced block
    /// </summary>
    //---------------------------------------------------------------------------------
    public class SliceValue
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public float OnScreenWidth { get; private set; }
        public float OnScreenHeight { get; private set; }

        public SliceValue(int x, int y, int w, int h, float pixelDimension)
        {
            X = x;
            Y = y;
            Width = w;
            Height = h;
            OnScreenWidth = Width * pixelDimension;
            OnScreenHeight = Height * pixelDimension;
        }
    }

    //---------------------------------------------------------------------------------
    /// <summary>
    /// Stores all the relevant details needed for a 9-sliced texture
    /// </summary>
    //---------------------------------------------------------------------------------
    public class SlicedTexture
    {
        public Texture2D Texture { get; set; }

        SliceValue[,] _sliceDimensions;
        public SliceValue[,] SliceDimensions => _sliceDimensions;

        public float OnScreenWidth { get;  set; }


        //---------------------------------------------------------------------------------
        /// <summary>
        /// Ctor
        /// </summary>
        //---------------------------------------------------------------------------------
        public SlicedTexture(Texture2D texture, int xSlice1InPixels, int xSlice2InPixels, int ySlice1InPixels, int ySlice2InPixels, float onscreenWidth)
        {
            Texture = texture;
            var pixelDimension = onscreenWidth / texture.Width;
            _sliceDimensions = new SliceValue[3, 3];
            var w0 = xSlice1InPixels;
            var w1 = xSlice2InPixels - xSlice1InPixels;
            var w2 = texture.Width - xSlice2InPixels;
            var h0 = ySlice1InPixels;
            var h1 = ySlice2InPixels - ySlice1InPixels;
            var h2 = texture.Height - ySlice2InPixels;
            


            _sliceDimensions[0, 0] = new SliceValue(0, 0, w0, h0, pixelDimension);
            _sliceDimensions[1, 0] = new SliceValue(xSlice1InPixels, 0, w1, h0, pixelDimension);
            _sliceDimensions[2, 0] = new SliceValue(xSlice2InPixels, 0, w2, h0, pixelDimension);
            _sliceDimensions[0, 1] = new SliceValue(0, ySlice1InPixels, w0, h1, pixelDimension);
            _sliceDimensions[1, 1] = new SliceValue(xSlice1InPixels, ySlice1InPixels, w1, h1, pixelDimension);
            _sliceDimensions[2, 1] = new SliceValue(xSlice2InPixels, ySlice1InPixels, w2, h1, pixelDimension);
            _sliceDimensions[0, 2] = new SliceValue(0, ySlice2InPixels, w0, h2, pixelDimension);
            _sliceDimensions[1, 2] = new SliceValue(xSlice1InPixels, ySlice2InPixels, w1, h2, pixelDimension);
            _sliceDimensions[2, 2] = new SliceValue(xSlice2InPixels, ySlice2InPixels, w2, h2, pixelDimension);
        }
    }
}
