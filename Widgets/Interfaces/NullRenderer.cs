﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonoVarmint.Widgets
{
     public class NullRenderer : IMediaRenderer
    {
        public void BeginClipping(Vector2 position, Vector2 size) { }
        public void DrawBox(Vector2 offset, Vector2 size, Color fillColor) { }
        public void DrawEllipse(Vector2 offset, Vector2 size, Color fillColor) { }
        public void DrawGlyph(string name, Vector2 offset, Vector2 size, Color color) { }
        public void DrawGlyph(string name, Vector2 offset, Vector2 size, Color color, float rotation, Vector2 origin) { }
        public void DrawLine(Vector2 start, Vector2 end, float lineThickness, Color color) { }
        public void DrawSprite(string name, int spriteFrame, Vector2 offset, Vector2 size, Color color) { }
        public void DrawSprite(string name, int spriteFrame, Vector2 offset, Vector2 size, Color color, float rotation, Vector2 rotationOrigin) { }
        public void DrawText(string text, string fontName, float fontSize, Vector2 offset, Color color, float wrapWidth = 0) { }
        public void EndClipping(float rotation, Vector2 rotationOrigin, Vector2 scale, bool flipHorizontal, bool flipVertical) { }
        public bool IsInRenderingWindow(Vector2 offset, Vector2 size) { return true; }
        public Vector2 MeasureText(string text, string fontName, float fontSize, float wrapWidth = 0) { return Vector2.Zero; }
        public void PlaySound(string name, double volume = 1) { }
    }
}