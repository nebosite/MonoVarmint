using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace MonoVarmint.Widgets
{
    public enum HorizontalContentAlignment
    {
        Left,
        Center,
        Right
    };

    public enum VerticalContentAlignment
    {
        Top,
        Center,
        Bottom
    };

    public enum TouchMoveType
    {
        Move,
        Leave,
        Enter
    };

    public partial class VarmintWidget
    {
        const string BindingContextPropertyName = "BindingContext";

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// EventHandledState
        /// </summary>
        //--------------------------------------------------------------------------------------
        public enum EventHandledState
        {
            NotHandled,
            Handled
        }
    }
    }