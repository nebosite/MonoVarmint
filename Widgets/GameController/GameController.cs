#region Using Statements

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

#endregion

namespace MonoVarmint.Widgets
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public partial class GameController : Game, IVarmintWidgetInjector
    {
        public Color GlobalBackgroundColor => Color.DarkGray;
        public Vector2 ScreenSize => new Vector2(1.0f, (float)_backBufferHeight/_backBufferWidth);
        public int CurrentFrameNumber { get; set; }
        public bool PauseInput { get; set; }
        public double Fps { get; private set; }

        public bool ShowFps { get; set; }

        // events
        public event Action OnLoaded;
        public event Action<GameTime> OnUpdate;
        public event Action<Keys, bool, char> OnTypedCharacter;
        public event Action OnUserBackButtonPress;

        private readonly object _bindingContext;
        int _frameCount;
        DateTime _lastFrameMeasureTime = DateTime.Now;
        VarmintWidgetSpace _widgetSpace;



        //-----------------------------------------------------------------------------------------------
        // ctor 
        //-----------------------------------------------------------------------------------------------
        public GameController(object bindingContext)
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreparingDeviceSettings += (sender, settings)=>
            {
                settings.GraphicsDeviceInformation.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;
            };
            _graphics.IsFullScreen = true;
            _bindingContext = bindingContext;
        }

        //-----------------------------------------------------------------------------------------------
        // GetScreen - return a screen object by name 
        //-----------------------------------------------------------------------------------------------
        internal VarmintWidget GetScreen(string screenName, object bindingContext)
        {
            return _widgetSpace.GetScreen(screenName, bindingContext);
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
#if WINDOWS
            TouchPanel.EnableMouseTouchPoint = true;
            TouchPanel.EnableMouseGestures = true;
#endif
            base.Initialize();
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

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                OnUserBackButtonPress?.Invoke();
            }

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
            UpdateAudio(gameTime);
            
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
            else if (key >= Keys.NumPad0 && key <= Keys.NumPad9) output = (key - Keys.NumPad0).ToString();
            else if (key >= Keys.D0 && key <= Keys.D9)
            {
                string num = (key - Keys.D0).ToString();
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

            if (!shifted) output = output?.ToLower();

            return output?[0];
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
                var hitSpot = (touch.Position / _backBufferWidth) + DrawOffset;
                hitSpot.X -= (float)_backBufferXOffset / _backBufferWidth;
                var adjustedTouch = new TouchLocation(touch.Id, touch.State, hitSpot);
                if (PauseInput) continue;
                _visualTree.ReportTouch(adjustedTouch, gameTime);
            }

            _visualTree.ResolveGestures(gameTime);
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
            UpdateFrame();
            _visualTree.AdvanceAnimations(gameTime);
            base.Draw(gameTime);

            //Debug.WriteLine("AAA---------------------- BEGIN ------------------------");
            //Debug.WriteLine("AAA_spriteBatch.Begin();");
            _spriteBatch.Begin();
            GraphicsDevice.Clear(GlobalBackgroundColor);
            BeginClipping(DrawOffset, ScreenSize);
            _visualTree.Prepare(_widgetSpace.StyleLibrary);
            _visualTree.RenderMe(gameTime);

            if (ShowFps)
            {
                DrawText("Fps: " + Fps.ToString(".0"), null, .05f, DrawOffset + new Vector2(0.01f, 0.01f), Color.Black);
            }

            EndClipping(0, Vector2.Zero, new Vector2(1), false, false);
            if (_drawBuffers.Count > 0)
            {
                throw new ApplicationException("There was an unmatch BeginClipping call.");
            }

            //Debug.WriteLine("AAA_spriteBatch.End();");
            _spriteBatch.End();
            //Debug.WriteLine("AAA---------------------- END ------------------------");
        }

        //-----------------------------------------------------------------------------------------------
        // 
        //-----------------------------------------------------------------------------------------------
        private void UpdateFrame()
        {
            _frameCount++;
            if (_frameCount == 30)
            {
                var span = DateTime.Now - _lastFrameMeasureTime;
                _lastFrameMeasureTime = DateTime.Now;
                _frameCount = 0;
                Fps = 30 / span.TotalSeconds;
            }
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
        // 
        //--------------------------------------------------------------------------------------
        public void AddVwmlContent(string replaceName, Stream vwmlStream, object bindingContext = null)
        {
            _widgetSpace.AddContent(replaceName, vwmlStream, bindingContext);
        }

        //--------------------------------------------------------------------------------------
        // 
        //--------------------------------------------------------------------------------------
        public void SetScreen(VarmintWidget screen)
        {
            _visualTree = screen;
            screen.Prepare(_widgetSpace.StyleLibrary);
        }

        //--------------------------------------------------------------------------------------
        // SetScreen - call this to change the visual tree to a screen you have defined in
        // a .vwml file
        //--------------------------------------------------------------------------------------
        public void SetScreen(string screenName, object bindingContext)
        {
            _visualTree = _widgetSpace.GetScreen(screenName, bindingContext);
        }

        //--------------------------------------------------------------------------------------
        // GetService 
        //--------------------------------------------------------------------------------------
        public object GetService(Type serviceType)
        {
            return Services.GetService(serviceType);
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// OnActivated
        /// </summary>
        //--------------------------------------------------------------------------------------
        protected override void OnActivated(object sender, EventArgs args)
        {
            Debug.WriteLine("ACTIVATED");
            base.OnActivated(sender, args);
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// OnDeactivated
        /// </summary>
        //--------------------------------------------------------------------------------------
        protected override void OnDeactivated(object sender, EventArgs args)
        {
            Debug.WriteLine("DE-ACTIVATED");
            base.OnDeactivated(sender, args);
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// OnExiting
        /// </summary>
        //--------------------------------------------------------------------------------------
        protected override void OnExiting(object sender, EventArgs args)
        {
            Debug.WriteLine("EXITING");
            base.OnExiting(sender, args);
        }

    }

}
