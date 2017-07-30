namespace MonoVarmint.Widgets.Animation
{
    public partial class VarmintAudioAnimation
    {
        public static VarmintAudioAnimation TransitionVolume(double duration, float startVolume, float finalVolume)
        {
            if (startVolume == finalVolume)
            {
                return new VarmintAudioAnimation(0, (instance, delta) => { });
            }
            return new VarmintAudioAnimation(duration, (instance, delta) =>
                instance.Volume = (float) (startVolume - delta / (startVolume - finalVolume)));
        }


    }
}
