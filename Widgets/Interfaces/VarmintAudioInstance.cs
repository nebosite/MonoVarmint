using System;

namespace MonoVarmint.Widgets
{
    public abstract class VarmintAudioInstance : IDisposable
    {
        public abstract void Stop();
        public abstract void Pause();
        public abstract void Resume();
        public abstract bool IsLooping { get; set; }

        public virtual void Dispose()
        {
            IsDisposed = true;
        }
        
        public bool IsDisposed { get; protected set; }
        
        public bool IsSong { get; }

        
        
        protected VarmintAudioInstance(bool isSong)
        {
            IsSong = isSong;
        }
        
        
    }
}
