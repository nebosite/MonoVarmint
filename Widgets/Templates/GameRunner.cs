﻿#region Using Statements
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

namespace ChangeThisToYourNameSpace
{
    //-----------------------------------------------------------------------------------------------
    // GameRunner - THis is your basic game controller class.  There is little need to subclass
    // this for simple games.  You can control several screens and functions from the this 
    // class. See the WindowsDemo project for a simple example of how to show and operate multiple 
    // screens in a game context. 
    //-----------------------------------------------------------------------------------------------
    public abstract partial class GameRunner: IDisposable
    {
        GameController _controller;
        public Vector2 ScreenSize { get { return _controller.ScreenSize; } }

        //-----------------------------------------------------------------------------------------------
        // ctor 
        //-----------------------------------------------------------------------------------------------
        public GameRunner()
        {
#if WINDOWS
            // For windows, we will show a windowed version that looks like a phone app
            _controller = new GameController(this, 500, 900);
#else
            _controller = new GameController(this);
#endif

            // TODO: uncomment this line and put in the name of the screen you
            // want to start with.
            //_controller.OnLoaded += () => _controller.SetScreen("MyStartScreenName");
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
