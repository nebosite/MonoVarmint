#region Using directives

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using XnaMediaPlayer = Microsoft.Xna.Framework.Media.MediaPlayer;

#endregion

namespace MonoVarmint.Widgets
{
    public partial class GameController
    {
        
        internal readonly List<SoundInstanceBase> AnimatingInstances = new List<SoundInstanceBase>();

        //-----------------------------------------------------------------------------------------------
        /// <summary>
        /// Update audio animations
        /// </summary>
        /// <param name="gameTime"></param>
        //-----------------------------------------------------------------------------------------------
        private void UpdateAudio(GameTime gameTime)
        {
            foreach(var instance in new List<SoundInstanceBase>(AnimatingInstances))
            {
                foreach (var animation in new List<VarmintAudioAnimation>(instance.Animations))
                {
                    animation.Update(instance, gameTime);
                    if (!animation.IsComplete) continue;
                    AnimatingInstances.Remove(instance);
                    instance.Animations.Remove(animation);
                }
            }
        }


        
        public IVarmintAudioInstance CurrentSong { get; private set; }
        
        //-----------------------------------------------------------------------------------------------
        /// <summary>
        /// Play a sound
        /// </summary>
        /// <param name="name">The name of the sound to play</param>
        /// <returns>Returns an IVarmintAudioInstance of the played sound.</returns>
        //-----------------------------------------------------------------------------------------------
        public IVarmintAudioInstance PlaySound(string name) // Add volume here
        {
            IVarmintAudioInstance instance;
            if (_soundEffectsByName.ContainsKey(name))
                instance = PlaySoundEffect(name);
            else if (_songsByName.ContainsKey(name))
                instance = PlaySong(name);
            else throw new ArgumentException($"No sound with the name {name} has been loaded.");
            return instance;
        }

        //-----------------------------------------------------------------------------------------------
        /// <summary>
        /// Play a sound effect
        /// </summary>
        /// <param name="name">The name of the sound to play</param>
        /// <returns>Returns an IVarmintAudioInstance from the played sound</returns>
        //-----------------------------------------------------------------------------------------------
        private IVarmintAudioInstance PlaySoundEffect(string name)
        {
            var soundEffectInstance = _soundEffectsByName[name].CreateInstance();
            soundEffectInstance.Volume *= SoundEffectVolume;
            soundEffectInstance.Play();
            return new VarmintSoundEffectInstance(soundEffectInstance, this, name);
        }

        //-----------------------------------------------------------------------------------------------
        /// <summary>
        /// Play a song
        /// </summary>
        /// <param name="name">The name of the song to play</param>
        /// <returns>Returns an IVarmintAudioInstance from the played sound</returns>
        //-----------------------------------------------------------------------------------------------
        private IVarmintAudioInstance PlaySong(string name)
        {
            var instance = new VarmintSongInstance(_songsByName[name], this);
            CurrentSong = instance;
            instance.Play();
            return instance;
        }

        //-----------------------------------------------------------------------------------------------
        /// <summary>
        /// Current music from 0 to 1.
        /// </summary>
        //-----------------------------------------------------------------------------------------------
        public float MusicVolume
        {
            set => XnaMediaPlayer.Volume = value;
            get => XnaMediaPlayer.Volume;
        }

        public float SoundEffectVolume { get; set; } = 1f;

#region classes

        internal abstract class SoundInstanceBase : IVarmintAudioInstance
        {
            // List of animations

            public abstract void Dispose();
            public abstract void Stop();
            public abstract void Pause();
            public abstract void Resume();
            public abstract bool IsLooping { get; set; }
            public abstract void Play();
            public abstract float Volume { get; set; }
            
            internal readonly List<VarmintAudioAnimation> Animations = new List<VarmintAudioAnimation>();
            //-----------------------------------------------------------------------------------------------
            /// <summary>
            /// Apply an animation to this instance.
            /// </summary>
            /// <param name="animation">The animation to add</param>
            //-----------------------------------------------------------------------------------------------
            public void AddAnimation(VarmintAudioAnimation animation)
            {
                Animations.Add(animation);
                Renderer.AnimatingInstances.Add(this);
            }
            public abstract AudioType Type { get; }
            public abstract string Name { get; }

            protected readonly GameController Renderer;

            protected SoundInstanceBase(GameController renderer)
            {
                Renderer = renderer;
            }
        }

        private sealed class VarmintSoundEffectInstance : SoundInstanceBase
        {
            private readonly SoundEffectInstance _instance;

