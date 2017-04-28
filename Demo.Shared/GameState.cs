using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Demo.Shared
{
    //-----------------------------------------------------------------------------------------------
    // GameLogic - To keep the code clean and decoupled, the game logic goes into it's own class.
    //             If anything needs to be rendered, that is communicated through events.
    //-----------------------------------------------------------------------------------------------
    public class GameState
    {
        private Vector2 _monsterPosition;
        public Vector2 MonsterPosition { get { return _monsterPosition; } }

        public int Score { get; private set; }

        private Vector2 _bunnyPosition;
        public Vector2 BunnyPosition { get { return _bunnyPosition; } }

        //-----------------------------------------------------------------------------------------------
        // Events are signals to the UI that the state has changed in some important.  The UI can
        // choose to respond with graphics and/or sounds to signal the user.  These events are much 
        // easier to test than if we had decided to allow the GameState to controll the UI
        //-----------------------------------------------------------------------------------------------
        public event Action OnpassBunny;
        public event Action OnFInishJump;
        public event Action OnStartJump;

        Vector2 _monsterVelocity = new Vector2(0.01f, 0);
        bool _isJumping = false;

        //-----------------------------------------------------------------------------------------------
        // ctor
        //-----------------------------------------------------------------------------------------------
        public GameState()
        {
            _monsterPosition = Vector2.Zero;
            _bunnyPosition = new Vector2(-1000, 0);
            Score = 0;
        }

        //-----------------------------------------------------------------------------------------------
        // Update - called on every frame to update the internal game state
        //-----------------------------------------------------------------------------------------------
        public void Update(GameTime gameTime)
        {
            _monsterPosition += _monsterVelocity;
            if(_isJumping)
            {
                _monsterVelocity.Y += .001f;
                if(_monsterPosition.Y > 0)
                {
                    _monsterPosition.Y = 0;
                    _monsterVelocity.Y = 0;
                    _isJumping = false;
                    OnFInishJump?.Invoke();
                }
            }
            
            if (_bunnyPosition.X < _monsterPosition.X - 0.1f)
            {
                Score++;
                _bunnyPosition.X = _monsterPosition.X + 2;
                OnpassBunny?.Invoke();
            }
        }

        //-----------------------------------------------------------------------------------------------
        // Jump - call this to attempt a jump on the monster 
        //-----------------------------------------------------------------------------------------------
        public void Jump()
        {
            if (_isJumping) return;

            _isJumping = true;
            _monsterVelocity.Y = -.03f;
            OnStartJump?.Invoke();
        }
    }
}
