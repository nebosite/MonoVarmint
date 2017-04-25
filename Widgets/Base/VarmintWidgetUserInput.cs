using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace MonoVarmint.Widgets
{


    //--------------------------------------------------------------------------------------
    /// <summary>
    /// VarmintWidget - A very simple widget class for MonoGame
    /// </summary>
    //--------------------------------------------------------------------------------------
    public partial class VarmintWidget
    {
        public static float DragLengthThreshhold { get; set; }
        public static double FlickThreshholdSeconds { get; set; }

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
        private EventHandledState HandleTap(Vector2 absoluteLocation)
        {
            if (AllowInput && OnTap != null)
            {
                var relativeLocation = absoluteLocation - this.AbsoluteOffset;
                return OnTap(this, relativeLocation);
            }
            return AllowInput ? EventHandledState.NotHandled : EventHandledState.Handled;
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        ///         private EventHandledState HandleFlick(Vector2 absoluteLocation)

        /// </summary>
        //--------------------------------------------------------------------------------------
        private EventHandledState HandleFlick(Vector2 absoluteStart, Vector2 absoluteEnd)
        {
            if (AllowInput && OnFlick != null)
            {
                var relativeStartLocation = absoluteStart - this.AbsoluteOffset;
                var relativeEndLocation = absoluteEnd - this.AbsoluteOffset;
                return OnFlick(this, relativeStartLocation, relativeEndLocation);
            }
            return AllowInput ? EventHandledState.NotHandled : EventHandledState.Handled;
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// HandleDrag
        /// </summary>
        //--------------------------------------------------------------------------------------
        private EventHandledState HandleDrag(Vector2 position)
        {
            if (AllowInput && OnDrag != null) return OnDrag(this, position);
            return AllowInput ? EventHandledState.NotHandled : EventHandledState.Handled;
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
        private EventHandledState HandleTouchUp(TouchLocation touch)
        {
            if (AllowInput && OnTouchUp != null) return OnTouchUp(this, touch);
            return AllowInput ? EventHandledState.NotHandled : EventHandledState.Handled;
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// HandleTouchDown
        /// </summary>
        //--------------------------------------------------------------------------------------
        private EventHandledState HandleTouchDown(TouchLocation touch)
        {
            if (AllowInput && OnTouchDown != null) return OnTouchDown(this, touch);
            return AllowInput ? EventHandledState.NotHandled : EventHandledState.Handled;
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
            if (AllowInput && OnTouchMove != null)
            {
                return OnTouchMove(this, moveType, thisTouch, previousTouch);
            }
            return AllowInput ? EventHandledState.NotHandled : EventHandledState.Handled;
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// HandleGesture
        /// </summary>
        //--------------------------------------------------------------------------------------
        public void HandleGesture(
            GestureType gesture,
            Vector2 gestureLocation1,
            Vector2? gestureLocation2)
        {
            // Flicks don't preserve location data, so We'll do our best to fill it in.
            if (gesture == GestureType.Flick && _lastTouchDownLocation != null)
            {
                gestureLocation1 = _lastTouchDownLocation.Value;
                gestureLocation2 = _lastTouchUpLocation;
                if (gestureLocation2 == null) gestureLocation2 = _lastTouchMoveLocation;
            }
            if (gestureLocation2 == null) gestureLocation2 = gestureLocation1;

            var hitList = HitTest(gestureLocation1);

            switch (gesture)
            {
                case GestureType.Tap:
                    for (int i = hitList.Count - 1; i >= 0; i--)
                    {
                        if (hitList[i].HandleTap(gestureLocation1) == EventHandledState.Handled) break;
                    }
                    break;
                case GestureType.Flick:
                    for (int i = hitList.Count - 1; i >= 0; i--)
                    {
                        if (hitList[i].HandleFlick(gestureLocation1, gestureLocation2.Value) == EventHandledState.Handled) break;
                    }
                    break;
                case GestureType.FreeDrag:
                    if (_recentDragWidgets.Count == 0) _dragStartTime = DateTime.Now;
                    for (int i = hitList.Count - 1; i >= 0; i--)
                    {
                        if (hitList[i].HandleDrag(gestureLocation1) == EventHandledState.Handled)
                        {
                            if (_recentDragWidgets.Count == 0
                                || _recentDragWidgets[_recentDragWidgets.Count - 1] != hitList[i])
                            {
                                _recentDragWidgets.Add(hitList[i]);
                            }
                            break;
                        }
                    }
                    break;
                case GestureType.DragComplete:
                    _recentDragWidgets[_recentDragWidgets.Count - 1].HandleDragComplete();
                    _recentDragWidgets.Clear();
                    break;
                default:
                    break;
            }
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// CancelDrag
        /// </summary>
        //--------------------------------------------------------------------------------------
        void CancelDrag()
        {
            foreach (var widget in _recentDragWidgets)
            {
                widget.HandleDragCancel();
            }
            _recentDragWidgets.Clear();
        }

        Dictionary<int, TouchMemory> _trackedTouches = new Dictionary<int, TouchMemory>();
        Vector2? _lastTouchDownLocation;
        Vector2? _lastTouchUpLocation;
        Vector2? _lastTouchMoveLocation;

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// HandleTouch
        /// </summary>
        //--------------------------------------------------------------------------------------
        public void HandleTouch(TouchLocation touch)
        {

            if (!_trackedTouches.ContainsKey(touch.Id))
            {
                _trackedTouches[touch.Id] = new TouchMemory();
            }
            var memory = _trackedTouches[touch.Id];
            if(touch.State == TouchLocationState.Moved 
                && memory.TouchCount > 0
                && memory.CurrentTouch.Position == touch.Position)
            {
                return;
            }
            memory.AddTouch(touch);


            var hitList = HitTest(touch.Position);

            switch (touch.State)
            {
                case TouchLocationState.Invalid:
                case TouchLocationState.Released:
                    for (int i = hitList.Count - 1; i >= 0; i--)
                    {
                        if (hitList[i].HandleTouchUp(touch) == EventHandledState.Handled) break;
                    }

                    if (_trackedTouches.ContainsKey(touch.Id))
                    {
                        // Generate touch leave events in case there was no move event 
                        // between this touch up and the last touch down
                        var handled = new HashSet<VarmintWidget>();
                        foreach (var widget in memory.PreviousWidgets)
                        {
                            if (!handled.Contains(widget) && !widget.PointIsInsideMe(touch.Position))
                            {
                                handled.Add(widget);
                                widget.HandleTouchMove(TouchMoveType.Leave, touch, memory.PreviousTouch);
                            }
                        }
                        _lastTouchDownLocation = memory.PreviousTouch.Position;
                        _lastTouchUpLocation = touch.Position;

                        // Generate drag complete gesture if we went far enough
                        if(memory.PathLength > DragLengthThreshhold)
                        {
                            var flickLength = (memory.FirstTouch.Position - touch.Position).Length();
                            var dragSeconds = (DateTime.Now - _dragStartTime).TotalSeconds;
                            if (_recentDragWidgets.Count != 0)
                            {
                                HandleGesture(GestureType.DragComplete, touch.Position, null);
                            }
                            else if(dragSeconds < FlickThreshholdSeconds && flickLength > DragLengthThreshhold)
                            {
                                HandleGesture(GestureType.Flick, touch.Position, memory.FirstTouch.Position);
                            }
                            _dragStartTime = DateTime.MinValue;
                        }
                        // Otherwise, it's a tap
                        else
                        {
                            CancelDrag();
                            HandleGesture(GestureType.Tap, touch.Position, null);
                        }

                        memory.Dispose();
                        _trackedTouches.Remove(touch.Id);
                    }

                    break;
                case TouchLocationState.Pressed:
                    for (int i = hitList.Count - 1; i >= 0; i--)
                    {
                        memory.AddPreviousWidget(hitList[i]);
                        if (hitList[i].HandleTouchDown(touch) == EventHandledState.Handled) break;
                    }
                    _lastTouchDownLocation = memory.PreviousTouch.Position;
                    _lastTouchUpLocation = null;
                    _lastTouchMoveLocation = null;

                    break;
                case TouchLocationState.Moved:
                    var previousControls = new List<VarmintWidget>(memory.PreviousWidgets);
                    memory.ClearPreviousWidgets();
                    for (int i = hitList.Count - 1; i >= 0; i--)
                    {
                        memory.AddPreviousWidget(hitList[i]);
                        var moveType = TouchMoveType.Enter;
                        if (previousControls.Contains(hitList[i]))
                        {
                            previousControls.Remove(hitList[i]);
                            moveType = TouchMoveType.Move;
                        }
                        if (hitList[i].HandleTouchMove(moveType, touch, memory.PreviousTouch) == EventHandledState.Handled) break;
                    }
                    _lastTouchMoveLocation = touch.Position;

                    foreach (var control in previousControls)
                    {
                        control.HandleTouchMove(TouchMoveType.Leave, touch, memory.PreviousTouch);
                    }
                    HandleGesture(GestureType.FreeDrag, touch.Position, memory.PreviousTouch.Position);
                    
                    break;
                default:
                    break;
            }
        }
    }
}