using System;
using Android.Views;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DemoForAndroid
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class AndroidGameRunner : Demo.Shared.GameRunner
    {
        public Action SoftExit { get; set; }
        public override void NativeHandleUserDeactivate()
        {
            SoftExit?.Invoke();
        }

        internal View GetViewService()
        {
            return (View)_controller.GetService(typeof(View));
        }
    }
}
