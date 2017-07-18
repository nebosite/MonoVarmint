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
        double animationDurationSeconds = 0;
        double animationProgressSeconds = 0;
        protected Action<double> _animate;

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
            animationDurationSeconds = durationSeconds;
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
            animationProgressSeconds = 0;
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Update - calculates the animation delta and calls the attached animation code
        /// </summary>
        //--------------------------------------------------------------------------------------
        internal void Update(GameTime gameTime)
        {
            if (!IsComplete)
            {
                animationProgressSeconds += gameTime.ElapsedGameTime.TotalSeconds;
                var delta = animationProgressSeconds;
                if (animationDurationSeconds > 0)
                {
                    if (animationProgressSeconds > animationDurationSeconds)
                    {
                        animationProgressSeconds = animationDurationSeconds;
                        OnComplete?.Invoke();
                        IsComplete = true;
                    }
                    delta = (animationProgressSeconds / animationDurationSeconds);
                }

                _animate(delta);
            }
        }

        TimeSpan Forever = TimeSpan.FromDays(100000);
        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Finish - call this to force an animation to complete
        /// </summary>
        //--------------------------------------------------------------------------------------
        internal void Finish(VarmintWidget widget)
        {
            if (animationDurationSeconds > 0) Update(new GameTime(Forever, Forever));
            else
            {
                OnComplete?.Invoke();
                IsComplete = true;
            }
        }
    }

}
