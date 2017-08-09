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
        private readonly double _animationDurationSeconds;
        private double _animationProgressSeconds;
        protected Action<double> Animate;
        

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
        /// ctor
        /// 
        /// durationSeconds - Set this to zero for infinite animation loops.  The delta in the 
        ///                   loop will be elapsed seconds.
        /// animator - method to do the animation work
        /// </summary>
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
            _animationProgressSeconds = 0;
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Update - calculates the animation delta and calls the attached animation code
        /// </summary>
        //--------------------------------------------------------------------------------------
        internal void Update(GameTime gameTime)
        {
            if (IsComplete) return;
            _animationProgressSeconds += gameTime.ElapsedGameTime.TotalSeconds;
            var delta = _animationProgressSeconds;
            if (_animationDurationSeconds > 0)
            {
                if (_animationProgressSeconds > _animationDurationSeconds)
                {
                    _animationProgressSeconds = _animationDurationSeconds;
                    OnComplete?.Invoke();
                    IsComplete = true;
                    return;
                }
                delta = _animationProgressSeconds / _animationDurationSeconds;
            }

            Animate(delta);
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Finish - call this to force an animation to complete
        /// </summary>
        //--------------------------------------------------------------------------------------
        internal void Finish(VarmintWidget widget)
        {
            if (_animationDurationSeconds > 0) Update(new GameTime(TimeSpan.MaxValue, TimeSpan.MaxValue));
            else
            {
                OnComplete?.Invoke();
                IsComplete = true;
            }
        }
    }
}
