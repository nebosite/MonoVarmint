using System;
using MonoVarmint.Widgets.Animation;

namespace MonoVarmint.Widgets
{
    public interface IVarmintAudioInstance : IDisposable
    {
        
        /// <summary>
        /// Stop this sound
        /// </summary>
        void Stop();
        
        /// <summary>
        /// Pause this sound
        /// </summary>
        void Pause();
        
        /// <summary>
        /// Resume this sound
        /// </summary>
        void Resume();
        
        /// <summary>
        /// Sets this sound to loop or not.
        /// </summary>
        bool IsLooping { get; set; }
        
        /// <summary>
        /// Plays this sound.
        /// </summary>
        void Play();
        
        /// <summary>
        /// The volume of the sound.
        /// </summary>
        float Volume { get; set; }
     
        /// <summary>
        /// Type of this sound: song or sound effect.
        /// </summary>
        AudioType Type { get; }
        
        string Name { get; }
        
    }

    public enum AudioType
    {
        Song,
        SoundEffect
    }
}
