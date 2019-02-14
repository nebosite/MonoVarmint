#region Using Statements
using System;
using Microsoft.Xna.Framework;
using MonoVarmint.Widgets;

#endregion

namespace Demo.Shared
{
    /// <summary>
    /// Your game control starts here
    /// </summary>
    public abstract partial class GameRunner: IDisposable
    {
        protected GameController _controller;
        public Color GlobalBackgroundColor { get { return Color.DarkGray; } }
        public string TimeText {  get { return "Current Time: " + DateTime.Now.ToLongTimeString(); } }
        public Vector2 ScreenSize { get { return _controller.ScreenSize; } }
        public float WowRotate { get; set; }
        public float MySliderValue { get; set; } = 0.5f;
        public string MyAdjustedSliderValue { get { return (MySliderValue * 100).ToString(".0"); } }

        //-----------------------------------------------------------------------------------------------
        // NATIVE METHODS - These methods are called when an action occurs that needs to be handled
        //                  natively.  Override these in the platform versein of GameRunner
        //-----------------------------------------------------------------------------------------------
        public abstract void NativeHandleUserDeactivate();

        //-----------------------------------------------------------------------------------------------
        // ctor 
        //-----------------------------------------------------------------------------------------------
        public GameRunner()
        {
#if WINDOWS
            _controller = new GameController(this, 500, 900);
#else
            _controller = new GameController(this);
#endif
            _controller.OnUserBackButtonPress += NativeHandleUserDeactivate;

            _controller.OnGameLoaded += () =>
            {
                _controller.LoadGlyph("Images/Mountains");
                _controller.LoadGlyph("Images/Trees");
                _controller.LoadGlyph("Images/Ground");
                _controller.LoadSprite("Images/Bunny", 100, 100);
                _controller.LoadSprite("Images/Monster", 100, 100);
                _controller.LoadSoundEffects("Sounds/Cowbell", "Sounds/Jump", "Sounds/Thump");
                _controller.SetScreen(_controller.GetScreen("MainScreen", this));
            };

            _controller.OnGameUpdate += (gameTime) =>
            {
                WowRotate += 1;
                _currentGame?.Update(gameTime);
            };

        }

        //-----------------------------------------------------------------------------------------------
        // PlayButtonOnTap - shift screen to the game screen
        //-----------------------------------------------------------------------------------------------
        public VarmintWidget.EventHandledState PlayButtonOnTap(VarmintWidget tappedObject, Vector2 tapPosition)
        {
            StartGame();
            return VarmintWidget.EventHandledState.Handled;
        }

        //-----------------------------------------------------------------------------------------------
        // Start the game 
        //-----------------------------------------------------------------------------------------------
        public void Run()
        {
            _controller.Run();
        }

        //-----------------------------------------------------------------------------------------------
        // Dispose
        //-----------------------------------------------------------------------------------------------
        public void Dispose()
        {
            _controller.Dispose();
        }
    }
}
