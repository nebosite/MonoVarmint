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
        
        private readonly List<(IVarmintAudioInstance, VarmintAudioAnimation)> _audioAnimations = new List<(IVarmintAudioInstance, VarmintAudioAnimation)>();

        private void AudioUpdate(GameTime gameTime)
        {
            foreach(var animation in new List<(IVarmintAudioInstance instance, VarmintAudioAnimation animation)>(_audioAnimations))
            {
                animation.animation.Update(animation.instance, gameTime);
                if (animation.animation.IsComplete)
                    _audioAnimations.Remove(animation);
            }
        }
        

        public void ApplyAnimation(IVarmintAudioInstance instance, VarmintAudioAnimation animation)
        {
            _audioAnimations.Add((instance, animation));
        }


        private IVarmintAudioInstance PlaySoundEffect(string name)
        {
            var sfx = _soundEffectsByName[name].CreateInstance();
            sfx.Volume *= (float)SoundEffectVolume;
            sfx.Play();
            return new VarmintSoundEffectInstance(sfx);
        }

        public static string CurrentSong { get; private set; }

        public IVarmintAudioInstance GetNewAudioInstance(string audioFileName)
        {
            if (_soundEffectsByName.ContainsKey(audioFileName))
                return new VarmintSoundEffectInstance(_soundEffectsByName[audioFileName].CreateInstance());
            if (_songsByName.ContainsKey(audioFileName))
                return new VarmintSongInstance(_songsByName[audioFileName]);
            throw new ArgumentException($"No sound with the name {audioFileName} has been loaded.");
        }
        
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
            CurrentSong = name;
            MediaPlayer.Play(_songsByName[name]);
            return new VarmintSongInstance(_songsByName[name]);
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

        private class VarmintSoundEffectInstance : IVarmintAudioInstance
        {
            private readonly SoundEffectInstance _instance;
            
            /// <summary>
            /// The constructor by default sets looping to false.
            /// </summary>
            /// <param name="instance">The SoundEffectInstance this object should represent</param>
            public VarmintSoundEffectInstance(SoundEffectInstance instance)
            {
                _instance = instance;
                IsLooping = false;
            }

            /// <inheritdoc />
            public void Stop()
            {
                _instance.Stop();
            }

            /// <inheritdoc />
            public void Pause()
            {
                _instance.Pause();
            }

            /// <inheritdoc />
            public void Resume()
            {
                _instance.Resume();
            }

            /// <inheritdoc />
            public bool IsLooping
            {
                get => _instance.IsLooped;
                set => _instance.IsLooped = value;
            }

            /// <inheritdoc />
            public void Play()
            {
                _instance.Play();
            }

            /// <inheritdoc />
            public void Dispose()
            {
                _instance.Dispose();
            }

            /// <inheritdoc />
            public AudioType Type => AudioType.SoundEffect;
            
            /// <inheritdoc />
            public float Volume
            {
                get => _instance.Volume;
                set => _instance.Volume = value;
            }
            
        }

        private class VarmintSongInstance : IVarmintAudioInstance
        {
            private readonly Song _song;

            /// <summary>
            /// The constructor by default sets looping to true.
            /// </summary>
            /// <param name="song">The Song this object should represent</param>
            public VarmintSongInstance(Song song)
            {
                _song = song;
                IsLooping = true;
            }

            /// <inheritdoc />
            /// <summary>
            /// Throws an exception if this is not the current song.
            /// </summary>
            public void Stop()
            {
                if (_song.Name != CurrentSong)
                    throw new InvalidOperationException("Attempted to stop a song when it is not the current song.");
                CurrentSong = null;
                MediaPlayer.Stop();
            }

            /// <inheritdoc />
            /// <summary>
            /// Throws an exception if this is not the current song.
            /// </summary>
            public void Pause()
            {
                if (_song.Name != CurrentSong)
                    throw new InvalidOperationException("Attempted to pause a song when it is not the current song.");
                MediaPlayer.Pause();
            }

            /// <inheritdoc />
            /// <summary>
            /// Throws an exception if this is not the current song.
            /// </summary>
            public void Resume()
            {
                if (_song.Name != CurrentSong)
                    throw new InvalidOperationException("Attempted to resume a song when it is not the current song.");
                MediaPlayer.Resume();
            }

            /// <inheritdoc />
            /// <summary>
            /// Throws an exception if this is not the current song.
            /// </summary>
            public void Play()
            {
                if (CurrentSong != null)
                    throw new InvalidOperationException("Cannnot play a song while a different song is playing.");
                CurrentSong = _song.Name;
                MediaPlayer.Play(_song);
            }

            /// <inheritdoc />
            public bool IsLooping
            {
                get => MediaPlayer.IsRepeating;
                set => MediaPlayer.IsRepeating = value;
            }

            public float Volume
            {
                get => MediaPlayer.Volume;
                set => MediaPlayer.Volume = value;
            }

            public void Dispose()
            {
                Stop();
            }
            
            public AudioType Type => AudioType.Song;
        }
#endregion
    }
}
