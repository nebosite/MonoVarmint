﻿using System;
using Microsoft.Xna.Framework;

namespace MonoVarmint.Widgets
{
    public partial class VarmintAudioAnimation : VarmintAnimation
    {
        public IVarmintAudioInstance Instance { get; private set; }
        public VarmintAudioAnimation(double durationSeconds, Action<IVarmintAudioInstance, double> animator) : base(durationSeconds)
        {
            Animate = delta => animator?.Invoke(Instance, delta);
        }

        internal void Update(IVarmintAudioInstance instance, GameTime gameTime)
        {
            Instance = instance;
            Update(gameTime);
        }
    }
}
