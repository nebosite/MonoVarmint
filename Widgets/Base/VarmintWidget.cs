using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace MonoVarmint.Widgets
{
    
    //--------------------------------------------------------------------------------------
    /// <summary>
    /// VarmintWidget - A very simple widget class for MonoGame
    /// </summary>
    //--------------------------------------------------------------------------------------
    public partial class VarmintWidget
    {
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
        public virtual Vector2 Offset
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
        /// Center of this widget in absolute coordinates
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
        /// ctor
        /// </summary>
        //--------------------------------------------------------------------------------------
        public VarmintWidget(Vector2 size) : this()
        {
            Size = size;
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

    }
}