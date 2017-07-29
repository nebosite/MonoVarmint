using Microsoft.Xna.Framework;


namespace MonoVarmint.Widgets
{
    //--------------------------------------------------------------------------------------
    /// <summary>
    /// IMediaRenderer - Interface to abstract the drawing framework from the widget library
    /// </summary>
    //--------------------------------------------------------------------------------------
    public interface IMediaRenderer
    {
        void DrawBox(Vector2 offset, Vector2 size, Color fillColor);
        void DrawEllipse(Vector2 offset, Vector2 size, Color fillColor);
        void DrawText(string text, string fontName, float fontSize, Vector2 offset, Color color, float wrapWidth = 0);
        Vector2 MeasureText(string text, string fontName, float fontSize, float wrapWidth = 0);
        void DrawLine(Vector2 start, Vector2 end, float lineThickness, Color color);
        void DrawGlyph(string name, Vector2 offset, Vector2 size, Color color);
        void DrawGlyph(string name, Vector2 offset, Vector2 size, Color color, float rotation, Vector2 origin);
        void DrawSprite(string name, int spriteFrame, Vector2 offset, Vector2 size, Color color);
        void DrawSprite(string name, int spriteFrame, Vector2 offset, Vector2 size, Color color, float rotation, Vector2 rotationOrigin);
        void BeginClipping(Vector2 position, Vector2 size);
        void EndClipping(float rotation, Vector2 rotationOrigin, Vector2 scale, bool flipHorizontal, bool flipVertical);
        bool IsInRenderingWindow(Vector2 offset, Vector2 size);
    }
}