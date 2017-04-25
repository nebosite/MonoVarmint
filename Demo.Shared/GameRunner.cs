#region Using Statements
using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoVarmint.Widgets;
using System.Text;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using System.Diagnostics;
using System.IO;

#endregion

namespace Demo.Shared
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class GameRunner : Game, IMediaRenderer, IVarmintWidgetInjector
    {
        public Color GlobalBackgroundColor { get { return Color.DarkGray; } }
        public string DynamicText {  get { return "Current Time: " + DateTime.Now.ToLongTimeString(); } }
        public Vector2 ScreenSize { get { return new Vector2(_backBufferWidth, _backBufferHeight); } }

        GraphicsDeviceManager _graphics;
        SpriteBatch _spriteBatch;

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

        //-----------------------------------------------------------------------------------------------
        // ctor 
        //-----------------------------------------------------------------------------------------------
        public GameRunner()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

#if WINDOWS
            _graphics.IsFullScreen = false;
            this.IsMouseVisible = true;
            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 900;
#else
            graphics.IsFullScreen = true;
#endif

        }


        //-----------------------------------------------------------------------------------------------
        // 
        //-----------------------------------------------------------------------------------------------
        protected override void Initialize()
        {
            base.Initialize();
        }

        class MyServiceManager : IServiceProvider, IGraphicsDeviceService
        {
            public GraphicsDevice GraphicsDevice { get; private set; }

            public event EventHandler<EventArgs> DeviceCreated;
            public event EventHandler<EventArgs> DeviceDisposing;
            public event EventHandler<EventArgs> DeviceReset;
            public event EventHandler<EventArgs> DeviceResetting;

            public MyServiceManager(GraphicsDevice graphicsDevice)
            {
                GraphicsDevice = graphicsDevice;
            }
            public object GetService(Type serviceType)
            {
                Debug.WriteLine("GetService called with: " + serviceType.Name);
                return this;
            }
        }
        class EmbeddedContentManager: ContentManager
        {
            Assembly _localAssembly;

            public EmbeddedContentManager(GraphicsDevice graphicsDevice ): base(new MyServiceManager(graphicsDevice))
            {
                _localAssembly = Assembly.GetExecutingAssembly();
            }

            protected override Stream OpenStream(string assetName)
            {
                var resourceName = "WindowsDemo.EmbeddedContent." + assetName + ".xnb";
                var output = _localAssembly.GetManifestResourceStream(resourceName);
                if(output == null)
                {
                    throw new ApplicationException("Could not find embedded content: " + resourceName);
                }
                return _localAssembly.GetManifestResourceStream(resourceName);
            }


        }

        //-----------------------------------------------------------------------------------------------
        // 
        //-----------------------------------------------------------------------------------------------
        protected override void LoadContent()
        {
            // Set up a back buffer to render to
            _backBufferWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
            _backBufferHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;
            if ((float)_backBufferHeight / _backBufferWidth < 1.6)
            {
                _backBufferWidth = (int)(_backBufferHeight / 1.6);
                _backBufferXOffset = (GraphicsDevice.PresentationParameters.BackBufferWidth - _backBufferWidth) / 2;
            }

            _backBuffer = new RenderTarget2D(
                _graphics.GraphicsDevice,
                _backBufferWidth,
                _backBufferHeight,
                false,
                SurfaceFormat.Color,
                DepthFormat.None);

            var scaleFactor = _backBufferWidth / 1000.0f;
            _scaleToNativeResolution = Matrix.CreateScale(new Vector3(scaleFactor, scaleFactor, 1));

            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Embeded resources
            var embeddedContent = new EmbeddedContentManager(_graphics.GraphicsDevice);
            _utilityBlockTexture = embeddedContent.Load<Texture2D>("utilityblock");
            _circleTexture = embeddedContent.Load<Texture2D>("circle");
            _defaultFont = embeddedContent.Load<SpriteFont>("SegoeUI");
            SelectFont();

            // Widgets
            var layout = VarmintWidget.LoadLayout(this);
            layout["MainScreen"].BindingContext = this;
            layout["MainScreen"].Init();

            _visualTree = layout["MainScreen"];
        }

        //-----------------------------------------------------------------------------------------------
        // 
        //-----------------------------------------------------------------------------------------------
        protected override void Update(GameTime gameTime)
        {
            // For Mobile devices, this logic will close the Game when the Back button is pressed
            // Exit() is obsolete on iOS
#if !__IOS__ && !__TVOS__
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }
#endif
            // TODO: Add your update logic here			
            base.Update(gameTime);
        }

        //-----------------------------------------------------------------------------------------------
        // 
        //-----------------------------------------------------------------------------------------------
        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            // First, We render the game to a backbuffer to be resolution independent
            GraphicsDevice.SetRenderTarget(_backBuffer);
            GraphicsDevice.Clear(Color.Blue);
            _visualTree.RenderMe(gameTime);
            if (_in_spriteBatch)
            {
                _spriteBatch.End();
                _in_spriteBatch = false;
            }
            GraphicsDevice.SetRenderTarget(null);

            // Now we render the back buffer to the screen
            GraphicsDevice.Clear(GlobalBackgroundColor);
            _spriteBatch.Begin();
            _spriteBatch.Draw(_backBuffer, new Vector2(_backBufferXOffset, 0));
            _spriteBatch.End();
        }

        bool _in_spriteBatch;


        //--------------------------------------------------------------------------------------
        //
        //--------------------------------------------------------------------------------------
        public object GetInjectedValue(VarmintWidgetInjectAttribute attribute, PropertyInfo property)
        {
            if (typeof(IMediaRenderer).IsAssignableFrom(property.PropertyType))
            {
                return this; 
            }
            throw new ApplicationException("Don't know how to inject a " + property.PropertyType);
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Ensure_spriteBatch
        /// </summary>
        //--------------------------------------------------------------------------------------
        void Ensure_spriteBatch()
        {
            if (!_in_spriteBatch)
            {
                _spriteBatch.Begin(transformMatrix: Matrix.CreateScale(new Vector3(_backBufferWidth, _backBufferWidth, 1)));
                _in_spriteBatch = true;
            }

        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// SelectFont - Select the Active font for text operations
        /// </summary>
        //--------------------------------------------------------------------------------------
        public void SelectFont(SpriteFont font = null)
        {
            if (font == null) font = _defaultFont;
            _selectedFont = font;
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

            var spaceSize = MeasureText(fontSize, " ");
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
                        var partSize = MeasureText(fontSize, part);
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
        public void DrawText(Vector2 offset, float fontSize, string text, Color color, float wrapWidth = 0)
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
        public Vector2 MeasureText(float fontSize, string text, float wrapWidth = 0)
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
                    var segmentSize = MeasureText(fontSize, line);
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
        public void PlaySound(int soundId, double volumeAdjust = 1.0)
        {
            //if (_settings.SoundVolume == VolumeSetting.Off) return;
            //var effect = _contentMap.Sounds[soundId];
            //if ((DateTime.Now - effect.LastPlayTime).TotalMilliseconds > 40)
            //{
            //    var volume = effect.PreferredVolume;
            //    if (_settings.SoundVolume == VolumeSetting.Low) volume *= .1;
            //    volume *= _masterVolume;
            //    if (volume > 1.0) volume = 1.0;
            //    if (volume < 0) volume = 0;

            //    effect.Effect.Play((float)volume, 0, 0);
            //    effect.LastPlayTime = DateTime.Now;
            //}
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// DrawGlyph
        /// </summary>
        //--------------------------------------------------------------------------------------
        public void DrawGlyph(int glyphId, Vector2 offset, Vector2 size, Color color)
        {
            DrawGlyph(glyphId, offset, size, color, 0, Vector2.Zero);
        }

        public void DrawGlyph(string glyphName, Vector2 offset, Vector2 size, Color color)
        {
            //DrawGlyph(_contentMap.GlyphsByName[glyphName], offset, size, color, 0, Vector2.Zero);
        }

        public void DrawGlyph(int glyphId, Vector2 offset, Vector2 size, Color color, float rotation, Vector2 origin)
        {
            //Texture2D texture = _contentMap.Glyphs[glyphId];
            //DrawGlyph(texture, offset, size, color, rotation, origin);
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
        public void DrawSprite(int spriteId, int spriteNumber, Vector2 offset, Vector2 size, Color color)
        {
            //var sprite = _contentMap.Sprites[spriteId];
            //Texture2D texture = sprite.Texture;
            //var sourceRect = sprite.GetRectangle(spriteNumber);
            //Vector2 scale = size / new Vector2(sprite.Width, sprite.Height);

            //Ensure_spriteBatch();

            //_spriteBatch.Draw(
            //      texture: texture,
            //      position: offset,
            //      sourceRectangle: sourceRect,
            //      color: color,
            //      rotation: 0,
            //      origin: Vector2.Zero,
            //      scale: scale,
            //      effects: SpriteEffects.None,
            //      layerDepth: 0);

        }
    }
}
