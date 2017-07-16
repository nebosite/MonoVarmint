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
        public int Score { get { return _currentGame == null ? 0 : _currentGame.Score; } }

        GameState _currentGame;

        //-----------------------------------------------------------------------------------------------
        // StartGame - This creates a new game state and the UI around it.  The code inside this 
        // method could be its own class if you want.  It's a matter of style/preference.  
        //-----------------------------------------------------------------------------------------------
        public void StartGame()
        {
            _controller.SetScreen(_controller.GetScreen("GameScreen", this));
            var holder = _controller.GetVisibleWidgetByName("GameContainer");
            var gameWindow = new VarmintWidgetGrid();
            gameWindow.Renderer = _controller;
            gameWindow.Size = holder.Size;
            holder.AddChild(gameWindow);

            _currentGame = new GameState();

            // Hook up events on the game for playing sounds, showing UI, etc.
            _currentGame.OnpassBunny += () => {  _controller.PlaySoundEffect("Sounds/Cowbell"); };
            _currentGame.OnFInishJump += () => { _controller.PlaySoundEffect("Sounds/Thump"); };
            _currentGame.OnStartJump += () => { _controller.PlaySoundEffect("Sounds/Jump"); };

            // Hook up the tap event on the game window for jumping
            gameWindow.OnTap += (widget, location) =>
            {
                _currentGame.Jump();
                return VarmintWidget.EventHandledState.Handled;
            };

            // Set up a render method to visualize the objects in the game.  This 
            // is essentially the code to draw a frame
            gameWindow.SetCustomRender((GameTime gameTime, VarmintWidget widget) =>
            {
                // draw the sky
                _controller.DrawBox(widget.AbsoluteOffset, widget.Size, Color.SkyBlue);

                // Helper to easily draw a scolling object
                Action<string, float> drawScrollingThing = (glyphName, relativeSize) =>
                {
                    float offset = ((_currentGame.MonsterPosition.X / relativeSize) % 1.0f) * widget.Size.X;
                    _controller.DrawGlyph(glyphName, widget.AbsoluteOffset - new Vector2(offset, 0), widget.Size, Color.White);
                    _controller.DrawGlyph(glyphName, widget.AbsoluteOffset + new Vector2(widget.Size.X - offset, 0), widget.Size, Color.White);
                };

                // draw the far scrolling background
                drawScrollingThing("Images/Mountains", 30);

                // draw the near scrolling background
                drawScrollingThing("Images/Trees", 3);

                // draw the bunny
                var monsterDelta = .1f;
                var bunnyDelta = _currentGame.BunnyPosition.X + monsterDelta - _currentGame.MonsterPosition.X;
                var bunnyDrawPosition = widget.AbsoluteOffset;
                bunnyDrawPosition.X += bunnyDelta * widget.Size.X;
                bunnyDrawPosition.Y = widget.Size.Y * .82f;
                 _controller.DrawSprite("Images/Bunny", (_controller.CurrentFrameNumber / 10) % 2, bunnyDrawPosition, new Vector2(.1f), Color.White);

                // Draw the monster
                var monsterDrawPosition = widget.AbsoluteOffset;
                monsterDrawPosition.X += monsterDelta * widget.Size.X;
                monsterDrawPosition.Y = widget.Size.Y * .65f + _currentGame.MonsterPosition.Y * widget.Size.Y;
                _controller.DrawSprite("Images/Monster", (_controller.CurrentFrameNumber / 10) % 4, monsterDrawPosition, new Vector2(.2f), Color.White);

                // Draw the ground
                drawScrollingThing("Images/Ground", 1);

            });
        }

        //-----------------------------------------------------------------------------------------------
        // GameQuitButtonOnTap - Clean up the game and return to main screen
        //-----------------------------------------------------------------------------------------------
        public VarmintWidget.EventHandledState GameQuitButtonOnTap(VarmintWidget tappedObject, Vector2 tapPosition)
        {
            _controller.GetVisibleWidgetByName("GameContainer").ClearChildren();
            _controller.SetScreen(_controller.GetScreen("MainScreen", this));
            _currentGame = null;

            return VarmintWidget.EventHandledState.Handled;
        }
    }
}
