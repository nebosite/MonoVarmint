using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace MonoVarmint.Widgets
{
    public partial class VarmintWidget
    {
        //--------------------------------------------------------------------------------------
        /// <summary>
        /// HitTest
        /// </summary>
        //--------------------------------------------------------------------------------------
        internal void ClearAnimations()
        {
            _animations.Clear();
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// HitTest
        /// </summary>
        //--------------------------------------------------------------------------------------
        internal void FinishAnimation()
        {
            foreach (var animation in _animations)
            {
                animation.Finish(this);
            }
            _animations.Clear();
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// AddAnimation - added animations are automatically handled while rendering
        /// </summary>
        //--------------------------------------------------------------------------------------
        public void AddAnimation(VarmintWidgetAnimation animation)
        {
            _animations.Add(animation);
        }

    }
}