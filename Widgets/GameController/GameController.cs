#region Using Statements

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

#endregion

namespace MonoVarmint.Widgets
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public partial class GameController : Game, IVarmintWidgetInjector
    {
        public Color GlobalBackgroundColor { get { return Color.DarkGray; } }
        public string DynamicText { get { return "Current Time: " + DateTime.Now.ToLongTimeString(); } }
        public Vector2 ScreenSize { get { return new Vector2(_backBufferWidth, _backBufferHeight); } }

        public event Action OnLoaded;

        Dictionary<string, VarmintWidget> _screensByName = new Dictionary<string, VarmintWidget>();

      
        //-----------------------------------------------------------------------------------------------
        // ctor 
        //-----------------------------------------------------------------------------------------------
        public GameController()
        {

            _graphics = new GraphicsDeviceManager(this);
            _graphics.IsFullScreen = true;
            
        }

        //-----------------------------------------------------------------------------------------------
        // Force a windowed, non-native resolution 
        //-----------------------------------------------------------------------------------------------
        public GameController(int width, int height) : this()
        {
            _graphics.IsFullScreen = false;
            _graphics.PreferredBackBufferWidth = width;
            _graphics.PreferredBackBufferHeight = height;

#if WINDOWS
            this.IsMouseVisible = true;
#endif
        }

        //-----------------------------------------------------------------------------------------------
        // 
        //-----------------------------------------------------------------------------------------------
        protected override void LoadContent()
        {
            Content = new EmbeddedContentManager(_graphics.GraphicsDevice);
            Content.RootDirectory = "Content";

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

            _utilityBlockTexture = Content.Load<Texture2D>("_utility_block");
            _circleTexture = Content.Load<Texture2D>("_utility_circle");
            _defaultFont = Content.Load<SpriteFont>("_utility_SegoeUI");

            _fontsByName.Add("_utility_SegoeUI", _defaultFont);
            SelectFont();

            // Widgets
            _screensByName = VarmintWidget.LoadLayout(this, this);

            _visualTree = _screensByName["_default_screen_"];
            OnLoaded?.Invoke();
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
            if (_inSpriteBatch)
            {
                _spriteBatch.End();
                _inSpriteBatch = false;
            }
            GraphicsDevice.SetRenderTarget(null);

            // Now we render the back buffer to the screen
            GraphicsDevice.Clear(GlobalBackgroundColor);
            _spriteBatch.Begin();
            _spriteBatch.Draw(_backBuffer, new Vector2(_backBufferXOffset, 0));
            _spriteBatch.End();
        }

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
        // SetScreen - call this to change the visual tree to a screen you have defined in
        // a .vwml file
        //--------------------------------------------------------------------------------------
        public void SetScreen(string screenName)
        {
            _visualTree = _screensByName[screenName];
        }
    }

}
