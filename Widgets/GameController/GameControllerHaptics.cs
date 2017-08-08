using System;

namespace MonoVarmint.Widgets
{
    public partial class GameController
    {
        public Action<long> OnVibrate;
        public void Vibrate(long milliseconds)
        {
            OnVibrate?.Invoke(milliseconds);
        }
    }
}