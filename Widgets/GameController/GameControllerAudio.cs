using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using System.Collections.Generic;

namespace MonoVarmint.Widgets
{
    public partial class GameController
    {

        public readonly List<VarmintWidgetAnimation> _audioAnimations = new List<VarmintWidgetAnimation>();

        private void AudioUpdate(GameTime gameTime)
        {
            foreach(var animation in new List<VarmintWidgetAnimation>( _audioAnimations))
            {
                animation.Update(null, gameTime);
                if (animation.IsComplete)
                    _audioAnimations.Remove(animation);
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

        /// <summary>
        /// Play a loaded sound effect by filename.
        /// If the sound is not loaded, attempt to load it.
        /// </summary>
        /// <param name="name">
        /// The filename to play.
        /// </param>
        /// <returns>
        /// True if the was played.
        /// </returns>
        public void PlaySoundEffect(string name)
        {
            var sfx = _soundEffectsByName[name].CreateInstance();
            sfx.Volume *= (float)SoundEffectVolume;
            sfx.Play();
        }

        public string CurrentSong { get; private set; }
        /// <summary>
        /// The current music playing. Set to null to stop playing.
        /// </summary>
	    public void PlaySong(string value)
        {

            if (CurrentSong == value) return;
            CurrentSong = value;
            if (value != null)
            {
                MediaPlayer.Play(_songsByName[value]);
            }
            else
            {
                MediaPlayer.Stop();
            }
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
                PlaySong(newSong);
            };
            _audioAnimations.Add(animation);
        }

        /// <summary>
        /// Pause the current music.
        /// </summary>
	    public static void Pause()
        {
            MediaPlayer.Pause();
        }

        /// <summary>
        /// Resume the current music.
        /// </summary>
	    public static void Play()
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
    }
}
