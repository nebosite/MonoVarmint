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

    //--------------------------------------------------------------------------------------
    /// <summary>
    /// VarmintWidget - A very simple widget class for MonoGame
    /// </summary>
    //--------------------------------------------------------------------------------------
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

        public IReadOnlyList<VarmintWidget> Children { get { return (IReadOnlyList<VarmintWidget>)children; } }

        [VarmintWidgetInject]
        public IMediaRenderer Renderer { get; set; }

        public string Name { get; set; }
        public VarmintWidget Parent { get; set; }
        public Color BackgroundColor { get; set; }
        public bool ChildrenAffectFormatting { get; set; }

        private Color? _foregroundColor;
        public Color ForegroundColor
        {
            get
            {
                if (_foregroundColor != null) return _foregroundColor.Value;
                if (Parent == null) return Color.Black;
                return Parent.ForegroundColor;
            }
            set { _foregroundColor = value; }
        }

        private Vector2 _offset;
        public Vector2 Offset
        {
            get
            {
                return _offset;
            }
            set
            {
                _offset = value;
            }
        }

        private Vector2? _size = null;
        internal Vector2? _originalSize = null;
        public Vector2 Size
        {
            get
            {
                if (_size == null)
                {
                    Size = Vector2.Zero;
                }
                return _size.Value;
            }
            set
            {
                if (_size == null || _size != value)
                {
                    if (_size == null)
                    {
                        _originalSize = value;
                    }
                    _size = value;
                    UpdateChildFormatting();
                }
            }
        }

        public Vector2 IntendedSize
        {
            get
            {
                if (_bindingTemplates.ContainsKey("Size")) return Size;
                return _originalSize ?? Vector2.Zero;
            }
        }

        public bool AllowInput { get; set; }
        public bool IsVisible { get; set; }

        private HorizontalContentAlignment? _horizontalContentAlignment;
        public HorizontalContentAlignment HorizontalContentAlignment
        {
            get
            {
                if (_horizontalContentAlignment != null) return _horizontalContentAlignment.Value;
                if (Parent == null) return HorizontalContentAlignment.Left;
                return Parent.HorizontalContentAlignment;
            }
            set { _horizontalContentAlignment = value; }
        }

        private VerticalContentAlignment? _verticalContentAlignment;
        public VerticalContentAlignment VerticalContentAlignment
        {
            get
            {
                if (_verticalContentAlignment != null) return _verticalContentAlignment.Value;
                if (Parent == null) return VerticalContentAlignment.Top;
                return Parent.VerticalContentAlignment;
            }
            set { _verticalContentAlignment = value; }
        }

        public virtual WidgetMargin Margin { get; set; }
        public object Content { get; set; }
        public bool WrapContent { get; set; }

        private object _bindingContext;
        public object BindingContext
        {
            get { return _bindingContext; }
            set { UpdateBindings(value); }
        }

        // EVENTS ---------------------------------------------------------------
        public event Func<VarmintWidget, Vector2, EventHandledState> OnTap;
        public event Func<VarmintWidget, Vector2, Vector2, EventHandledState> OnFlick;
        public event Func<VarmintWidget, Vector2, EventHandledState> OnDrag;
        public event Func<EventHandledState> OnDragComplete;
        public event Func<EventHandledState> OnDragCancel;
        public event Func<VarmintWidget, TouchLocation, EventHandledState> OnTouchUp;
        public event Func<VarmintWidget, TouchLocation, EventHandledState> OnTouchDown;
        public event Func<VarmintWidget, TouchMoveType, TouchLocation, TouchLocation, EventHandledState> OnTouchMove;
        public event Action<GameTime, VarmintWidget> OnRender;
        public event Action<VarmintWidget> OnInit;
        public event Func<char, EventHandledState> OnInputCharacter;
        // EVENTS --------------------------------------------------------------

        private IList<VarmintWidget> children = new List<VarmintWidget>();
        List<VarmintWidgetAnimation> _animations = new List<VarmintWidgetAnimation>();

        /// <summary>
        /// AbsoluteOffset
        /// </summary>
        public Vector2 AbsoluteOffset
        {
            get
            {
                if (Parent == null) return Offset;
                else return Parent.AbsoluteOffset + Offset;
            }
        }

        /// <summary>
        /// AbsoluteOffset
        /// </summary>
        public Vector2 AbsoluteCenter
        {
            get
            {
                return AbsoluteOffset + Size / 2;
            }
        }

        static int _globalWidgetCount = 0;

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// ctor
        /// </summary>
        //--------------------------------------------------------------------------------------
        public VarmintWidget()
        {
            ChildrenAffectFormatting = false;
            IsVisible = true;
            _globalWidgetCount++;
            Name = "W" + _globalWidgetCount.ToString("000000");
            AllowInput = true;
            Margin = new WidgetMargin();
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// SetCustomRender
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

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Static ctor
        /// </summary>
        //--------------------------------------------------------------------------------------
        static VarmintWidget()
        {
            _knownAssemblies.Add(typeof(VarmintWidget).GetTypeInfo().Assembly);
            DragLengthThreshhold = .05f;
            FlickThreshholdSeconds = 0.3;
        }

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

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// ctor
        /// </summary>
        //--------------------------------------------------------------------------------------
        public VarmintWidget(Vector2 size) : this()
        {
            Size = size;
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// AddChild
        /// </summary>
        //--------------------------------------------------------------------------------------
        public virtual void AddChild(VarmintWidget widget, bool suppressChildUpdate = false)
        {
            children.Add(widget);
            widget.Parent = this;
            if(ChildrenAffectFormatting && !suppressChildUpdate)
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

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// ReadBindings - Force the bindings to be read
        /// </summary>
        //--------------------------------------------------------------------------------------
        public void ReadBindings(bool recurse = false)
        {
            if(recurse)
            {
                foreach(var child in children)
                {
                    child.ReadBindings(true);
                }
            }

            foreach (var bindingAction in _bindingActions)
            {
                bindingAction();
            }
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Render
        /// </summary>
        //--------------------------------------------------------------------------------------
        public virtual void RenderMe(GameTime gameTime)
        {
            ReadBindings();
            if (!IsVisible) return; 
            var localAnimations = _animations.ToArray();
            foreach (var animation in localAnimations) animation.Update(this, gameTime);
            _animations.RemoveAll(a => a.IsComplete);
            OnRender?.Invoke(gameTime, this);

            if (children.Count > 0)
            {
                // Make a local copy because children can modify parent/child relationships
                var localChildren = new List<VarmintWidget>(children);
                foreach (var child in localChildren)
                {
                    child.RenderMe(gameTime);
                }
            }
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// HitTest
        /// </summary>
        //--------------------------------------------------------------------------------------
        public IList<VarmintWidget> HitTest(Vector2 absolutePoint)
        {
            var hitList = new List<VarmintWidget>();
            HitTestInternal(absolutePoint, hitList);
            return hitList;
        }

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

        DateTime _dragStartTime = DateTime.MaxValue;
        List<VarmintWidget> _recentDragWidgets = new List<VarmintWidget>();


        List<Action> _bindingActions = new List<Action>();
        //--------------------------------------------------------------------------------------
        /// <summary>
        /// UpdateBindings - Recalculate binding actions based on the new binding context.
        ///                 Binding actions are run every time an object is rendered
        /// </summary>
        //--------------------------------------------------------------------------------------
        private void UpdateBindings(object newContext)
        {
            if(_bindingTemplates.ContainsKey(BindingContextPropertyName))
            {
                // pull the new binding context off of the incoming context
                var contextProperty = _bindingTemplates[BindingContextPropertyName].Trim('{', '}', ' ');

                var propertyInfo = newContext.GetType().GetProperty(contextProperty);
                if(propertyInfo== null)
                {
                    throw new ApplicationException("Could not find property '" + contextProperty 
                        + "' on incoming context object of type: " + newContext.GetType().Name);
                }

                newContext = propertyInfo.GetValue(newContext);
            }

            // Update the children first.  Stop when we find a child that has had it's 
            // binding context set explicitely to something else.

            foreach (var child in children)
            {
                if (child.BindingContext == null || child.BindingContext == this.BindingContext)
                {
                    child.BindingContext = newContext;
                }
            }

            _bindingContext = newContext;

            _bindingActions = new List<Action>();
            foreach (var bindingPair in _bindingTemplates)
            {
                if (bindingPair.Key == BindingContextPropertyName) continue;
                var bindingName = bindingPair.Value.Trim('{', '}', ' ');

                var targetEventInfo = GetType().GetEvent(bindingPair.Key, _publicInstance);
                if (targetEventInfo != null)
                {
                    var sourceMethodInfo = _bindingContext.GetType().GetMethod(bindingName, _publicInstance);
                    if (sourceMethodInfo == null)
                    {
                        throw new ApplicationException("Could not find method '" + bindingName
                            + "' on type " + _bindingContext.GetType());
                    }

                    Delegate handler =
                         Delegate.CreateDelegate(targetEventInfo.EventHandlerType,
                                                 _bindingContext,
                                                 sourceMethodInfo);
                    targetEventInfo.AddEventHandler(this, handler);

                }
                else
                {
                    if (_bindingContext != null)
                    {
                        var sourcePropertyInfo = _bindingContext.GetType().GetProperty(bindingName, _publicInstance);
                        if (sourcePropertyInfo == null) throw new ApplicationException("Could not find property '"
                             + bindingName + "' on " + _bindingContext.GetType());
                        var targetPropertyInfo = GetType().GetProperty(bindingPair.Key);
                        if (targetPropertyInfo == null) throw new ApplicationException("Could not find property '"
                            + bindingPair.Key + "' on " + GetType());
                        _bindingActions.Add(() =>
                        {
                            object newValue = sourcePropertyInfo.GetValue(_bindingContext);
                            targetPropertyInfo.SetValue(this, newValue);
                        });
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("WARNING: Binding object is NULL");
                    }
                }

            }

            UpdateChildFormatting(true);
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// UpdateChildFormatting - the size has changed, so update the offsets and sizes of
        ///                         the children of this widget
        /// </summary>
        //--------------------------------------------------------------------------------------
        public virtual void UpdateChildFormatting(bool recurse = false)
        {
            if (recurse)
            {
                foreach (var child in children)
                {
                    child.UpdateChildFormatting(true);
                }
            } 

            foreach (var child in children)
            {
                var newSize = child.IntendedSize;
                var newOffset = Vector2.Zero;
                var availableSize = Size - newSize;
                availableSize.X -= ((child.Margin.Left ?? 0) + (child.Margin.Right ?? 0));
                availableSize.Y -= ((child.Margin.Top ?? 0) + (child.Margin.Bottom ?? 0));

                switch (HorizontalContentAlignment)
                {
                    case HorizontalContentAlignment.Left:
                        newOffset.X = child.Margin.Left?? 0;
                        if (child.Margin.Right != null) newSize.X += availableSize.X;
                        break;
                    case HorizontalContentAlignment.Center:
                        var width = newSize.X + (child.Margin.Left ?? 0) + (child.Margin.Right ?? 0);
                        newOffset.X = (Size.X - width) / 2 + (child.Margin.Left ?? 0);
                        break;
                    case HorizontalContentAlignment.Right:
                        if (child.Margin.Left != null) newSize.X += availableSize.X;
                        else
                        {
                            newOffset.X = availableSize.X;
                        }
                        break;
                }

                switch (VerticalContentAlignment)
                {
                    case VerticalContentAlignment.Top:
                        newOffset.Y = child.Margin.Top?? 0;
                        if (child.Margin.Bottom != null) newSize.Y += availableSize.Y;
                        break;
                    case VerticalContentAlignment.Center:
                        var height = newSize.Y + (child.Margin.Top ?? 0) + (child.Margin.Bottom ?? 0);
                        newOffset.Y = (Size.Y - height) / 2 + (child.Margin.Top ?? 0);
                        break;
                    case VerticalContentAlignment.Bottom:
                        if (child.Margin.Top != null) newSize.Y += availableSize.Y;
                        else
                        {
                            newOffset.Y = availableSize.Y;
                        }
                        break;
                }

                child.Offset = newOffset;
                child.Size = newSize;
                
            }
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Init - trip all the OnInit events in this widget Tree.  
        /// </summary>
        //--------------------------------------------------------------------------------------
        public void Init()
        {
            // Load binding data frist in case the init logic needs it.
            ReadBindings();

            // user-provided init logic
            OnInit?.Invoke(this);

            foreach (var child in children)
            {
                child.Init();
            }
        }

        static VarmintWidget _focusedContol = null;
        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Focus - set input focus on this control 
        /// </summary>
        //--------------------------------------------------------------------------------------
        public void Focus()
        {
            _focusedContol = this;
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// HandleInputCharacter
        /// </summary>
        //--------------------------------------------------------------------------------------
        public void HandleInputCharacter(char c)
        {
            if(_focusedContol != null)
            {
                _focusedContol.OnInputCharacter(c);
            }
        }
    }
}