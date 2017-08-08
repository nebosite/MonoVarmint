namespace MonoVarmint.Widgets
{
    public partial class GameController
    {
        public Func<long> OnVibrate;
        public void Vibrate(long milliseconds)
        {
            if(OnVibrate == null) return;
            OnVibrate();
        }
    }
}