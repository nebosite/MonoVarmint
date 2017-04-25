using Microsoft.Xna.Framework;


namespace MonoVarmint.Widgets
{
    //--------------------------------------------------------------------------------------
    /// <summary>
    /// IMediaRenderer - Interface to abstract the drawing framework
    /// </summary>
    //--------------------------------------------------------------------------------------
    public interface IMediaRenderer
    {
        void DrawBox(Vector2 offset, Vector2 size, Color color);
        void DrawEllipse(Vector2 offset, Vector2 size, Color color);
        void DrawText(Vector2 offset, float fontSize, string text, Color color, float wrapWidth = 0);
        Vector2 MeasureText(float fontSize, string text, float wrapWidth = 0);
        void DrawLine(Vector2 start, Vector2 end, float lineThickness, Color color);
        void DrawGlyph(string name, Vector2 offset, Vector2 size, Color color);
        void DrawGlyph(int glyphId, Vector2 offset, Vector2 size, Color color);
        void DrawGlyph(int glyphId, Vector2 offset, Vector2 size, Color color, float rotation, Vector2 origin);
        void DrawSprite(int spriteId, int spriteFrame, Vector2 offset, Vector2 size, Color color);
        void PlaySound(int soundEffectId, double volume = 1.0);
    }
}