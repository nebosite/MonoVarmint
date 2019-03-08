using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Content;
using System.Linq;

namespace MonoVarmint.Widgets
{
    public partial class GameController
    {
        private readonly Dictionary<string, SpriteFont> _fontsByName = new Dictionary<string, SpriteFont>();
        private readonly Dictionary<string, Texture2D> _glyphsByName = new Dictionary<string, Texture2D>();
        private readonly Dictionary<string, VarmintSprite> _spritesByName = new Dictionary<string, VarmintSprite>();
        
        private readonly Dictionary<string, SoundEffect> _soundEffectsByName = new Dictionary<string, SoundEffect>();
        private readonly Dictionary<string, Song> _songsByName = new Dictionary<string, Song>();

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

            // Set up buffer size to be a portrait mode of at least 1x1.6 ratio
            _backBufferWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
            _backBufferHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;
            if ((float)_backBufferHeight / _backBufferWidth < 1.6)
            {
                _backBufferWidth = (int)(_backBufferHeight / 1.6);
                _backBufferXOffset = (GraphicsDevice.PresentationParameters.BackBufferWidth - _backBufferWidth) / 2;
            }

            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _utilityBlockTexture = Content.Load<Texture2D>("_utility_block");
            _circleTexture = Content.Load<Texture2D>("_utility_circle");
            _defaultFont = Content.Load<SpriteFont>("_utility_SegoeUI");

            _fontsByName.Add("_utility_SegoeUI", _defaultFont);
            SelectFont();

            // Widgets
            _widgetSpace = new VarmintWidgetSpace(this);

            _debugContext.ScreenSize = ScreenSize;
            _debugScreen = _widgetSpace.FindWidgetByName("_debug_screen_");
            _debugScreen.BindingContext = _debugContext;
            _debugScreen.Prepare(null);
            _debugWidget = _debugScreen.Children.First();
            _debugContentHolder = _debugScreen.FindWidgetByName("_debug_contentslot_");
            SetScreen("_default_screen_", null);
            OnGameLoaded?.Invoke();
        }

        //-----------------------------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        //-----------------------------------------------------------------------------------------------
        internal void LoadFonts(params string[] names)
        {
            foreach (var name in names)
            {
                _fontsByName.Add(Path.GetFileNameWithoutExtension(name), Content.Load<SpriteFont>(name));
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
            _songsByName.Clear();
            _soundEffectsByName.Clear();
            _widgetSpace = new VarmintWidgetSpace(this);
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

            _glyphsByName.Add(Path.GetFileNameWithoutExtension(name), LoadTexture(name));
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
            _spritesByName.Add(Path.GetFileNameWithoutExtension(name), new VarmintSprite(spriteTexture, width, height));
        }


        //-----------------------------------------------------------------------------------------------
        /// <summary>
        /// Try to load local raw files first
        /// </summary>
        //-----------------------------------------------------------------------------------------------
        Texture2D LoadTexture(string name)
        {
            // MONOTODO: Can you extend this to look for the content with any extension anywhere int he content folder?
            var fileName = Path.Combine(Content.RootDirectory, name) + ".png";
            if(File.Exists(fileName))
            {
                using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    return Texture2D.FromStream(GraphicsDevice, fileStream);
                }
            }

            try
            {
                return Content.Load<Texture2D>(name);
            }
            catch(Exception e)
            {
                throw new InvalidOperationException($"Could not load texture: {name}.  {e.Message}", e);
            }
        }

        string[] _soundExtensions = new[] { ".wav", ".mp3"};
        //-----------------------------------------------------------------------------------------------
        /// <summary>
        /// Load sound effects
        /// </summary>
        //-----------------------------------------------------------------------------------------------
        public void LoadSoundEffects(params string[] names)
        {
            foreach (var name in names)
            {
                var indexName = Path.GetFileNameWithoutExtension(name);
                if (_soundEffectsByName.ContainsKey(name))
                    return;
                if(_songsByName.ContainsKey(name))
                    throw new ContentLoadException("Cannot load same file as both a sound effect and song.");

                foreach(var extension in _soundExtensions)
                {
                    var fileName = Path.Combine(Content.RootDirectory, name) + extension;
                    if (File.Exists(fileName))
                    {
                        using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                        {
                            _soundEffectsByName.Add(indexName, SoundEffect.FromStream(fileStream));

                            return;
                        }
                    }

                }

                var soundEffect = Content.Load<SoundEffect>(name);
                _soundEffectsByName.Add(indexName, soundEffect );
            }
        }

        //-----------------------------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        //-----------------------------------------------------------------------------------------------
        public void LoadMusic(params string[] names)
        {
            foreach (var name in names)
            {
                var indexName = Path.GetFileNameWithoutExtension(name);
                if (_songsByName.ContainsKey(name))
                    return;
                if (_soundEffectsByName.ContainsKey(name))
                    throw new ContentLoadException("Cannot laod same file as both a sound effect and song.");

                _songsByName.Add(indexName, Content.Load<Song>(name));
            }
        }
    }
}
