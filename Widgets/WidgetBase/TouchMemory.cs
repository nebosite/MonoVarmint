using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;

namespace MonoVarmint.Widgets
{
    //--------------------------------------------------------------------------------------
    /// <summary>
    /// Helper for remembering touch details
    /// </summary>
    //--------------------------------------------------------------------------------------
    internal class TouchMemory : IDisposable
    {
        private List<TouchLocation> _touches = GetTouchBuffer();
        private readonly List<VarmintWidget> _previousWidgets = new List<VarmintWidget>();
        private readonly List<VarmintWidget> _startWidgets = new List<VarmintWidget>();
        private int _lastUnresolvedTouchIndex;

        /// <summary>
        /// Array of the controls this touch saw last update
        /// </summary>
        public VarmintWidget[] PreviousWidgets => _previousWidgets.ToArray();

        /// <summary>
        /// Array of the controls this touch started with
        /// </summary>
        public VarmintWidget[] StartWidgets => _startWidgets.ToArray();

        /// <summary>
        /// LastUpdateTime
        /// </summary>
        private readonly GameTime _touchStartTime = new GameTime();
        public GameTime TouchStartTime
        {
            get => _touchStartTime;
            set
            {
                _touchStartTime.TotalGameTime = value.TotalGameTime;
                _touchStartTime.ElapsedGameTime = value.ElapsedGameTime;
            }
        }

        private readonly GameTime _lastUpdateTime = new GameTime();
        public GameTime LastUpdateTime
        {
            get => _lastUpdateTime;
            set
            {
                _lastUpdateTime.TotalGameTime = value.TotalGameTime;
                _lastUpdateTime.ElapsedGameTime = value.ElapsedGameTime;
            }
        }

        public float TotalDistance { get; set; }

        public GestureType GestureType { get; set; }

        /// <summary>
        /// The touch just before the current touch in memory
        /// </summary>
        public TouchLocation PreviousTouch
        {
            get
            {
                if (_touches.Count < 2) return _touches[0];
                return _touches[_touches.Count - 2];
            }
        }
        public TouchLocation LastUnresolvedTouch => _touches[_lastUnresolvedTouchIndex];

        public int UnresolvedCount => _touches.Count - 1 - _lastUnresolvedTouchIndex;

        /// <summary>
        /// The last touch in the list
        /// </summary>
        public TouchLocation CurrentTouch => _touches[_touches.Count - 1];

        /// <summary>
        /// The first touch in the list
        /// </summary>
        public TouchLocation FirstTouch => _touches[0];

        /// <summary>
        /// number of touches in the list
        /// </summary>
        public int TouchCount => _touches.Count;

        /// <summary>
        /// the total length of the path traced by all the touches
        /// </summary>
        public float PathLength
        {
            get
            {
                var totalLength = 0f;
                for(var i = 1; i < _touches.Count; i++)
                {
                    totalLength += (_touches[i].Position - _touches[i - 1].Position).Length();
                }
                return totalLength;
            }
        }

        private static readonly Stack<List<TouchLocation>> TouchBuffers = new Stack<List<TouchLocation>>();
        //--------------------------------------------------------------------------------------
        /// <summary>
        /// GetTouchBuffer - To relax pressure on the GC, we will try to recycle touch buffers 
        /// </summary>
        //--------------------------------------------------------------------------------------
        private static List<TouchLocation> GetTouchBuffer()
        {
            return TouchBuffers.Count > 0 ? TouchBuffers.Pop() : new List<TouchLocation>();
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// ctor
        /// </summary>
        //--------------------------------------------------------------------------------------
        public TouchMemory()
        {
            GestureType = GestureType.None;
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// AddTouch - remember this touch
        /// </summary>
        //--------------------------------------------------------------------------------------
        internal void AddTouch(TouchLocation touch)
        {
            if (_touches.Count > 0) TotalDistance += (CurrentTouch.Position - touch.Position).Length();
            _touches.Add(touch);
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// AddPreviousWidget - remember this as a recent widget
        /// </summary>
        //--------------------------------------------------------------------------------------
        internal void AddTouchedWidget(VarmintWidget widget)
        {
            _previousWidgets.Add(widget);
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// AddStartWidget - remember this as an initially touched widget
        /// </summary>
        //--------------------------------------------------------------------------------------
        internal void AddStartWidget(VarmintWidget widget)
        {
            _startWidgets.Add(widget);
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// ClearPreviousWidgets - forget about recent widgets
        /// </summary>
        //--------------------------------------------------------------------------------------
        internal void ClearPreviousWidgets()
        {
            _previousWidgets.Clear();
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// ToString
        /// </summary>
        //--------------------------------------------------------------------------------------
        public override string ToString()
        {
            return "T:" + _touches.Count + " P:" + _previousWidgets.Count;
        }

        private bool _isDisposed;
        //--------------------------------------------------------------------------------------
        /// <summary>
        /// ToString
        /// </summary>
        //--------------------------------------------------------------------------------------
        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            _touches.Clear();
            TouchBuffers.Push(_touches);
            _touches = null;
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// SecondsAfterLastUpdate
        /// </summary>
        //--------------------------------------------------------------------------------------
        internal double SecondsAfterLastUpdate(GameTime gameTime)
        {
            return (gameTime.TotalGameTime - LastUpdateTime.TotalGameTime).TotalSeconds;
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// SecondsAfterStart
        /// </summary>
        //--------------------------------------------------------------------------------------
        internal double SecondsAfterStart(GameTime gameTime)
        {
            return (gameTime.TotalGameTime - TouchStartTime.TotalGameTime).TotalSeconds;
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// MarkResolved - Signal that all the known touches have been inspected
        /// </summary>
        //--------------------------------------------------------------------------------------
        public void MarkResolved()
        {
            _lastUnresolvedTouchIndex = _touches.Count - 1;
        }
    }
}
