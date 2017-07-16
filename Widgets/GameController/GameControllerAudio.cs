using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;

namespace MonoVarmint.Widgets
{
    public partial class GameController
    {

        public readonly List<VarmintWidgetAnimation> AudioAnimations = new List<VarmintWidgetAnimation>();

        private void AudioUpdate(GameTime gameTime)
        {
            foreach(var animation in new List<VarmintWidgetAnimation>( AudioAnimations))
            {
                animation.Update(null, gameTime);
                if (animation.IsComplete)
                    AudioAnimations.Remove(animation);
            }
        }

        public VarmintWidgetAnimation FadeMusic(double durationSeconds)
        {
            var startVolume = MusicVolume;
            return new VarmintWidgetAnimation(durationSeconds, (widget, delta) =>
            {
                MusicVolume = (1 - delta) * startVolume;   
            });
        }


        private VarmintAudioInstance PlaySoundEffect(string name)
        {
            var sfx = _soundEffectsByName[name].CreateInstance();
            sfx.Volume *= (float)SoundEffectVolume;
            sfx.Play();
            return new VarmintSoundEffectInstance(sfx);
        }

        public static string CurrentSong { get; private set; }

	    public VarmintAudioInstance PlaySound(string name)
        {
            VarmintAudioInstance instance;
            if (_soundEffectsByName.ContainsKey(name))
                instance = PlaySoundEffect(name);
            else if (_songsByName.ContainsKey(name))
                instance = PlaySong(name);
            else throw new ArgumentException($"No sound with the name {name} has been loaded.");
            return instance;
        }

        private VarmintAudioInstance PlaySong(string name)
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

        /// <summary>
		/// Fade out of a song and stop.
		/// </summary>
		/// <param name="outDuration"></param>
	    public void FadeoutMusic(double outDuration)
        {
            FadeToSong(outDuration, null);
        }
        /// <summary>
        /// Fade out the current song and play a new one.
        /// </summary>
        /// <param name="outDuration">
        /// Time in seconds of the fade out.
        /// </param>
        /// <param name="newSong">
        /// The song to play after the fade.
        /// </param>
        public void FadeToSong(double outDuration, string newSong)
        {
            var volume = MusicVolume;
            var animation = FadeMusic(outDuration);
            animation.OnComplete += () => {
                MusicVolume = volume;
                PlaySound(newSong);
            };
            AudioAnimations.Add(animation);
        }

        /// <summary>
        /// Pause the current music.
        /// </summary>
	    public static void PauseMusic()
        {
            MediaPlayer.Pause();
        }

        /// <summary>
        /// Resume the current music.
        /// </summary>
	    public static void ResumeMusic()
        {
            MediaPlayer.Resume();
        }

        /// <summary>
        /// Is the music repeating?
        /// </summary>
        public static bool Looping
        {
            set => MediaPlayer.IsRepeating = value;
            get => MediaPlayer.IsRepeating;
        }

#region classes

        private class VarmintSoundEffectInstance : VarmintAudioInstance
        {
            private readonly SoundEffectInstance _instance;
            public VarmintSoundEffectInstance(SoundEffectInstance instance) : base(false)
            {
                _instance = instance;
            }

            public override void Stop()
            {
                _instance.Stop();
            }

            public override void Pause()
            {
                _instance.Pause();
            }

            public override void Resume()
            {
                _instance.Resume();
            }

            public override bool IsLooping
            {
                get => _instance.IsLooped;
                set => _instance.IsLooped = value;
            }
            
            public override void Dispose()
            {
                _instance.Dispose();
                base.Dispose();
            }
        }

        private class VarmintSongInstance : VarmintAudioInstance
        {
            private readonly Song _song;

            public VarmintSongInstance(Song song) : base(true)
            {
                _song = song;
            }

            public override void Stop()
            {
                CurrentSong = null;
                MediaPlayer.Stop();
            }

            public override void Pause()
            {
                MediaPlayer.Pause();
            }

            public override void Resume()
            {
                MediaPlayer.Resume();
            }

            public override bool IsLooping
            {
                get => MediaPlayer.IsRepeating;
                set => MediaPlayer.IsRepeating = value;
            }

            public override void Dispose()
            {
                Stop();
            }
        }
#endregion
    }
}