            //-----------------------------------------------------------------------------------------------
            /// <summary>
            /// The constructor by default sets looping to false.
            /// </summary>
            /// <param name="instance">The SoundEffectInstance this object should represent</param>
            /// <param name="renderer">The renderer to use with animations</param>
            /// <param name="name">The name of the sound effect</param>
            //-----------------------------------------------------------------------------------------------
            public VarmintSoundEffectInstance(SoundEffectInstance instance, GameController renderer, string name) : base(renderer)
            {
                Name = name;
                _instance = instance;
                IsLooping = false;
            }

            //-----------------------------------------------------------------------------------------------
            /// <inheritdoc />
            //-----------------------------------------------------------------------------------------------
            public override void Stop()
            {
                _instance.Stop();
            }

            //-----------------------------------------------------------------------------------------------
            /// <inheritdoc />
            //-----------------------------------------------------------------------------------------------
            public override void Pause()
            {
                _instance.Pause();
            }

            //-----------------------------------------------------------------------------------------------
            /// <inheritdoc />
            //-----------------------------------------------------------------------------------------------
            public override void Resume()
            {
                _instance.Resume();
            }

            //-----------------------------------------------------------------------------------------------
            /// <inheritdoc />
            //-----------------------------------------------------------------------------------------------
            public override bool IsLooping
            {
                get => _instance.IsLooped;
                set => _instance.IsLooped = value;
            }

            //-----------------------------------------------------------------------------------------------
            /// <inheritdoc />
            //-----------------------------------------------------------------------------------------------
            public override void Play()
            {
                _instance.Play();
            }

            //-----------------------------------------------------------------------------------------------
            /// <inheritdoc />
            //-----------------------------------------------------------------------------------------------
            public override void Dispose()
            {
                _instance.Dispose();
            }



            //-----------------------------------------------------------------------------------------------
            /// <inheritdoc />
            //-----------------------------------------------------------------------------------------------
            public override AudioType Type => AudioType.SoundEffect;

            public override string Name { get; }

            //-----------------------------------------------------------------------------------------------
            /// <inheritdoc />
            //-----------------------------------------------------------------------------------------------
            public override float Volume
            {
                get => _instance.Volume;
                set => _instance.Volume = value;
            }
            
        }

        private sealed class VarmintSongInstance : SoundInstanceBase
        {
            private readonly Song _song;
            
            //-----------------------------------------------------------------------------------------------
            /// <summary>
            /// The constructor by default sets looping to true.
            /// </summary>
            /// <param name="song">The Song this object should represent</param>
            /// <param name="renderer">The renderer to use for animations</param>
            public VarmintSongInstance(Song song, GameController renderer) : base(renderer)
            {
                _song = song;
                IsLooping = false;
            }

            //-----------------------------------------------------------------------------------------------
            /// <inheritdoc />
            //-----------------------------------------------------------------------------------------------
            public override void Stop()
            {
                if (this != Renderer.CurrentSong)
                    return;
				Renderer.CurrentSong = null;
				XnaMediaPlayer.Stop();
            }

            //-----------------------------------------------------------------------------------------------
            /// <inheritdoc />
            //-----------------------------------------------------------------------------------------------
            public override void Pause()
            {
                if (this != Renderer.CurrentSong)
                    return;
                XnaMediaPlayer.Pause();
            }

            //-----------------------------------------------------------------------------------------------
            /// <inheritdoc />
            /// <summary>
            /// Throws an exception if this is not the current song.
            /// </summary>
            //-----------------------------------------------------------------------------------------------
            public override void Resume()
            {
                if (this != Renderer.CurrentSong)
                    throw new InvalidOperationException("Attempted to resume a song when it is not the current song.");
                XnaMediaPlayer.Resume();
            }

            //-----------------------------------------------------------------------------------------------
            /// <inheritdoc />
            //-----------------------------------------------------------------------------------------------
            public override void Play()
            {
				Renderer.CurrentSong.Stop();
				Renderer.CurrentSong = this;
				XnaMediaPlayer.Play(_song);
            }

            //-----------------------------------------------------------------------------------------------
            /// <inheritdoc />
            //-----------------------------------------------------------------------------------------------
            public override bool IsLooping
            {
                get => XnaMediaPlayer.IsRepeating;
                set => XnaMediaPlayer.IsRepeating = value;
            }

            //-----------------------------------------------------------------------------------------------
            /// <inheritdoc />
            //-----------------------------------------------------------------------------------------------
            public override float Volume
            {
                get => XnaMediaPlayer.Volume;
                set => XnaMediaPlayer.Volume = value;
            }

            //-----------------------------------------------------------------------------------------------
            /// <inheritdoc />
            //-----------------------------------------------------------------------------------------------
            public override void Dispose()
            {
                Stop();
            }

            public override AudioType Type => AudioType.Song;
            public override string Name => _song.Name;
        }
#endregion
    }
}