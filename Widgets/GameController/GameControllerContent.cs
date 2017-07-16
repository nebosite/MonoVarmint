using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.IO;

namespace MonoVarmint.Widgets
{
    public partial class GameController
    {
        Dictionary<string, SpriteFont> _fontsByName = new Dictionary<string, SpriteFont>();
        Dictionary<string, Texture2D> _glyphsByName = new Dictionary<string, Texture2D>();
        Dictionary<string, VarmintSprite> _spritesByName = new Dictionary<string, VarmintSprite>();

        Dictionary<string, SoundEffect> _soundEffectsByName = new Dictionary<string, SoundEffect>();
        Dictionary<string, Song> _songsByName = new Dictionary<string, Song>();

        //-----------------------------------------------------------------------------------------------
        /// <summary>
        /// Do some visual setup to get the normalize the coordinate system, load default
        /// content, and then call out for any user-assigned work using the OnLoaded event.
        /// </summary>
        //-----------------------------------------------------------------------------------------------
        protected override void LoadContent()
        {
            Content = new EmbeddedContentManager(_graphics.GraphicsDevice)
            {
                RootDirectory = "Content"
            };
            // Set up a back buffer to render to
            _backBufferWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
            _backBufferHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;
            if ((float)_backBufferHeight / _backBufferWidth < 1.6)
            {
                _backBufferWidth = (int)(_backBufferHeight / 1.6);
                _backBufferXOffset = (GraphicsDevice.PresentationParameters.BackBufferWidth - _backBufferWidth) / 2;
            }

            var scaleFactor = _backBufferWidth / 1000.0f;
            _scaleToNativeResolution = Matrix.CreateScale(new Vector3(scaleFactor, scaleFactor, 1));

            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _utilityBlockTexture = Content.Load<Texture2D>("_utility_block");
            _circleTexture = Content.Load<Texture2D>("_utility_circle");
            _defaultFont = Content.Load<SpriteFont>("_utility_SegoeUI");

            _fontsByName.Add("_utility_SegoeUI", _defaultFont);
            SelectFont();

            // Widgets
            _widgetSpace = new VarmintWidgetSpace(this, _bindingContext);

            _visualTree = _widgetSpace.GetScreen("_default_screen_", null);
            OnLoaded?.Invoke();

            
        }

        //-----------------------------------------------------------------------------------------------
        /// <summary>
        /// Prepare - prepare the visual elements in this widget according to sytles
        /// </summary>
        //-----------------------------------------------------------------------------------------------
        internal void Prepare(VarmintWidget overlay)
        {
            overlay.Prepare(_widgetSpace.StyleLibrary);
        }

        internal void LoadFonts(params string[] names)
        {
            foreach (var name in names)
            {
                _fontsByName.Add(name, Content.Load<SpriteFont>(name));
            }
        }


        //-----------------------------------------------------------------------------------------------
        /// <summary>
        /// Call this first if you want to reload content
        /// </summary>
        //-----------------------------------------------------------------------------------------------
        public void ClearContent()
        {
            _fontsByName.Clear();
            _glyphsByName.Clear();
            _spritesByName.Clear();
            _widgetSpace = new VarmintWidgetSpace(this, _bindingContext);
        }

        //-----------------------------------------------------------------------------------------------
        /// <summary>
        /// Glyphs are textures with single images.   These can be saved as .xnb files embedded anywhere 
        /// in the project or they can be textures stored somewhere under the Content folder.  
        /// Glyphs are stored under the text key used to load them here.
        /// </summary>
        //-----------------------------------------------------------------------------------------------
        public void LoadGlyph(string name, bool throwOnDuplicate = true)
        {
            if (_glyphsByName.ContainsKey(name))
            {
                if (throwOnDuplicate) throw new ApplicationException("Duplicate Glyph Name: " + name);
                return;
            }

            _glyphsByName.Add(name, LoadTexture(name));
        }

        //-----------------------------------------------------------------------------------------------
        /// <summary>
        /// Sprites are textures that contain a set of images, all the same size (width x heigt).   
        /// These can be saved as .xnb files embedded anywhere in the project or they can be 
        /// textures stored somewhere under the Content folder. Sprites are stored under the text 
        /// key used to load them here.
        /// </summary>
        //-----------------------------------------------------------------------------------------------
        public void LoadSprite(string name, int width, int height)
        {
           
            var spriteTexture = LoadTexture(name);
            _spritesByName.Add(name, new VarmintSprite(spriteTexture, width, height));
        }


        //-----------------------------------------------------------------------------------------------
        /// <summary>
        /// Try to load local raw files first
        /// </summary>
        //-----------------------------------------------------------------------------------------------
        Texture2D LoadTexture(string name)
        {
            var fileName = Path.Combine(Content.RootDirectory, name) + ".png";
            if(File.Exists(fileName))
            {
                using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    return Texture2D.FromStream(GraphicsDevice, fileStream);
                }
            }

            return Content.Load<Texture2D>(name);
        }

        //-----------------------------------------------------------------------------------------------
        /// <summary>
        /// Load sound effects
        /// </summary>
        //-----------------------------------------------------------------------------------------------
        public void LoadSFX(params string[] names)
        {
            foreach (var name in names)
            {
                if (_soundEffectsByName.ContainsKey(name))
                    return;
                _soundEffectsByName.Add(name, Content.Load<SoundEffect>(name));
            }
        }

        public void LoadMusic(params string[] names)
        {
            foreach (var name in names)
            {
                if (_songsByName.ContainsKey(name))
                    return;
                _songsByName.Add(name, Content.Load<Song>(name));
            }
        }
    }
}
