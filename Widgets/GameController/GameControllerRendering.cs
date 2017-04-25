using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;

namespace MonoVarmint.Widgets
{
    public partial class GameController : IMediaRenderer
    {
        GraphicsDeviceManager _graphics;
        SpriteBatch _spriteBatch;
        ContentManager _content;

        public double SoundVolume { get; set; }

        // these are used to allow a portrait-oriented app on any resolution
        Matrix _scaleToNativeResolution;
        int _backBufferWidth;
        int _backBufferHeight;
        int _backBufferXOffset;
        RenderTarget2D _backBuffer;
        float _selectedFontPixelSize;

        VarmintWidget _visualTree;
        Texture2D _utilityBlockTexture;
        Texture2D _circleTexture;
        SpriteFont _selectedFont;
        SpriteFont _defaultFont;

        bool _inSpriteBatch = false;
        Dictionary<string, SpriteFont> _fontsByName = new Dictionary<string, SpriteFont>();
        Dictionary<string, VarmintSoundEffect> _soundsByName = new Dictionary<string, VarmintSoundEffect>();
        Dictionary<string, Texture2D> _texturesByName = new Dictionary<string, Texture2D>();
        Dictionary<string, VarmintSprite> _spritesByName = new Dictionary<string, VarmintSprite>();

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// VarmintSoundEffect - Tacks on some useful info for playing sound effects
        /// </summary>
        //--------------------------------------------------------------------------------------
        public class VarmintSoundEffect
        {
            public SoundEffect Effect { get; set; }
            public double PreferredVolume { get; set; }
            public DateTime LastPlayTime { get; set; }

            public VarmintSoundEffect() { PreferredVolume = 1.0; LastPlayTime = DateTime.MinValue; }
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// VarmintSprite - A texture with regular sized sub-images
        /// </summary>
        //--------------------------------------------------------------------------------------
        public class VarmintSprite
        {
            public Texture2D Texture { get; private set; }
            public int Width { get; private set; }
            public int Height { get; private set; }
            int _rows;
            int _columns;
            int _totalSpriteCount;

            //--------------------------------------------------------------------------------------
            /// <summary>
            /// ctor
            /// </summary>
            //--------------------------------------------------------------------------------------
            public VarmintSprite(Texture2D texture, int width, int height)
            {
                Width = width;
                Height = height;

                Texture = texture;
                if (texture.Height % height != 0) throw new ApplicationException("The sprite texture height "
                     + texture.Height + " is not evenly divisible by the sprite height " + height + ".");
                if (texture.Width % width != 0) throw new ApplicationException("The sprite texture width "
                     + texture.Width + " is not evenly divisible by the sprite width " + width + ".");
                _rows = texture.Height / height;
                _columns = texture.Width / width;
                _totalSpriteCount = _rows * _columns;
            }

