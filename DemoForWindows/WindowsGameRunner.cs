using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DemoForWindows
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class WindowsGameRunner : Demo.Shared.GameRunner
    {
        public override void NativeHandleUserDeactivate()
        {
            _controller.Exit();
        }
    }
}
