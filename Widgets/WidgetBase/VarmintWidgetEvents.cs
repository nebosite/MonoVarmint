using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace MonoVarmint.Widgets
{

    public enum FlickDirection { W, SW, S, SE, E, NE, N, NW }

    public struct VarmintFlickData
    {
        public VarmintWidget SourceWidget;
        public Vector2 Location;
        public Vector2 Delta;
        public double Angle =>  Math.Atan2(-Delta.Y, Delta.X);

        public FlickDirection Direction
        {
            get
            {
                var angle = Angle;
                var theta = -Math.PI;
                var step = Math.PI * 2 / 8;
                theta += step/2; if (angle < theta) return FlickDirection.W;
                theta += step;   if (angle < theta) return FlickDirection.SW;
                theta += step;   if (angle < theta) return FlickDirection.S;
                theta += step;   if (angle < theta) return FlickDirection.SE;
                theta += step;   if (angle < theta) return FlickDirection.E;
                theta += step;   if (angle < theta) return FlickDirection.NE;
                theta += step;   if (angle < theta) return FlickDirection.N;
                theta += step;   if (angle < theta) return FlickDirection.NW;
                return FlickDirection.W;
            }
        }       
    }

    public partial class VarmintWidget
    {
        public event Func<VarmintWidget, Vector2, EventHandledState> OnTap;
        public event Func<VarmintWidget, Vector2, EventHandledState> OnDoubleTap;
        public event Func<VarmintWidget, Vector2, EventHandledState> OnContextTap;
        public event Func<VarmintFlickData, EventHandledState> OnFlick; 
        public event Func<VarmintWidget, Vector2, Vector2, EventHandledState> OnDrag; // location, delta
        public event Func<EventHandledState> OnDragComplete;
        public event Func<EventHandledState> OnDragCancel;
        public event Func<VarmintWidget, Vector2, float, float, EventHandledState> OnPinch; // location, rotation, scale
        public event Func<EventHandledState> OnPinchComplete;

        public event Func<VarmintWidget, TouchLocation, EventHandledState> OnTouchUp;
        public event Func<VarmintWidget, TouchLocation, EventHandledState> OnTouchDown;
        public event Func<VarmintWidget, TouchMoveType, TouchLocation, TouchLocation, EventHandledState> OnTouchMove;

        public event Action<GameTime, VarmintWidget> OnRender;
        public event Action<VarmintWidget> OnInit;
        public event Func<char, EventHandledState> OnInputCharacter;
        public event Func<VarmintWidget, EventHandledState> OnSizeChanged;


        //--------------------------------------------------------------------------------------
        /// <summary>
        /// SetCustomRender - Clear out the Render event and replace with yours
        /// </summary>
        //--------------------------------------------------------------------------------------
        public void SetCustomRender(Action<GameTime, VarmintWidget> render, bool removeDefaultRenderActions = true)
        {
            if (OnRender != null && removeDefaultRenderActions)
            {
                foreach (Action<GameTime, VarmintWidget> action in OnRender.GetInvocationList())
                {
                    OnRender -= action;
                }
            }

            OnRender += render;
        }

    }
}