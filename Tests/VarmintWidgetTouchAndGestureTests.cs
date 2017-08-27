using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonoVarmint.Tools;
using MonoVarmint.Widgets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using System.Collections.Generic;
using System.Threading;

namespace MonoVarmint.Tools.Tests
{
    [TestClass]
    public class VarmintWidgetTouchAndGestureTests
    {
        class TouchTester : VarmintWidget
        {
            public List<string> Calls = new List<string>();
            public bool PassThrough = false;

            public TouchTester()
            {
                // Touches
                OnTouchDown += (w, t) => Report("OnTouchDown(" + t.Id + ":" + t.Position +  ")");
                OnTouchUp += (w, t) => Report("OnTouchUp(" + t.Id + ":" + t.Position + ")");
                OnTouchMove += (w, mt, t1, t2) => Report("OnTouchMove(" + mt + " , " + t1.Id + " , " + t1.Position + " , " + t2.Position + ")");

                // Gestures
                OnTap += (w, pos) => Report("OnTap(" + pos + ")");
                OnDoubleTap += (w, p) => Report("OnDoubleTap(" + p + ")");
                OnFlick += (fd) => Report("OnFlick(" + fd.Location + " , " + fd.Delta + ")");
                OnDrag += (w, p1, p2) => Report("OnDrag(" + p1 + " , " + p2 + ")");
                OnDragComplete += () => Report("OnDragComplete");
                OnDragCancel += () => Report("OnDragCancel");
                OnPinch += (w, t, r, s) => Report("OnPinch(" + t + " , " + r + " , " + s + ")");
                OnPinchComplete += () => Report("OnPinchComplete");
            }

            EventHandledState Report(string text)
            {
                Calls.Add(text);
                return PassThrough ? EventHandledState.NotHandled : EventHandledState.Handled;
            }

            public void AssertLast(int callCount, string expectedCallText)
            {
                Assert.AreEqual(callCount, Calls.Count, "Bad number of calls");
                Assert.AreEqual(expectedCallText, Calls[callCount - 1]);
            }
        }

        [TestMethod]
        public void QuickLongDrag_CausesFlick()
        {
            var target = new TouchTester() { Size = new Vector2(10, 10) };
            var child1 = new TouchTester() { Size = new Vector2(5, 5), Offset = new Vector2(1, 1) };
            target.AddChild(child1);

            var t = new GameTime();
            target.ReportTouch(new TouchLocation(4, TouchLocationState.Pressed, new Vector2(4, 4)), t);
            target.ResolveGestures(t);
            target.ReportTouch(new TouchLocation(4, TouchLocationState.Released, new Vector2(6f, 4f)), t);
            target.ResolveGestures(t);
            child1.AssertLast(3, "OnFlick({X:3 Y:3} , {X:2 Y:0})");

            // Again with a move
            target = new TouchTester() { Size = new Vector2(10, 10) };
            child1 = new TouchTester() { Size = new Vector2(5, 5), Offset = new Vector2(1, 1) };
            target.AddChild(child1);
            target.ReportTouch(new TouchLocation(4, TouchLocationState.Pressed, new Vector2(4, 4)), t);
            target.ResolveGestures(t);
            target.ReportTouch(new TouchLocation(4, TouchLocationState.Moved, new Vector2(6f, 4f)), t);
            target.ResolveGestures(t);
            target.ReportTouch(new TouchLocation(4, TouchLocationState.Released, new Vector2(6f, 4.5f)), t);
            target.ResolveGestures(t);
            Assert.AreEqual("OnDrag({X:5 Y:3} , {X:2 Y:0})", child1.Calls[2]); // Should also have a drag
            child1.AssertLast(4, "OnFlick({X:3 Y:3} , {X:2 Y:0.5})");
        }


