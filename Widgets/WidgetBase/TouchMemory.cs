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
    class TouchMemory : IDisposable
    {
        List<TouchLocation> _touches = GetTouchBuffer();
        List<VarmintWidget> _previousWidgets = new List<VarmintWidget>();

        /// <summary>
        /// Array of the controls this touch saw last update
        /// </summary>
        public VarmintWidget[] PreviousWidgets { get { return _previousWidgets.ToArray(); } }

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

        /// <summary>
        /// The last touch in the list
        /// </summary>
        public TouchLocation CurrentTouch
        {
            get
            {
                return _touches[_touches.Count - 1];
            }
        }

        /// <summary>
        /// The first touch in the list
        /// </summary>
        public TouchLocation FirstTouch
        {
            get
            {
                return _touches[0];
            }
        }

        /// <summary>
        /// number of touches in the list
        /// </summary>
        public int TouchCount
        {
            get
            {
                return _touches.Count;
            }
        }

        /// <summary>
        /// the total length of the path traced by all the touches
        /// </summary>
        public float PathLength
        {
            get
            {
                var totalLength = 0f;
                for(int i = 1; i < _touches.Count; i++)
                {
                    totalLength += (_touches[i].Position - _touches[i - 1].Position).Length();
                }
                return totalLength;
            }
        }

        static Stack<List<TouchLocation>> _touchBuffers = new Stack<List<TouchLocation>>();
        //--------------------------------------------------------------------------------------
        /// <summary>
        /// GetTouchBuffer - To relax pressure on the GC, we will try to recycle touch buffers 
        /// </summary>
        //--------------------------------------------------------------------------------------
        static List<TouchLocation> GetTouchBuffer()
        {
            if (_touchBuffers.Count > 0)
            {
                return _touchBuffers.Pop();
            }
            else
            {
                return new List<TouchLocation>();
            }
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// AddTouch - remember this touch
        /// </summary>
        //--------------------------------------------------------------------------------------
        internal void AddTouch(TouchLocation touch)
        {
            _touches.Add(touch);
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// AddPreviousWidget - remember this as a recent widget
        /// </summary>
        //--------------------------------------------------------------------------------------
        internal void AddPreviousWidget(VarmintWidget widget)
        {
            _previousWidgets.Add(widget);
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

        bool isDisposed = false;
        //--------------------------------------------------------------------------------------
        /// <summary>
        /// ToString
        /// </summary>
        //--------------------------------------------------------------------------------------
        public void Dispose()
        {
            if (isDisposed) return;
            isDisposed = true;
            _touches.Clear();
            _touchBuffers.Push(_touches);
            _touches = null;
        }
    }
}