            //--------------------------------------------------------------------------------------
            /// <summary>
            /// GetRectangle - get the source rectangle for a specific sprite image
            /// </summary>
            //--------------------------------------------------------------------------------------
            public Rectangle GetRectangle(int spriteNumber)
            {
                if (spriteNumber >= _totalSpriteCount) throw new ApplicationException("Bad sprite number: " + spriteNumber);
                int x = spriteNumber % _columns;
                int y = spriteNumber / _columns;

                return new Microsoft.Xna.Framework.Rectangle(x * Width, y * Height, Width, Height);
            }
        }


        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Ensure_spriteBatch
        /// </summary>
        //--------------------------------------------------------------------------------------
        void Ensure_spriteBatch()
        {
            if (!_inSpriteBatch)
            {
                _spriteBatch.Begin(transformMatrix: Matrix.CreateScale(new Vector3(_backBufferWidth, _backBufferWidth, 1)));
                _inSpriteBatch = true;
            }

        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// SelectFont - Select the Active font for text operations
        /// </summary>
        //--------------------------------------------------------------------------------------
        public void SelectFont(string fontName = null)
        {
            _selectedFont = _defaultFont;
            if (fontName != null) _selectedFont = _fontsByName[fontName];
            _selectedFontPixelSize = _selectedFont.MeasureString("A").Y;
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// DrawBox
        /// </summary>
        //--------------------------------------------------------------------------------------
        public void DrawBox(Vector2 offset, Vector2 size, Color color)
        {
            Vector2 scale = size / new Vector2(_utilityBlockTexture.Width, _utilityBlockTexture.Height);
            Ensure_spriteBatch();

            _spriteBatch.Draw(
                texture: _utilityBlockTexture,
                position: offset,
                sourceRectangle: null,
                color: color,
                rotation: 0,
                origin: Vector2.Zero,
                scale: scale,
                effects: SpriteEffects.None,
                layerDepth: 0);
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// DrawEllipse
        /// </summary>
        //--------------------------------------------------------------------------------------
        public void DrawEllipse(Vector2 offset, Vector2 size, Color color)
        {
            Vector2 scale = size / new Vector2(_circleTexture.Width, _circleTexture.Height);
            Ensure_spriteBatch();

            _spriteBatch.Draw(
              texture: _circleTexture,
              position: offset,
              sourceRectangle: null,
              color: color,
              rotation: 0,
              origin: Vector2.Zero,
              scale: scale,
              effects: SpriteEffects.None,
              layerDepth: 0);
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// BreakIntoWrappedLines
        /// </summary>
        //--------------------------------------------------------------------------------------
        string[] BreakIntoWrappedLines(string text, float fontSize, float wrapWidth)
        {
            var output = new List<string>();

            var spaceSize = MeasureText(" ", fontSize);
            var lines = text.Split('\n');
            var currentTextWidth = 0f;
            foreach (var line in lines)
            {
                var lineBuilder = new StringBuilder();
                currentTextWidth = 0;
                var parts = text.Split(' ');
                foreach (var part in parts)
                {
                    if (part.Length > 0)
                    {
                        var partSize = MeasureText(part, fontSize);
                        if (partSize.X + currentTextWidth > wrapWidth)
                        {
                            output.Add(lineBuilder.ToString());
                            lineBuilder.Clear();
                            currentTextWidth = 0;
                        }
                        if (lineBuilder.Length > 0) lineBuilder.Append(" ");
                        lineBuilder.Append(part);
                        currentTextWidth += partSize.X + spaceSize.X;
                    }
                }
                output.Add(lineBuilder.ToString());
            }
            return output.ToArray();
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// DrawText
        /// </summary>
        //--------------------------------------------------------------------------------------
        public void DrawText(string text, string fontName, float fontSize, Vector2 offset, Color color, float wrapWidth = 0)
        {
            text = FixText(text);
            float scale = fontSize / _selectedFontPixelSize;
            var cursor = offset;

            Action<string> drawTextSegment = (segment) =>
            {
                _spriteBatch.DrawString(
                    spriteFont: _selectedFont,
                    text: segment,
                    position: cursor,
                    color: color,
                    rotation: 0,
                    origin: Vector2.Zero,
                    scale: scale,
                    effects: SpriteEffects.None,
                    layerDepth: 0);
            };

            Ensure_spriteBatch();
            if (wrapWidth == 0)
            {
                drawTextSegment(text);
            }
            else
            {
                foreach (var line in BreakIntoWrappedLines(text, fontSize, wrapWidth))
                {
                    drawTextSegment(line);
                    cursor.Y += fontSize;
                }
            }
        }

        Dictionary<string, string> _fixedStrings = new Dictionary<string, string>();

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// FixText - convert a string to have only characters that can be printed
        /// </summary>
        //--------------------------------------------------------------------------------------
        string FixText(string text)
        {
            if (_fixedStrings.ContainsKey(text)) return _fixedStrings[text];

            var output = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                output.Append(_selectedFont.Characters.Contains(text[i]) ?
                              text[i] : ' ');
            }
            _fixedStrings[text] = output.ToString();
            return output.ToString();
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// MeasureText
        /// </summary>
        //--------------------------------------------------------------------------------------
        public Vector2 MeasureText(string text, string fontName, float fontSize, float wrapWidth = 0)
        {
            SelectFont(fontName);
            return MeasureText(text, fontSize, wrapWidth);
        }

        public Vector2 MeasureText(string text, float fontSize, float wrapWidth = 0)
        {
            text = FixText(text);
            if (text == null || text == "")
            {
                var size = _selectedFont.MeasureString("I") * fontSize / _selectedFontPixelSize;
                size.X = 0;
                return size;
            }

            if (wrapWidth > 0)
            {
                var output = Vector2.Zero;
                foreach (var line in BreakIntoWrappedLines(text, fontSize, wrapWidth))
                {
                    var segmentSize = MeasureText(line, fontSize);
                    output.Y += segmentSize.Y;
                    if (segmentSize.X > output.X) output.X = segmentSize.X;
                }
                return output;
            }
            else return _selectedFont.MeasureString(text) * fontSize / _selectedFontPixelSize;
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// DrawLine
        /// </summary>
        //--------------------------------------------------------------------------------------
        public void DrawLine(Vector2 start, Vector2 end, float lineWidth, Color color)
        {
            Ensure_spriteBatch();
            Vector2 rotationOrigin = new Vector2(0, _utilityBlockTexture.Height / 2);

            float length = (start - end).Length();
            Vector2 scale = new Vector2(length / _utilityBlockTexture.Width, lineWidth / _utilityBlockTexture.Height);

            float rotationAngle = (float)Math.Atan2(end.Y - start.Y, end.X - start.X);

            _spriteBatch.Draw(
                texture: _utilityBlockTexture,
                position: start,
                sourceRectangle: null,
                color: color,
                rotation: rotationAngle,
                origin: rotationOrigin,
                scale: scale,
                effects: SpriteEffects.None,
                layerDepth: 0);

        }


        //--------------------------------------------------------------------------------------
        /// <summary>
        /// PlaySound
        /// </summary>
        //--------------------------------------------------------------------------------------
        public void PlaySound(string soundName, double volumeAdjust = 1.0)
        {
            if (SoundVolume == 0) return;
            var effect = _soundsByName[soundName];
            if ((DateTime.Now - effect.LastPlayTime).TotalMilliseconds > 40)
            {
                var volume = effect.PreferredVolume;
                volume *= SoundVolume;
                if (volume > 1.0) volume = 1.0;
                if (volume < 0) volume = 0;

                effect.Effect.Play((float)volume, 0, 0);
                effect.LastPlayTime = DateTime.Now;
            }
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// DrawGlyph
        /// </summary>
        //--------------------------------------------------------------------------------------
        public void DrawGlyph(string glyphName, Vector2 offset, Vector2 size, Color color)
        {
            DrawGlyph(glyphName, offset, size, color, 0, Vector2.Zero);
        }

        public void DrawGlyph(string glyphName, Vector2 offset, Vector2 size, Color color, float rotation, Vector2 origin)
        {
            Texture2D texture = _texturesByName[glyphName];
            DrawGlyph(texture, offset, size, color, rotation, origin);
        }

        public void DrawGlyph(Texture2D texture, Vector2 offset, Vector2 size, Color color, float rotation, Vector2 origin)
        {
            Vector2 scale = size / new Vector2(texture.Width, texture.Height);

            Ensure_spriteBatch();

            _spriteBatch.Draw(
                texture: texture,
                position: offset,
                sourceRectangle: null,
                color: color,
                rotation: rotation,
                origin: origin,
                scale: scale,
                effects: SpriteEffects.None,
                layerDepth: 0);
        }


        //--------------------------------------------------------------------------------------
        /// <summary>
        /// DrawSprite
        /// </summary>
        //--------------------------------------------------------------------------------------
        public void DrawSprite(string spriteName, int spriteNumber, Vector2 offset, Vector2 size, Color color)
        {
            var sprite = _spritesByName[spriteName];
            Texture2D texture = sprite.Texture;
            var sourceRect = sprite.GetRectangle(spriteNumber);
            Vector2 scale = size / new Vector2(sprite.Width, sprite.Height);

            Ensure_spriteBatch();

            _spriteBatch.Draw(
                  texture: texture,
                  position: offset,
                  sourceRectangle: sourceRect,
                  color: color,
                  rotation: 0,
                  origin: Vector2.Zero,
                  scale: scale,
                  effects: SpriteEffects.None,
                  layerDepth: 0);
        }
    }
}