        [TestMethod]
        public void Dragging_Cancels_OnTimeoutOrInvalidTouch()
        {
            var target = new TouchTester() { Size = new Vector2(10, 10) };
            var child1 = new TouchTester() { Size = new Vector2(5, 5), Offset = new Vector2(1, 1) };
            target.AddChild(child1);

            var t = new GameTime();
            target.ReportTouch(new TouchLocation(4, TouchLocationState.Pressed, new Vector2(4, 4)),t);
            target.ResolveGestures(t);
            t.TotalGameTime = TimeSpan.FromSeconds(target.FlickThreshholdSeconds);
            target.ReportTouch(new TouchLocation(4, TouchLocationState.Moved, new Vector2(6f, 4f)),t);
            target.ResolveGestures(t);
            child1.AssertLast(3, "OnDrag({X:5 Y:3} , {X:2 Y:0})");

            t.TotalGameTime = TimeSpan.FromSeconds(100);
            target.ResolveGestures(t);
            child1.AssertLast(4, "OnDragCancel");
        }

        [TestMethod]
        public void Dragging_KicksOff_WhenFarEnough()
        {
            var target = new TouchTester() { Size = new Vector2(10, 10) };
            var child1 = new TouchTester() { Size = new Vector2(5, 5), Offset = new Vector2(1, 1), PassThrough = true };
            var child2 = new TouchTester() { Size = new Vector2(5, 5), Offset = new Vector2(2, 2) };
            var child3 = new TouchTester() { Size = new Vector2(5, 5), Offset = new Vector2(3, 3) };
            target.AddChild(child3);
            target.AddChild(child2);
            target.AddChild(child1);

            var t = new GameTime();
            target.ReportTouch(new TouchLocation(4, TouchLocationState.Pressed, new Vector2(4, 4)), t);
            target.ResolveGestures(t);
            target.ReportTouch(new TouchLocation(4, TouchLocationState.Moved, new Vector2(4.01f, 4f)), t);
            target.ResolveGestures(t);
            t.TotalGameTime = TimeSpan.FromSeconds(target.FlickThreshholdSeconds);
            child1.AssertLast(2, "OnTouchMove(Move , 4 , {X:3.01 Y:3} , {X:3 Y:3})");
            child2.AssertLast(2, "OnTouchMove(Move , 4 , {X:2.01 Y:2} , {X:2 Y:2})");
            Assert.AreEqual(0, child3.Calls.Count);
            target.ReportTouch(new TouchLocation(4, TouchLocationState.Moved, new Vector2(5, 4)), t);
            target.ResolveGestures(t);
            child1.AssertLast(4, "OnDrag({X:4 Y:3} , {X:1 Y:0})");
            child2.AssertLast(4, "OnDrag({X:3 Y:2} , {X:1 Y:0})");
            Assert.AreEqual(0, child3.Calls.Count);

            // Don't re-report moves
            target.ResolveGestures(t);
            child1.AssertLast(4, "OnDrag({X:4 Y:3} , {X:1 Y:0})");

            // Drag down
            target.ReportTouch(new TouchLocation(4, TouchLocationState.Moved, new Vector2(5, 5)), t);
            target.ResolveGestures(t);
            child1.AssertLast(6, "OnDrag({X:4 Y:4} , {X:0 Y:1})");
            child2.AssertLast(6, "OnDrag({X:3 Y:3} , {X:0 Y:1})");

            // Release the drag
            target.ReportTouch(new TouchLocation(4, TouchLocationState.Released, new Vector2(5, 5)), t);
            target.ResolveGestures(t);
            child1.AssertLast(8, "OnDragComplete");
            child2.AssertLast(8, "OnDragComplete");
            Assert.AreEqual(0, child3.Calls.Count);
        }

        [TestMethod]
        public void LongSlowStationaryTouch_GeneratesTap()
        {
            var target = new TouchTester() { Size = new Vector2(10, 10) };
            var child = new TouchTester() { Size = new Vector2(5, 5), Offset = new Vector2(1, 1) };
            target.AddChild(child);

            var t = new GameTime();
            target.ReportTouch(new TouchLocation(4, TouchLocationState.Pressed, new Vector2(2, 2)), t);
            t.TotalGameTime = TimeSpan.FromSeconds(1);
            target.ReportTouch(new TouchLocation(4, TouchLocationState.Released, new Vector2(2.01f, 2.01f)), t);
            target.ResolveGestures(t);
            child.AssertLast(3, "OnTap({X:1.01 Y:1.01})");
        }

