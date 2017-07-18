using Microsoft.Xna.Framework;
using System;

namespace MonoVarmint.Widgets
{
    //--------------------------------------------------------------------------------------
    /// <summary>
    /// IMediaRenderer - Interface to abstract the drawing framework from the widget library
    /// </summary>
    //--------------------------------------------------------------------------------------
    public interface IMediaRenderer
    {
        /// <summary>
        /// Draw a simple colored box at the given coordinates
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <param name="fillColor"></param>
        void DrawBox(Vector2 offset, Vector2 size, Color fillColor);
        /// <summary>
        /// Draw a simple colored ellipse at the given coordinates
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <param name="fillColor"></param>
        void DrawEllipse(Vector2 offset, Vector2 size, Color fillColor);
        /// <summary>
        /// Draw the given text at the given coordinates
        /// </summary>
        /// <param name="text"></param>
        /// <param name="fontName"></param>
        /// <param name="fontSize"></param>
        /// <param name="offset"></param>
        /// <param name="color"></param>
        /// <param name="wrapWidth"></param>
        void DrawText(string text, string fontName, float fontSize, Vector2 offset, Color color, float wrapWidth = 0);
        /// <summary>
        /// Return the size of a given string
        /// </summary>
        /// <param name="text"></param>
        /// <param name="fontName"></param>
        /// <param name="fontSize"></param>
        /// <param name="wrapWidth"></param>
        /// <returns></returns>
        Vector2 MeasureText(string text, string fontName, float fontSize, float wrapWidth = 0);
        /// <summary>
        /// Draw a straight line between the given coordinates at the given thickness
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="lineThickness"></param>
        /// <param name="color"></param>
        void DrawLine(Vector2 start, Vector2 end, float lineThickness, Color color);
        /// <summary>
        /// Draw a named glyph at the given coordinates
        /// </summary>
        /// <param name="name"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <param name="color"></param>
        void DrawGlyph(string name, Vector2 offset, Vector2 size, Color color);
        /// <summary>
        /// Draw a named glyph at the given coordinates with the given rotation
        /// </summary>
        /// <param name="name"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <param name="color"></param>
        /// <param name="rotation"></param>
        /// <param name="origin"></param>
        void DrawGlyph(string name, Vector2 offset, Vector2 size, Color color, float rotation, Vector2 origin);
        /// <summary>
        /// Draw a sprite at the given coordinates
        /// </summary>
        /// <param name="name"></param>
        /// <param name="spriteFrame"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <param name="color"></param>
        void DrawSprite(string name, int spriteFrame, Vector2 offset, Vector2 size, Color color);
        /// <summary>
        /// Draw a sprite at the given coordinates with the given rotation
        /// </summary>
        /// <param name="name"></param>
        /// <param name="spriteFrame"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <param name="color"></param>
        /// <param name="rotation"></param>
        /// <param name="rotationOrigin"></param>
        void DrawSprite(string name, int spriteFrame, Vector2 offset, Vector2 size, Color color, float rotation, Vector2 rotationOrigin);
        /// <summary>
        /// Play the given sound at the given volume
        /// </summary>
        /// <param name="name"></param>
        /// <param name="volume"></param>
        void PlaySound(string name, double volume = 1.0);
        /// <summary>
        /// Cause all rendering operations to be relative to the given coordinate space
        /// </summary>
        /// <remarks>
        /// This method is obsolete. The widget was used to facilitate widget caching
        /// (as per <see cref="DrawCachedWidget(VarmintWidget, Matrix)"/>), which will
        /// no longer be needed to support this method. Users of this method
        /// should use <see cref="BeginInnerCoordinateSpace(Vector2, Vector2, float, bool, bool, bool)"/>
        /// instead.
        /// </remarks>
        [Obsolete]
        void BeginInnerCoordinateSpace(VarmintWidget widget, Vector2 size);

        /// <summary>
        /// Cause all rendering operations to be relative to the given coordinate space
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <param name="rotate"></param>
        /// <param name="flipHorizontally"></param>
        /// <param name="flipVertically"></param>
        /// <param name="shouldClip"></param>
        void BeginInnerCoordinateSpace(Vector2 offset, Vector2 size, float rotate, Vector2 rotationOrigin, bool flipHorizontally, bool flipVertically, bool shouldClip);
        /// <summary>
        /// Revert to the previously set coordinate space
        /// </summary>
        void EndInnerCoordinateSpace();

        /// <summary>
        /// Draw the cached image data for the given widget
        /// </summary>
        /// <remarks>
        /// This method is obsolete. Widget caching will not be supported in future revisions
        /// </remarks>
        /// <param name="widget"></param>
        /// <param name="transform"></param>
        [Obsolete]
        void DrawCachedWidget(VarmintWidget widget, Matrix transform);

        /// <summary>
        /// Returns true if the given point is inside the rendering window
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        bool IsInRenderingWindow(Vector2 offset, Vector2 size);
    }
}