namespace MonoVarmint.Widgets.Animation
{
    public partial class VarmintAudioAnimation
    {
        public static VarmintAudioAnimation SetVolume(float startVolume, float finalVolume, double duration)
        {
            return new VarmintAudioAnimation(duration, (instance, delta) => instance.Volume = (float) (startVolume - (startVolume - finalVolume) / delta));
        }

        public static VarmintAudioAnimation FadeOut(float startVolume, double duration)
        {
            return SetVolume(startVolume, 0, duration);
        }

        public static VarmintAudioAnimation TransitionWithFadeOut(float startVolume, IVarmintAudioInstance newInstance,
            double duration)
        {
            var animation = FadeOut(startVolume, duration);
            animation.OnComplete += () =>
            {
                animation.Instance.Stop();
                newInstance.Play();
            };
            return animation;
        }
    }
}
