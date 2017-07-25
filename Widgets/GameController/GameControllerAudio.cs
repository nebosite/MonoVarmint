using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using MonoVarmint.Widgets.Animation;

namespace MonoVarmint.Widgets
{
    public partial class GameController
    {
        
        internal readonly List<SoundInstanceBase> AnimatingInstances = new List<SoundInstanceBase>();

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
        


        private IVarmintAudioInstance PlaySoundEffect(string name)
        {
            var soundEffectInstance = _soundEffectsByName[name].CreateInstance();
            soundEffectInstance.Volume *= (float)SoundEffectVolume;
            soundEffectInstance.Play();
            return new VarmintSoundEffectInstance(soundEffectInstance, this);
        }

        public static IVarmintAudioInstance CurrentSong { get; private set; }
        
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

        private IVarmintAudioInstance PlaySong(string name)
        {
            if (CurrentSong != null)
                throw new InvalidOperationException("Cannot play a song while another song is currently playing.");
            var instance = new VarmintSongInstance(_songsByName[name], this);
            CurrentSong = instance;
            instance.Play();
            return instance;
        }

        /// <summary>
        /// Current music from 0 to 1.
        /// </summary>
	    public double MusicVolume
        {
            set => MediaPlayer.Volume = (float)value;
            get => MediaPlayer.Volume;
        }

        public double SoundEffectVolume { get; set; } = 1.0;

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
            public void ApplyAnimation(VarmintAudioAnimation animation)
            {
                Animations.Add(animation);
                _renderer.AnimatingInstances.Add(this);
            }
            public abstract AudioType Type { get; }
            public abstract string Name { get; }

            private readonly GameController _renderer;

            protected SoundInstanceBase(GameController renderer)
            {
                _renderer = renderer;
            }
        }

        private sealed class VarmintSoundEffectInstance : SoundInstanceBase
        {
            private readonly SoundEffectInstance _instance;

            /// <summary>
            /// The constructor by default sets looping to false.
            /// </summary>
            /// <param name="instance">The SoundEffectInstance this object should represent</param>
            /// <param name="renderer">The renderer to use with animations</param>
            /// <param name="name">The name of the sound effect</param>
            public VarmintSoundEffectInstance(SoundEffectInstance instance, GameController renderer, string name) : base(renderer)
            {
                Name = name;
                _instance = instance;
                IsLooping = false;
            }

            /// <inheritdoc />
            public override void Stop()
            {
                _instance.Stop();
            }

            /// <inheritdoc />
            public override void Pause()
            {
                _instance.Pause();
            }

            /// <inheritdoc />
            public override void Resume()
            {
                _instance.Resume();
            }

            /// <inheritdoc />
            public override bool IsLooping
            {
                get => _instance.IsLooped;
                set => _instance.IsLooped = value;
            }

            /// <inheritdoc />
            public override void Play()
            {
                _instance.Play();
            }

            /// <inheritdoc />
            public override void Dispose()
            {
                _instance.Dispose();
            }

            

            /// <inheritdoc />
            public override AudioType Type => AudioType.SoundEffect;

            public override string Name { get; }

            /// <inheritdoc />
            public override float Volume
            {
                get => _instance.Volume;
                set => _instance.Volume = value;
            }
            
        }

        private sealed class VarmintSongInstance : SoundInstanceBase
        {
            private readonly Song _song;

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

            /// <inheritdoc />
            /// <summary>
            /// Throws an exception if this is not the current song.
            /// </summary>
            public override void Stop()
            {
                if (this != CurrentSong)
                    return;
                CurrentSong = null;
                MediaPlayer.Stop();
            }

            /// <inheritdoc />
            /// <summary>
            /// Throws an exception if this is not the current song.
            /// </summary>
            public override void Pause()
            {
                if (this != CurrentSong)
                    return;
                MediaPlayer.Pause();
            }

            /// <inheritdoc />
            /// <summary>
            /// Throws an exception if this is not the current song.
            /// </summary>
            public override void Resume()
            {
                if (this != CurrentSong)
                    throw new InvalidOperationException("Attempted to resume a song when it is not the current song.");
                MediaPlayer.Resume();
            }

            /// <inheritdoc />
            /// <summary>
            /// Throws an exception if this is not the current song.
            /// </summary>
            public override void Play()
            {
                CurrentSong.Stop();
                CurrentSong = this;
                MediaPlayer.Play(_song);
            }

            /// <inheritdoc />
            public override bool IsLooping
            {
                get => MediaPlayer.IsRepeating;
                set => MediaPlayer.IsRepeating = value;
            }

            public override float Volume
            {
                get => MediaPlayer.Volume;
                set => MediaPlayer.Volume = value;
            }

            public override void Dispose()
            {
                Stop();
            }

            public override AudioType Type => AudioType.Song;
            public override string Name => _song
        }
#endregion
    }
}
