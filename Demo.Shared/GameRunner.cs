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
    /// Your game control starts here
    /// </summary>
    public abstract partial class GameRunner: IDisposable
    {
        GameController _controller;
        public Color GlobalBackgroundColor { get { return Color.DarkGray; } }
        public string TimeText {  get { return "Current Time: " + DateTime.Now.ToLongTimeString(); } }
        public Vector2 ScreenSize { get { return _controller.ScreenSize; } }

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
            _controller.OnLoaded += () =>
            {
                _controller.LoadGlyph("Images/Mountains");
                _controller.LoadGlyph("Images/Trees");
                _controller.LoadGlyph("Images/Ground");
                _controller.LoadSprite("Images/Bunny", 100, 100);
                _controller.LoadSprite("Images/Monster", 100, 100);
                _controller.LoadSounds("Sounds/Cowbell", "Sounds/Jump", "Sounds/Thump");
                _controller.SetScreen("MainScreen");
            };

            _controller.OnUpdate += (gameTime) =>
            {
                if (_currentGame != null) _currentGame.Update(gameTime);
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
