namespace MonoVarmint.Widgets.Animation
{
    public partial class VarmintAudioAnimation
    {
        public static VarmintAudioAnimation TransitionVolume(float startVolume, float finalVolume, double duration)
        {
            return new VarmintAudioAnimation(duration, (instance, delta) =>
                instance.Volume = (float) (startVolume - (startVolume - finalVolume) / delta));
        }

        public static VarmintAudioAnimation TransitionWithFadeOut(float startVolume, IVarmintAudioInstance newInstance,
            double duration)
        {
            var animation = TransitionVolume(startVolume, 0, duration);
            animation.OnComplete += () =>
            {
                animation.Instance.Stop();
                newInstance.Play();
            };
            return animation;
        }
    }
}
