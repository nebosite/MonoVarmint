using Microsoft.Xna.Framework;
using System;

namespace MonoVarmint.Widgets
{
    //--------------------------------------------------------------------------------------
    /// <summary>
    /// VarmintAnimation -       simple class for providing animations.  This class handles
    ///                          thinking about the delta and then calls out to the subclass
    ///                          to actually perform the animation
    /// </summary>
    //--------------------------------------------------------------------------------------
    public class VarmintAnimation
    {
        private readonly double? _animationDurationSeconds;
        private TimeSpan? _animationStartTime;

        /// <summary>
        /// Represents an action to take when animating
        /// </summary>
        /// <param name="delta">The number of seconds for infinite durations, the progress from 0 to 1 for finite durations</param>
        protected delegate void AnimateAction(double delta);

        protected AnimateAction Animate;
        
        /// <summary>
        /// IsComplete
        /// </summary>
        public bool IsComplete { get; internal set; }

        /// <summary>
        /// OnComplete
        /// </summary>
        public event Action OnComplete;

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Instantiates a new instance of the VarmintAnimation class with infinite duration
        /// </summary>
        //--------------------------------------------------------------------------------------
        public VarmintAnimation()
        {
            _animationDurationSeconds = null;
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Instantiates a new instance of the VarmintAnimation class with the given duration
        /// </summary>
        /// <param name="durationSeconds">The number of seconds the animation will last</param>
        //--------------------------------------------------------------------------------------
        public VarmintAnimation(double durationSeconds)
        {
            _animationDurationSeconds = durationSeconds;
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Reset - put defaults back and clear any attached events
        /// </summary>
        //--------------------------------------------------------------------------------------
        public void Reset()
        {
            if(OnComplete != null)
            {
                foreach(Action handler in OnComplete.GetInvocationList())
                {
                    OnComplete -= handler;
                }
            }

            IsComplete = false;
            _animationStartTime = null;
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Update - calculates the animation delta and calls the attached animation code
        /// </summary>
        //--------------------------------------------------------------------------------------
        internal void Update(GameTime gameTime)
        {
            if (IsComplete) return;
            if (_animationStartTime == null) _animationStartTime = gameTime.TotalGameTime;
            var animationProgressSeconds = (gameTime.TotalGameTime - _animationStartTime.Value).TotalSeconds;
            if (_animationDurationSeconds != null)
            {
                Animate(animationProgressSeconds / _animationDurationSeconds.Value);
                if (animationProgressSeconds >= _animationDurationSeconds)
                {
                    OnComplete?.Invoke();
                    IsComplete = true;
                }
            }
            else
            {
                Animate(animationProgressSeconds);
            }
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Finish - call this to force an animation to complete
        /// </summary>
        //--------------------------------------------------------------------------------------
        internal void Finish(VarmintWidget widget)
        {
            if (_animationDurationSeconds != null) Update(new GameTime(TimeSpan.MaxValue, TimeSpan.MaxValue));
            else
            {
                OnComplete?.Invoke();
                IsComplete = true;
            }
        }
    }
}
