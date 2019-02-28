using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;

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
        public string Style { get; set; }
        public VarmintWidget Parent { get; set; }
        public Color BackgroundColor { get; set; }
        public bool ChildrenAffectFormatting { get; set; }
        public bool HasFocus { get; set; }
        public object Tag { get; set; }

        private Color? _foregroundColor;
        public Color ForegroundColor
        {
            get
            {
                if (_foregroundColor != null) return _foregroundColor.Value;
                return Parent?.ForegroundColor ?? Color.Black;
            }
            set => _foregroundColor = value;
        }

        public float Opacity { get; set; } = 1.0f;

        public virtual Vector2 Offset { get; set; }

        public float Rotate { get; set; }
        public bool FlipVertical { get; set; }
        public bool FlipHorizontal { get; set; }

        public Tuple<float?, float?> SpecifiedSize { get; set; } 

        private Vector2? _size;
        internal Vector2? OriginalSize;
        public Vector2 Size
        {
            get => _size ?? Vector2.Zero;
            set
            {
                if (_size != null && _size == value) return;
                if (OriginalSize == null || _applyingStyles)
                {
                    OriginalSize = value;
                }
                _size = value;
                //UpdateFormatting(_size);
                OnSizeChanged?.Invoke(this);
            }
        }

        public bool HasSize => _size.HasValue;

        public Vector2 IntendedSize
        {
            get
            {
                if (_bindingTemplates.ContainsKey("Size")) return Size;
                return OriginalSize ?? Vector2.Zero;
            }
        }

        public bool AllowInput { get; set; }
        public bool IsVisible { get; set; }
        public bool ClipToBounds { get; set; }

        private AlignmentTuple _widgetAlignment;
        public AlignmentTuple WidgetAlignment
        {
            get
            {
                var output = new AlignmentTuple(
                    _widgetAlignment?.X ?? Parent?.WidgetAlignment.X,
                    _widgetAlignment?.Y ?? Parent?.WidgetAlignment.Y
                    );
                return output;
            }
            set => _widgetAlignment = value;
        }

        private AlignmentTuple _contentAlignment;
        public AlignmentTuple ContentAlignment
        {
            get
            {
                var output = new AlignmentTuple(
                    _contentAlignment?.X ?? Parent?.ContentAlignment.X,
                    _contentAlignment?.Y ?? Parent?.ContentAlignment.Y
                    );
                return output;
            }
            set => _contentAlignment = value;
        }

        private float? _fontSize;
        public float FontSize
        {
            get
            {
                if (_fontSize != null) return _fontSize.Value;
                if (Parent == null) return 0.1f;
                return Parent.FontSize;
            }
            set => _fontSize = value;
        }


        private string _fontName;
        public string FontName
        {
            get => _fontName ?? Parent?.FontName;
            set => _fontName = value;
        }



        public virtual WidgetMargin Margin { get; set; }

        private object _content;
        public virtual object Content
        {
            get => _content;
            set
            {
                if (value is VarmintWidget varmintWidget) {
                    varmintWidget.Parent = this;
                }
                _content = value;
            }
        }

        public bool WrapContent { get; set; }

        private object _xbindingContext;
        public object BindingContext
        {
            get => _xbindingContext ?? Parent?.BindingContext;
            set
            {
                _xbindingContext = value;
                _prepared = false;
            }
        }

        private object _eventBindingContext;
        public object ControlHandler
        {
            get => _eventBindingContext ?? Parent?.ControlHandler;
            set => _eventBindingContext = value;
        }

        private readonly List<VarmintWidgetAnimation> _animations = new List<VarmintWidgetAnimation>();

        /// <summary>
        /// AbsoluteOffset
        /// </summary>
        public virtual Vector2 AbsoluteOffset
        {
            get
            {
                if (Parent == null) return Offset;
                return Parent.AbsoluteOffset + Offset;
            }
        }


        /// <summary>
        /// Center of this widget in absolute coordinates
        /// </summary>
        public Vector2 AbsoluteCenter => AbsoluteOffset + Size / 2;

        public Dictionary<string, string> Parameters { get; set; }

        private static int _globalWidgetCount;


        /// <summary>
        /// AbsoluteOpacity
        /// </summary>
        public virtual float AbsoluteOpacity
        {
            get
            {
                if (Parent == null) return Opacity;
                return Parent.AbsoluteOpacity * Opacity;
            }
        }

        public virtual Color RenderBackgroundColor => BackgroundColor * AbsoluteOpacity;

        public virtual Color RenderForegroundColor => ForegroundColor * AbsoluteOpacity;

        public virtual Color RenderGraphicColor => Color.White * AbsoluteOpacity;

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Static ctor
        /// </summary>
        //--------------------------------------------------------------------------------------
        static VarmintWidget()
        {
            KnownAssemblies.Add(typeof(VarmintWidget).GetTypeInfo().Assembly);
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// ctor
        /// </summary>
        //--------------------------------------------------------------------------------------
        public VarmintWidget()
        {
            Parameters = new Dictionary<string, string>();
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

        private bool _prepared;
        private bool _updating;
        private bool _initCalled;

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Prepare - apply bindings and styles
        /// </summary>
        //--------------------------------------------------------------------------------------
        public void Prepare(Dictionary<string, VarmintWidgetStyle> styleLibrary)
        {
            if (_prepared) return;
            ApplyStyles(styleLibrary);
            UpdateBindings(BindingContext);
            ReadBindings();
            if (!_initCalled)
            {
                _initCalled = true;
                OnInit?.Invoke(this);
            }

            foreach(var child in Children)
            {
                child.Prepare(styleLibrary);
            }
            _prepared = true;
        }


        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Update 
        /// </summary>
        //--------------------------------------------------------------------------------------
        public void Update()
        {
            _updating = true;
            // Load binding data first in case the init logic needs it.
            ReadBindings();
            _updating = false;
        }

    }
}