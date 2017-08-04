using System;
using Microsoft.Xna.Framework;

namespace MonoVarmint.Widgets.Animation
{
    public partial class VarmintAudioAnimation : VarmintAnimation
    {
        public IVarmintAudioInstance Instance { get; private set; }
        public VarmintAudioAnimation(double durationSeconds, Action<IVarmintAudioInstance, double> animator) : base(durationSeconds)
        {
            Animate = delta => animator(Instance, delta);
        }

        internal void Update(IVarmintAudioInstance instance, GameTime gameTime)
        {
            Instance = instance;
            Update(gameTime);
        }
    }
}
