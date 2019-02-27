using Microsoft.Xna.Framework;
using MonoVarmint.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoVarmint.Tools.Tests
{
    class MockRenderer : IMediaRenderer
    {
        public Vector2 ScreenSize { get; set; }
        public Vector2 MeasureTextReturn { get; internal set; }

        public void BeginClipping(Vector2 position, Vector2 size) { }
        public void DrawBox(Vector2 offset, Vector2 size, Color fillColor) { }
        public void DrawEllipse(Vector2 offset, Vector2 size, Color fillColor) { }
        public void DrawGlyph(string name, Vector2 offset, Vector2 size, Color color) { }
        public void DrawGlyph(string name, Vector2 offset, Vector2 size, Color color, float rotation, Vector2 origin) { }
        public void DrawLine(Vector2 start, Vector2 end, float lineThickness, Color color) { }
        public void DrawRectangle(Vector2 offset, Vector2 size, float lineWidth, Color color) { }
        public void DrawSprite(string name, int spriteFrame, Vector2 offset, Vector2 size, Color color) { }
        public void DrawSprite(string name, int spriteFrame, Vector2 offset, Vector2 size, Color color, float rotation, Vector2 rotationOrigin) { }
        public void DrawText(string text, string fontName, float fontSize, Vector2 offset, Color color, float wrapWidth = 0) { }
        public void EndClipping(float rotation, Vector2 rotationOrigin, Vector2 scale, bool flipHorizontal, bool flipVertical) { }
        public bool IsInRenderingWindow(Vector2 offset, Vector2 size) { return false; }
        public Vector2 MeasureText(string text, string fontName, float fontSize, float wrapWidth = 0) { return MeasureTextReturn; }
        public IVarmintAudioInstance PlaySound(string name) { return null; }
        public void Vibrate(long milliseconds) { }

        public MockRenderer(float width, float height)
        {
            ScreenSize = new Vector2(width, height);
        }
    }


}
