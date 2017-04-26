﻿#region Using Statements

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
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
        public Vector2 ScreenSize { get { return new Vector2(1.0f, (float)_backBufferHeight/_backBufferWidth); } }
        public int CurrentFrameNumber { get; set; }
        public bool PauseInput { get; set; }

        // events
        public event Action OnLoaded;
        public event Action<GameTime> OnUpdate;
        public event Action<Keys, bool, char> OnTypedCharacter;


        Dictionary<string, VarmintWidget> _screensByName = new Dictionary<string, VarmintWidget>();
        object _bindingContext;
        
      
        //-----------------------------------------------------------------------------------------------
        // ctor 
        //-----------------------------------------------------------------------------------------------
        public GameController(object bindingContext)
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.IsFullScreen = true;
            _bindingContext = bindingContext;
        }

        //-----------------------------------------------------------------------------------------------
        // Force a windowed, non-native resolution 
        //-----------------------------------------------------------------------------------------------
        public GameController(object bindingContext, int width, int height) : this(bindingContext)
        {
            _graphics.IsFullScreen = false;
            _graphics.PreferredBackBufferWidth = width;
            _graphics.PreferredBackBufferHeight = height;

#if WINDOWS
            this.IsMouseVisible = true;
#endif
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Initialize
        /// </summary>
        //--------------------------------------------------------------------------------------
        protected override void Initialize()
        {
            // Monotouch doesn't handle flick and drag gestures well, so we are going
            // to ignore them and just process touches
            //TouchPanel.EnabledGestures = GestureType.DoubleTap
            //    | GestureType.Tap
            //    | GestureType.FreeDrag
            //    | GestureType.DragComplete
            //    | GestureType.Flick
            //    | GestureType.Hold;

            TouchPanel.EnableMouseTouchPoint = true;
            TouchPanel.EnableMouseGestures = true;

            base.Initialize();
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
            _screensByName = VarmintWidget.LoadLayout(this, _bindingContext);

            _visualTree = _screensByName["_default_screen_"];
            OnLoaded?.Invoke();
        }

#if WINDOWS
        Dictionary<Keys, bool> _pressedKeys = new Dictionary<Keys, bool>();
#endif

        //-----------------------------------------------------------------------------------------------
        // 
        //-----------------------------------------------------------------------------------------------
        protected override void Update(GameTime gameTime)
        {
            CurrentFrameNumber++;

            // For Mobile devices, this logic will close the Game when the Back button is pressed
            // Exit() is obsolete on iOS
#if !__IOS__ && !__TVOS__
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }
#endif

#if WINDOWS
            // In windows, we want to use keystrokes to mimic mobile buttons
            KeyboardState state = Keyboard.GetState();
            foreach (var key in state.GetPressedKeys())
            {
                _pressedKeys[key] = true;
            }

            bool shifted = state.IsKeyDown(Keys.LeftShift) || state.IsKeyDown(Keys.RightShift);

            var wasPressed = new List<Keys>(_pressedKeys.Keys);
            foreach (var key in wasPressed)
            {
                if (state.IsKeyUp(key))
                {
                    _pressedKeys.Remove(key);

                    var c = CharFromKey(key, shifted);
                    if (c != null) HandleNativeInputCharacter(c.Value);
                    OnTypedCharacter?.Invoke(key, shifted, c ?? '\0');
                }
            }

#endif
            HandleUserInput(gameTime);

            OnUpdate?.Invoke(gameTime);
            base.Update(gameTime);
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// CharFromKey - Convert windows key character to an actual character
        /// </summary>
        //--------------------------------------------------------------------------------------
        public static char? CharFromKey(Keys key, bool shifted)
        {
            string output = null;
            if (key >= Keys.A && key <= Keys.Z) output = key.ToString();
            else if (key >= Keys.NumPad0 && key <= Keys.NumPad9) output = ((int)(key - Keys.NumPad0)).ToString();
            else if (key >= Keys.D0 && key <= Keys.D9)
            {
                string num = ((int)(key - Keys.D0)).ToString();
                if (shifted)
                {
                    switch (num)
                    {
                        case "1": num = "!"; break;
                        case "2": num = "@"; break;
                        case "3": num = "#"; break;
                        case "4": num = "$"; break;
                        case "5": num = "%"; break;
                        case "6": num = "^"; break;
                        case "7": num = "&"; break;
                        case "8": num = "*"; break;
                        case "9": num = "("; break;
                        case "0": num = ")"; break;
                        default: break;
                    }
                }
                output = num;
            }
            else if (key == Keys.OemPeriod) output = ".";
            else if (key == Keys.OemTilde) output = "'";
            else if (key == Keys.Space) output = " ";
            else if (key == Keys.OemMinus) output = "-";
            else if (key == Keys.OemPlus) output = "+";
            else if (key == Keys.OemQuestion && shifted) output = "?";
            else if (key == Keys.Back) output = "\b";

            if (!shifted && output != null) output = output.ToLower();

            if (output == null) return null;
            return output[0];
        }


        //--------------------------------------------------------------------------------------
        /// <summary>
        /// HandleUserInput
        /// </summary>
        //--------------------------------------------------------------------------------------
        private void HandleUserInput(GameTime gameTime)
        {
            // Varmint Widgets does it's own gesture handling because the built-in gesture handling
            // for mono does a bad job with flicking and dragging
            foreach (var touch in TouchPanel.GetState())
            {
                var hitSpot = touch.Position / _backBufferWidth;
                hitSpot.X -= (float)_backBufferXOffset / _backBufferWidth;
                var adjustedTouch = new TouchLocation(touch.Id, touch.State, hitSpot);
                if (PauseInput) continue;
                _visualTree.HandleTouch(adjustedTouch);
            }
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// HandleNativeInputCharacter
        /// </summary>
        //--------------------------------------------------------------------------------------
        void HandleNativeInputCharacter(char c)
        {
            if (PauseInput) return;
            _visualTree.HandleInputCharacter(c);
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
