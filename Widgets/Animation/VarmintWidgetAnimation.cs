using Microsoft.Xna.Framework;
using System;

namespace MonoVarmint.Widgets
{
    //--------------------------------------------------------------------------------------
    /// <summary>
    /// VarmintWidgetAnimation - Animator for widgets
    /// </summary>
    //--------------------------------------------------------------------------------------
    public partial class VarmintWidgetAnimation : VarmintAnimation
    {
        VarmintWidget _widget;
        
        //--------------------------------------------------------------------------------------
        /// <summary>
        /// ctor
        /// </summary>
        //--------------------------------------------------------------------------------------
        public VarmintWidgetAnimation(double durationSeconds, Action<VarmintWidget, double> animator) : 
            base(durationSeconds)
        {
            _animate = (delta) => animator(_widget, delta);
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Update - calculates the animation delta and calls the attached animation code
        /// </summary>
        //--------------------------------------------------------------------------------------
        internal void Update(VarmintWidget widget, GameTime gameTime)
        {
            _widget = widget;
            base.Update(gameTime);
        }
    }
}
