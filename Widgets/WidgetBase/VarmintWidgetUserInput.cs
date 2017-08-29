using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MonoVarmint.Widgets
{

    //--------------------------------------------------------------------------------------
    //
    //--------------------------------------------------------------------------------------
    public partial class VarmintWidget
    {
/*
        private DateTime _dragStartTime = DateTime.MaxValue;
        List<VarmintWidget> _recentDragWidgets = new List<VarmintWidget>();
*/
        private static VarmintWidget _focusedContol;

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Focus - set input focus on this control 
        /// </summary>
        //--------------------------------------------------------------------------------------
        public void Focus()
        {
            if (_focusedContol != null) _focusedContol.HasFocus = false;
            _focusedContol = this;
            _focusedContol.HasFocus = true;
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// HandleInputCharacter
        /// </summary>
        //--------------------------------------------------------------------------------------
        public void HandleInputCharacter(char c)
        {
            _focusedContol?.OnInputCharacter(c);
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// HitTest
        /// </summary>
        //--------------------------------------------------------------------------------------
        public IList<VarmintWidget> HitTest(Vector2 absolutePoint)
        {
            var hitList = new List<VarmintWidget>();
            HitTestInternal(absolutePoint, hitList);
            return hitList;
        }

        public float DragLengthThreshhold { get; set; } = .05f;
        public double FlickThreshholdSeconds { get; set; } = 0.5;
        public double DoubleTapIntervalSeconds { get; set; } = 0.25;
        public double ContextHoldThreshholdSeconds { get; set; } = 1;
        public double TouchOrphanTimeoutSeconds { get; set; } = 1.0;
        public float DoubleTapRadius { get; set; } = 0.1f;

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// HitTestInternal
        /// </summary>
        //--------------------------------------------------------------------------------------
        internal virtual void HitTestInternal(Vector2 absolutePoint, IList<VarmintWidget> hitList)
        {
            if (!IsVisible) return;
            if (PointIsInsideMe(absolutePoint))
            {
                hitList.Add(this);
            }

            foreach (var child in Children)
            {
                child.HitTestInternal(absolutePoint, hitList);
            }
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// PointIsInsideMe
        /// </summary>
        //--------------------------------------------------------------------------------------
        public virtual bool PointIsInsideMe(Vector2 absolutePoint)
        {
            return absolutePoint.X > AbsoluteOffset.X
                && absolutePoint.X < AbsoluteOffset.X + Size.X
                && absolutePoint.Y > AbsoluteOffset.Y
                && absolutePoint.Y < AbsoluteOffset.Y + Size.Y;
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// HandleTap
        /// </summary>
        //--------------------------------------------------------------------------------------
        public EventHandledState HandleTap(Vector2 absoluteLocation)
        {
            if (!AllowInput || OnTap == null)
                return AllowInput ? EventHandledState.NotHandled : EventHandledState.Handled;
            var relativeLocation = absoluteLocation - AbsoluteOffset;
            return OnTap(this, relativeLocation);
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// HandleDoubleTap
        /// </summary>
        //--------------------------------------------------------------------------------------
        private EventHandledState HandleDoubleTap(Vector2 absoluteLocation)
        {
            if (!AllowInput || OnDoubleTap == null)
                return AllowInput ? EventHandledState.NotHandled : EventHandledState.Handled;
            var relativeLocation = absoluteLocation - AbsoluteOffset;
            return OnDoubleTap(this, relativeLocation);
        }

        private EventHandledState HandleContextTap(Vector2 absoluteLocation)
        {
            if (!AllowInput || OnContextTap == null)
                return AllowInput ? EventHandledState.NotHandled : EventHandledState.Handled;
            var relativeLocation = absoluteLocation - AbsoluteOffset;
            return OnContextTap(this, relativeLocation);
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// HandleFlick
        /// </summary>
        //--------------------------------------------------------------------------------------
        private EventHandledState HandleFlick(Vector2 absoluteStart, Vector2 delta)
        {
            if (AllowInput && OnFlick != null)
            {
                var relativeStartLocation = absoluteStart - this.AbsoluteOffset;
                return OnFlick(new VarmintFlickData() { SourceWidget = this, Location = relativeStartLocation, Delta = delta });
            }
            return AllowInput ? EventHandledState.NotHandled : EventHandledState.Handled;
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// HandleDrag
        /// </summary>
        //--------------------------------------------------------------------------------------
        private EventHandledState HandleDrag(Vector2 absoluteLocation, Vector2 delta)
        {
            if (!AllowInput || OnDrag == null)
                return AllowInput ? EventHandledState.NotHandled : EventHandledState.Handled;
            var relativeLocation = absoluteLocation - AbsoluteOffset;
            return OnDrag(this, relativeLocation, delta);
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// HandleDragComplete
        /// </summary>
        //--------------------------------------------------------------------------------------
        private EventHandledState HandleDragComplete()
        {
            if (AllowInput && OnDragComplete != null) return OnDragComplete();
            return AllowInput ? EventHandledState.NotHandled : EventHandledState.Handled;
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// HandleDragCancel
        /// </summary>
        //--------------------------------------------------------------------------------------
        private EventHandledState HandleDragCancel()
        {
            if (OnDragCancel != null) return OnDragCancel();
            return EventHandledState.Handled;
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// HandleTouchUp
        /// </summary>
        //--------------------------------------------------------------------------------------
        private EventHandledState HandleTouchUp(TouchLocation absoluteTouch)
        {
            if (!AllowInput || OnTouchUp == null)
                return AllowInput ? EventHandledState.NotHandled : EventHandledState.Handled;
            var relativeTouch = new TouchLocation(absoluteTouch.Id, absoluteTouch.State, absoluteTouch.Position - AbsoluteOffset);
            return OnTouchUp(this, relativeTouch);
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// HandleTouchDown
        /// </summary>
        //--------------------------------------------------------------------------------------
        private EventHandledState HandleTouchDown(TouchLocation absoluteTouch)
        {
            if (!AllowInput || OnTouchDown == null)
                return AllowInput ? EventHandledState.NotHandled : EventHandledState.Handled;
            var relativeTouch = new TouchLocation(absoluteTouch.Id, absoluteTouch.State, absoluteTouch.Position - AbsoluteOffset);
            return OnTouchDown(this, relativeTouch);
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// HandleTouchMove
        /// </summary>
        //--------------------------------------------------------------------------------------
        private EventHandledState HandleTouchMove(
            TouchMoveType moveType,
            TouchLocation thisTouch,
            TouchLocation previousTouch)
        {
            if (!AllowInput || OnTouchMove == null)
                return AllowInput ? EventHandledState.NotHandled : EventHandledState.Handled;
            var relativeTouch = new TouchLocation(thisTouch.Id, thisTouch.State, thisTouch.Position - AbsoluteOffset);
            var relativePreviousTouch = new TouchLocation(previousTouch.Id, previousTouch.State, previousTouch.Position - AbsoluteOffset);
            return OnTouchMove(this, moveType, relativeTouch, relativePreviousTouch);
        }

        private readonly List<TouchMemory> _currentTouches = new List<TouchMemory>();

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// ReportTouch - This communicates raw touch events to controls and remembers gestures
        /// </summary>
        //--------------------------------------------------------------------------------------
        public void ReportTouch(TouchLocation touch, GameTime gameTime)
        {
            TouchMemory touchMemory = null;
            var touchIndex = -1;
            for(var i = 0; i < _currentTouches.Count; i++)
            {
                var trackedTouch = _currentTouches[i];
                if (trackedTouch.FirstTouch.Id != touch.Id) continue;
                touchMemory = trackedTouch;
                touchIndex = i;
                break;
            }

            if(touchMemory == null)
            {
                touchMemory = new TouchMemory {TouchStartTime = gameTime};
                touchIndex = _currentTouches.Count;
                _currentTouches.Add(touchMemory);
            }

            touchMemory.LastUpdateTime = gameTime;

            // If this is a tracked touch, but there is no movement,
            // then do nothing.
            if (touch.State == TouchLocationState.Moved
                && touchMemory.TouchCount > 0
                && touchMemory.CurrentTouch.Position == touch.Position)
            {
                return;
            }

            touchMemory.AddTouch(touch);
            var hitList = HitTest(touch.Position);

            void ProcessHit(Func<VarmintWidget, EventHandledState> tryEvent)
            {
                for (var i = hitList.Count - 1; i >= 0; i--)
                {
                    if (tryEvent(hitList[i]) == EventHandledState.Handled) break;
                }
            }

            switch (touch.State)
            {
                case TouchLocationState.Invalid:
                case TouchLocationState.Released:
                    ProcessHit(w => w.HandleTouchUp(touch));

                    foreach (var widget in touchMemory.PreviousWidgets)
                    {
                        if(!widget.PointIsInsideMe(touch.Position))
                            widget.HandleTouchMove(TouchMoveType.Leave, touch, touchMemory.PreviousTouch);
                    }

                    if(touchMemory.TotalDistance > DragLengthThreshhold)
                    {
                        if (touchMemory.SecondsAfterStart(gameTime) < FlickThreshholdSeconds)
                        {
                            var location = touchMemory.FirstTouch.Position;
                            hitList = HitTest(location);
                            ProcessHit(w => w.HandleFlick(location, touchMemory.CurrentTouch.Position - location));
                        }
                        else
                        {
                            hitList = touchMemory.StartWidgets;
                            ProcessHit(w => w.HandleDragComplete());
                        }
                    }
                    else if(_tapInReserve != null)
                    {
                        var oldLocation = _tapInReserve.CurrentTouch.Position;
                        if((touch.Position - oldLocation).Length() < DoubleTapRadius)
                        {
                            ProcessHit(w => w.HandleDoubleTap(oldLocation));
                        }
                        else
                        {
                            ProcessHit(w => w.HandleTap(oldLocation));
                            ProcessHit(w => w.HandleTap(touch.Position));
                        }
                        _tapInReserve = null;
                    }
                    else
                    {
                        _tapInReserve = touchMemory;
                    }

                    _currentTouches.RemoveAt(touchIndex);

                    break;
                case TouchLocationState.Pressed:
                    ProcessHit(w =>
                    {
                        // Remember this widget was touched so we can process a leave correctly later.
                        touchMemory.AddTouchedWidget(w);
                        var returnValue = w.HandleTouchDown(touch);
                        if (returnValue == EventHandledState.Handled) w.Focus();
                        return returnValue;
                    });

                    foreach (var i in hitList)
                    {
                        touchMemory.AddStartWidget(i);
                    }

                    break;
                case TouchLocationState.Moved:
                    var previousControls = new List<VarmintWidget>(touchMemory.PreviousWidgets);
                    touchMemory.ClearPreviousWidgets();
                    ProcessHit(w =>
                    {
                        // If the old hitlist contains this widget, it's a move,  otherwise
                        // it's an enter
                        touchMemory.AddTouchedWidget(w);
                        var moveType = TouchMoveType.Enter;
                        if (!previousControls.Contains(w))
                            return w.HandleTouchMove(moveType, touch, touchMemory.PreviousTouch);
                        previousControls.Remove(w);
                        moveType = TouchMoveType.Move;
                        return w.HandleTouchMove(moveType, touch, touchMemory.PreviousTouch);
                    });

                    // Any controls left over?  The touch left those
                    foreach (var control in previousControls)
                    {
                        control.HandleTouchMove(TouchMoveType.Leave, touch, touchMemory.PreviousTouch);
                    }

                    break;
            }
        }

        private TouchMemory _tapInReserve;
        //--------------------------------------------------------------------------------------
        /// <summary>
        /// ResolveGestures - Touch events have to coordinated with other touches over time to
        ///                   generate the proper gestures and resolve ambiguity.  Call this on
        ///                   every frame to properly turn recent touches into gestures
        /// </summary>
        //--------------------------------------------------------------------------------------
        public void ResolveGestures(GameTime gameTime)
        {
            IList<VarmintWidget> hitList;

            void ProcessHit(Func<VarmintWidget, EventHandledState> tryEvent)
            {
                for (var i = hitList.Count - 1; i >= 0; i--)
                {
                    if (tryEvent(hitList[i]) == EventHandledState.Handled) break;
                }
            }

            // Handle orphaned touches
            for(var i = 0; i < _currentTouches.Count; )
            {
                var touchMemory = _currentTouches[i];
                if ((gameTime.TotalGameTime - touchMemory.LastUpdateTime.TotalGameTime).TotalSeconds > TouchOrphanTimeoutSeconds)
                {
                    if(touchMemory.GestureType == GestureType.FreeDrag)
                    {
                        hitList = touchMemory.StartWidgets;
                        ProcessHit(w => w.HandleDragCancel());
                    }
                    Debug.WriteLine("BYE");
                    _currentTouches.RemoveAt(i);
                    continue;
                }
                i++;
            }

            if (_tapInReserve != null)
            {
                if ( _tapInReserve.SecondsAfterStart(gameTime) >= DoubleTapIntervalSeconds)
                {
                    var tapLocation = _tapInReserve.CurrentTouch.Position;
                    hitList = HitTest(tapLocation);
                    ProcessHit(w => w.HandleTap(tapLocation));
                    _tapInReserve = null;
                }
            }

            // One touch could be dragging or holding
            if (_currentTouches.Count != 1) return;
            {
                var touchMemory = _currentTouches[0];

                if (touchMemory.UnresolvedCount > 0
                    && touchMemory.TotalDistance > DragLengthThreshhold
                )//&& touchMemory.SecondsAfterStart(gameTime) >= FlickThreshholdSeconds)
                {
                    touchMemory.GestureType = GestureType.FreeDrag;
                    hitList = touchMemory.StartWidgets;
                    ProcessHit(w => w.HandleDrag(touchMemory.CurrentTouch.Position,
                        touchMemory.CurrentTouch.Position - touchMemory.LastUnresolvedTouch.Position));
                    touchMemory.MarkResolved();
                }
                else if (touchMemory.TotalDistance < DragLengthThreshhold
                         && touchMemory.SecondsAfterStart(gameTime) >= ContextHoldThreshholdSeconds)
                {
                    hitList = touchMemory.StartWidgets;
                    // if we haven't started reporting as a hold, report as a hold
                    // and do a context tap
                    if (touchMemory.GestureType == GestureType.Hold) return;
                    ProcessHit(w => w.HandleContextTap(touchMemory.CurrentTouch.Position));
                    touchMemory.GestureType = GestureType.Hold;
                    // TODO: Do a haptic vibrate here, using a native method in the game runner
                    // TODO: Process extended hold events here when needed
                }
            }
        }
    }
}