        [TestMethod]
        public void Touch_GeneratesTwoTapsInsteadOfDouble_WhenFarApart()
        {
            var target = new TouchTester() { Size = new Vector2(10, 10) };
            var child = new TouchTester() { Size = new Vector2(5, 5), Offset = new Vector2(1, 1) };
            target.AddChild(child);

            var t = new GameTime();
            target.ReportTouch(new TouchLocation(5, TouchLocationState.Pressed, new Vector2(2, 2)), t);
            target.ReportTouch(new TouchLocation(5, TouchLocationState.Released, new Vector2(2, 2)), t);
            target.ReportTouch(new TouchLocation(5, TouchLocationState.Pressed, new Vector2(4, 4)),t);
            target.ReportTouch(new TouchLocation(5, TouchLocationState.Released, new Vector2(4, 4)), t);
            t.TotalGameTime = TimeSpan.FromSeconds(1);
            target.ResolveGestures(t);
            child.AssertLast(6, "OnTap({X:3 Y:3})");
            Assert.AreEqual("OnTap({X:1 Y:1})", child.Calls[4]);
        }

        [TestMethod]
        public void TouchSingleDownUp_GeneratesTap_AfterDoubleTapTimeout()
        {
            var target = new TouchTester() { Size = new Vector2(10, 10) };
            var child = new TouchTester() { Size = new Vector2(5, 5), Offset = new Vector2(1,1) };
            target.AddChild(child);

            var t = new GameTime();
            target.ReportTouch(new TouchLocation(5, TouchLocationState.Pressed, new Vector2(4, 4)), t);
            target.ReportTouch(new TouchLocation(5, TouchLocationState.Released, new Vector2(4, 4)), t);
            target.ResolveGestures(t);
            child.AssertLast(2, "OnTouchUp(5:{X:3 Y:3})");

            t.TotalGameTime = TimeSpan.FromSeconds(target.DoubleTapIntervalSeconds);
            target.ResolveGestures(t);
            child.AssertLast(3, "OnTap({X:3 Y:3})");

            // an immediate second tap should not generate more onTap calls
            target.ReportTouch(new TouchLocation(5, TouchLocationState.Pressed, new Vector2(4, 4)), t);
            target.ResolveGestures(t);
            child.AssertLast(4, "OnTouchDown(5:{X:3 Y:3})");
            target.ReportTouch(new TouchLocation(5, TouchLocationState.Released, new Vector2(4, 4)), t);
            target.ResolveGestures(t);
            child.AssertLast(5, "OnTouchUp(5:{X:3 Y:3})");

            // Another immediate tap should generate a doubleTap
            target.ReportTouch(new TouchLocation(5, TouchLocationState.Pressed, new Vector2(4, 4)), t);
            target.ResolveGestures(t);
            child.AssertLast(6, "OnTouchDown(5:{X:3 Y:3})");
            target.ReportTouch(new TouchLocation(5, TouchLocationState.Released, new Vector2(4, 4)), t);
            child.AssertLast(8, "OnDoubleTap({X:3 Y:3})");
        }

        [TestMethod]
        public void SimpleTouchDownAndUp()
        {
            var target = new TouchTester() { Size = new Vector2(10, 10) };
            var child = new TouchTester() { Size = new Vector2(3, 3) };
            target.AddChild(child);

            var t = new GameTime();
            target.ReportTouch(new TouchLocation(44, TouchLocationState.Pressed, new Vector2(1, 1)), t);
            Assert.AreEqual(1, child.Calls.Count);
            Assert.AreEqual(0, target.Calls.Count);
            child.AssertLast(1, "OnTouchDown(44:{X:1 Y:1})");

            target.ReportTouch(new TouchLocation(22, TouchLocationState.Released, new Vector2(2, 1)), t);
            Assert.AreEqual(2, child.Calls.Count);
            Assert.AreEqual(0, target.Calls.Count);
            child.AssertLast(2, "OnTouchUp(22:{X:2 Y:1})");
        }

        [TestMethod]
        public void Touch_GeneratesTouchup_WhenInvalid()
        {
            var target = new TouchTester() { Size = new Vector2(10, 10) };
            var child = new TouchTester() { Size = new Vector2(3, 3) };
            target.AddChild(child);

            var t = new GameTime();
            target.ReportTouch(new TouchLocation(11, TouchLocationState.Invalid, new Vector2(1, 1)), t);
            Assert.AreEqual(1, child.Calls.Count);
            Assert.AreEqual(0, target.Calls.Count);
            Assert.AreEqual("OnTouchUp(11:{X:1 Y:1})", child.Calls[0]);
        }

