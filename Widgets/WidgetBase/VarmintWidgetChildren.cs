using System.Collections.Generic;

namespace MonoVarmint.Widgets
{
    public partial class VarmintWidget
    {
        private readonly IList<VarmintWidget> _children = new List<VarmintWidget>();

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// FindWidgetsByType - seach the family tree for all widgets of the matching type
        /// </summary>
        //--------------------------------------------------------------------------------------
        public List<T> FindWidgetsByType<T>() where T : VarmintWidget
        {
            var output = new List<T>();
            FindWidgetsByType(output);
            return output;
        }
        private void FindWidgetsByType<T>(ICollection<T> output) where T : VarmintWidget
        {
            if (this is T) output.Add(this as T);
            foreach(var child in Children)
            {
                child.FindWidgetsByType(output);
            }
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// FindWidgetByName
        /// </summary>
        //--------------------------------------------------------------------------------------
        internal VarmintWidget FindWidgetByName(string name)
        {
            if (Name == name) return this;

            foreach (var child in Children)
            {
                var match = child.FindWidgetByName(name);
                if (match != null) return match;
            }
            return null;
        }

        /// <summary>
        /// Enumerated list of children in visual order
        /// </summary>
        public virtual IEnumerable<VarmintWidget> Children
        {
            get
            {
                if (Content is VarmintWidget widget)
                    yield return widget;
                foreach (var child in _children)
                {
                    yield return child;
                }
            }
        }

        public List<VarmintWidget> ChildrenCopy => new List<VarmintWidget>(Children);

        public bool HasChildren => Children.GetEnumerator().MoveNext();

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// AddChild
        /// </summary>
        //--------------------------------------------------------------------------------------
        public virtual void AddChild(VarmintWidget widget, bool suppressChildUpdate = false)
        {
            _children.Add(widget);
            widget.Parent = this;
            //if (ChildrenAffectFormatting && !suppressChildUpdate)
            //{
            //    UpdateFormatting(Parent?.Size ?? Size);
            //}
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// InsertChild
        /// </summary>
        //--------------------------------------------------------------------------------------
        public virtual void InsertChild(VarmintWidget widget, bool suppressChildUpdate = false)
        {
            _children.Insert(0, widget);
            widget.Parent = this;
            //if (ChildrenAffectFormatting && !suppressChildUpdate)
            //{
            //    UpdateFormatting(Parent?.Size ?? Size);
            //}
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// RemoveChild
        /// </summary>
        //--------------------------------------------------------------------------------------
        public virtual void RemoveChild(VarmintWidget childToRemove, bool suppressChildUpdate = false)
        {
            _children.Remove(childToRemove);
            //if (ChildrenAffectFormatting && !suppressChildUpdate)
            //{
            //    UpdateFormatting(Parent?.Size ?? Size);
            //}
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// ClearChildren
        /// </summary>
        //--------------------------------------------------------------------------------------
        public virtual void ClearChildren()
        {
            _children.Clear();
        //    if (ChildrenAffectFormatting)
        //    {
        //        UpdateFormatting(Parent?.Size ?? Size);
        //    }
        }

    }
}