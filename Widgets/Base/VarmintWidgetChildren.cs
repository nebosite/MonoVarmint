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
        private IList<VarmintWidget> children = new List<VarmintWidget>();

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// FindWidgetByName
        /// </summary>
        //--------------------------------------------------------------------------------------
        internal VarmintWidget FindWidgetByName(string name)
        {
            if (Name == name) return this;

            foreach (var child in children)
            {
                var match = child.FindWidgetByName(name);
                if (match != null) return match;
            }
            return null;
        }

        /// <summary>
        /// Enumerated list of children in visual order
        /// </summary>
        public IReadOnlyList<VarmintWidget> Children { get { return (IReadOnlyList<VarmintWidget>)children; } }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// AddChild
        /// </summary>
        //--------------------------------------------------------------------------------------
        public virtual void AddChild(VarmintWidget widget, bool suppressChildUpdate = false)
        {
            children.Add(widget);
            widget.Parent = this;
            if (ChildrenAffectFormatting && !suppressChildUpdate)
            {
                UpdateChildFormatting(true);
            }
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// AddChildren
        /// </summary>
        //--------------------------------------------------------------------------------------
        public virtual void AddChildren(VarmintWidget[] widgets, bool suppressChildUpdate = false)
        {
            foreach (var widget in widgets)
            {
                children.Add(widget);
                widget.Parent = this;
            }
            if (ChildrenAffectFormatting && !suppressChildUpdate)
            {
                UpdateChildFormatting(true);
            }
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// RemoveChild
        /// </summary>
        //--------------------------------------------------------------------------------------
        public void RemoveChild(VarmintWidget childToRemove, bool suppressChildUpdate = false)
        {
            children.Remove(childToRemove);
            if (ChildrenAffectFormatting && !suppressChildUpdate)
            {
                UpdateChildFormatting(true);
            }
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// ClearChildren
        /// </summary>
        //--------------------------------------------------------------------------------------
        public void ClearChildren()
        {
            children.Clear();
        }

    }
}