        [TestMethod]
        public void Touch_GeneratesMove_OnlyWhenPositionChanges()
        {
            var target = new TouchTester() { Size = new Vector2(10, 10) };
            var child = new TouchTester() { Size = new Vector2(3, 3) };
            target.AddChild(child);

            var t = new GameTime();
            target.ReportTouch(new TouchLocation(11, TouchLocationState.Pressed, new Vector2(1.51f, 1.21f)), t);
            target.ReportTouch(new TouchLocation(11, TouchLocationState.Moved, new Vector2(1.5f, 1.2f)), t);
            Assert.AreEqual(0, target.Calls.Count);
            child.AssertLast(2, "OnTouchMove(Move , 11 , {X:1.5 Y:1.2} , {X:1.51 Y:1.21})");

            target.ReportTouch(new TouchLocation(11, TouchLocationState.Moved, new Vector2(1.5f, 1.2f)), t);
            Assert.AreEqual(2, child.Calls.Count);

            target.ReportTouch(new TouchLocation(11, TouchLocationState.Moved, new Vector2(1.51f, 1.21f)), t);
            child.AssertLast(3, "OnTouchMove(Move , 11 , {X:1.51 Y:1.21} , {X:1.5 Y:1.2})");

            target.ReportTouch(new TouchLocation(11, TouchLocationState.Released, new Vector2(1.51f, 1.21f)), t);
            child.AssertLast(4, "OnTouchUp(11:{X:1.51 Y:1.21})");
        }

        [TestMethod]
        public void Touch_GeneratesEntryExitEvents_Correctly()
        {
            var target = new TouchTester() { Size = new Vector2(10, 10) };
            var child1 = new TouchTester() { Size = new Vector2(5, 5), PassThrough = false, Offset = new Vector2(1, 1) };
            var child2 = new TouchTester() { Size = new Vector2(2, 2), PassThrough = true,  Offset = new Vector2(.5f, .5f) };
            var child3 = new TouchTester() { Size = new Vector2(3, 3), PassThrough = true,  Offset = new Vector2(3, 3) };
            target.AddChild(child1);
            child1.AddChild(child2);
            target.AddChild(child3);

            // Touchdown
            var t = new GameTime();
            target.ReportTouch(new TouchLocation(19, TouchLocationState.Pressed, new Vector2(2, 2)), t);
            Assert.AreEqual(0, target.Calls.Count);
            child1.AssertLast(1, "OnTouchDown(19:{X:1 Y:1})");
            child2.AssertLast(1, "OnTouchDown(19:{X:0.5 Y:0.5})");
            Assert.AreEqual(0, child3.Calls.Count);

            // Move into child3, out of child 2
            target.ReportTouch(new TouchLocation(19, TouchLocationState.Moved, new Vector2(4, 4)), t);
            Assert.AreEqual(0, target.Calls.Count);
            child1.AssertLast(2, "OnTouchMove(Move , 19 , {X:3 Y:3} , {X:1 Y:1})");
            child2.AssertLast(2, "OnTouchMove(Leave , 19 , {X:2.5 Y:2.5} , {X:0.5 Y:0.5})");
            child3.AssertLast(1, "OnTouchMove(Enter , 19 , {X:1 Y:1} , {X:-1 Y:-1})");

            // Touchup somewhere away from all of the children
            target.ReportTouch(new TouchLocation(19, TouchLocationState.Released, new Vector2(100, 100)), t);
            Assert.AreEqual(0, target.Calls.Count);
            child1.AssertLast(4, "OnFlick({X:1 Y:1} , {X:98 Y:98})");
            Assert.AreEqual("OnTouchMove(Leave , 19 , {X:99 Y:99} , {X:3 Y:3})", child1.Calls[2]);
            Assert.AreEqual(3, child2.Calls.Count);
            child3.AssertLast(2, "OnTouchMove(Leave , 19 , {X:97 Y:97} , {X:1 Y:1})");

        }

    }
}
