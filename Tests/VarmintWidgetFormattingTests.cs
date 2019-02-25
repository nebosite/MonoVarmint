using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonoVarmint.Tools;
using MonoVarmint.Widgets;
using Microsoft.Xna.Framework;
using System.Reflection;

namespace MonoVarmint.Tools.Tests
{
    [TestClass]
    public class VarmintWidgetsFormattingTests : IVarmintWidgetInjector
    {
        public object GetInjectedValue(VarmintWidgetInjectAttribute attribute, PropertyInfo property)
        {
            return null;
        }



        [TestMethod]
        public void Format_NoMargins_NoSize_Stretch_FillsToMax()
        {
            var root = new TestWidget() {};
            root.UpdateFormatting(new Vector2(1.1f, 2.2f));
            Assert.AreEqual(new Vector2(1.1f, 2.2f), root.Size);

            var grid = new VarmintWidgetGrid() { };
            root.AddChild(grid);
            root.Prepare(null);
            root.UpdateFormatting(new Vector2(10,20));
            Assert.AreEqual(new Vector2(10, 20), grid.Size);
        }

        [TestMethod]
        public void Format_Handles_StretchAlignment()
        {
            var layoutText = @"<TestWidget WidgetAlignment=""Stretch,Stretch"" />";
            var target = (TestWidget)TestUtils.LoadFromText(this, layoutText, "(root)");

            target.Prepare(null);
            target.UpdateFormatting(new Vector2(5, 8));
            Assert.AreEqual(new Vector2(5, 8), target.Size);
            Assert.AreEqual(new Vector2(0, 0), target.Offset);

            // margins should constrain the stretch
            layoutText = @"<TestWidget WidgetAlignment=""Stretch,Stretch"" Margin=""1"" />";
            target = (TestWidget)TestUtils.LoadFromText(this, layoutText, "(root)");
            target.Prepare(null);
            target.UpdateFormatting(new Vector2(5, 8));
            Assert.AreEqual(new Vector2(3, 6), target.Size);
            Assert.AreEqual(new Vector2(1, 1), target.Offset);

            layoutText = @"<TestWidget WidgetAlignment=""Stretch,Stretch"" Margin=""1.5,2"" />";
            target = (TestWidget)TestUtils.LoadFromText(this, layoutText, "(root)");
            target.Prepare(null);
            target.UpdateFormatting(new Vector2(5, 8));
            Assert.AreEqual(new Vector2(2, 4), target.Size);
            Assert.AreEqual(new Vector2(1.5f, 2), target.Offset);

            layoutText = @"<TestWidget WidgetAlignment=""Stretch,Stretch"" Margin="",,.5"" />";
            target = (TestWidget)TestUtils.LoadFromText(this, layoutText, "(root)");
            target.Prepare(null);
            target.UpdateFormatting(new Vector2(5, 8));
            Assert.AreEqual(new Vector2(4.5f, 8), target.Size);
            Assert.AreEqual(new Vector2(0, 0), target.Offset);

            // Should throw if Size is specified with stretch
            layoutText = @"<TestWidget WidgetAlignment=""Stretch,Stretch"" Size=""1,1"" />";
            target = (TestWidget)TestUtils.LoadFromText(this, layoutText, "(root)");
            target.Prepare(null);           
            Assert.ThrowsException<ArgumentException>(() => target.UpdateFormatting(new Vector2(5, 8)));

            layoutText = @"<TestWidget WidgetAlignment="",Stretch"" Size="",1"" />";
            target = (TestWidget)TestUtils.LoadFromText(this, layoutText, "(root)");
            target.Prepare(null);
            Assert.ThrowsException<ArgumentException>(() => target.UpdateFormatting(new Vector2(5, 8)));

            layoutText = @"<TestWidget WidgetAlignment=""Stretch,"" Size=""1,"" />";
            target = (TestWidget)TestUtils.LoadFromText(this, layoutText, "(root)");
            target.Prepare(null);
            Assert.ThrowsException<ArgumentException>(() => target.UpdateFormatting(new Vector2(5, 8)));

            // Specifying no alignment and a size should default to Left or Top
            layoutText = @"<TestWidget WidgetAlignment="",Stretch"" Size=""1,"" />";
            target = (TestWidget)TestUtils.LoadFromText(this, layoutText, "(root)");
            target.Prepare(null);
            target.UpdateFormatting(new Vector2(5, 8));
            Assert.AreEqual(null, target.WidgetAlignment.X);
            Assert.AreEqual(Alignment.Stretch, target.WidgetAlignment.Y);
            Assert.AreEqual(new Vector2(0, 0), target.Offset);

            layoutText = @"<TestWidget WidgetAlignment=""Stretch,"" Size="",1"" />";
            target = (TestWidget)TestUtils.LoadFromText(this, layoutText, "(root)");
            target.Prepare(null);
            target.UpdateFormatting(new Vector2(5, 8));
            Assert.AreEqual(Alignment.Stretch, target.WidgetAlignment.X);
            Assert.AreEqual(null, target.WidgetAlignment.Y);
            Assert.AreEqual(new Vector2(0, 0), target.Offset);
        }


        [TestMethod]
        public void Format_Handles_LowAlignment()
        {
            var layoutText = @"<TestWidget WidgetAlignment=""Left,Top"" Size=""1,2"" />";
            var target = (TestWidget)TestUtils.LoadFromText(this, layoutText, "(root)");

            target.Prepare(null);
            target.UpdateFormatting(new Vector2(5, 8));
            Assert.AreEqual(new Vector2(1, 2), target.Size);
            Assert.AreEqual(new Vector2(0, 0), target.Offset);

            // Extra size should be truncated
            layoutText = @"<TestWidget  WidgetAlignment=""Left,Top""  Size=""1000,2000"" Margin=""1"" />";
            target = (TestWidget)TestUtils.LoadFromText(this, layoutText, "(root)");
            target.Prepare(null);
            target.UpdateFormatting(new Vector2(5, 8));
            Assert.AreEqual(new Vector2(3, 6), target.Size);
            Assert.AreEqual(new Vector2(1, 1), target.Offset);
        }


        [TestMethod]
        public void Format_Handles_CenterAlignment()
        {
            var layoutText = @"<TestWidget WidgetAlignment=""Center,Center"" Size=""1,2"" Margin=""1,2,.5,1.5"" />";
            var target = (TestWidget)TestUtils.LoadFromText(this, layoutText, "(root)");

            target.Prepare(null);
            target.UpdateFormatting(new Vector2(5, 8));
            Assert.AreEqual(new Vector2(1, 2), target.Size);
            Assert.AreEqual(new Vector2(2.25f, 3.25f), target.Offset);

            // Extra size should be truncated
            layoutText = @"<TestWidget  WidgetAlignment=""Center,Center""  Size=""1000,2000"" Margin="".5"" />";
            target = (TestWidget)TestUtils.LoadFromText(this, layoutText, "(root)");
            target.Prepare(null);
            target.UpdateFormatting(new Vector2(5, 8));
            Assert.AreEqual(new Vector2(4, 7), target.Size);
            Assert.AreEqual(new Vector2(.5f, .5f), target.Offset);

        }

        [TestMethod]
        public void Format_Handles_HighAlignment()
        {
            var layoutText = @"<TestWidget WidgetAlignment=""Right,Bottom"" Size=""1,2"" />";
            var target = (TestWidget)TestUtils.LoadFromText(this, layoutText, "(root)");

            target.Prepare(null);
            target.UpdateFormatting(new Vector2(5, 8));
            Assert.AreEqual(new Vector2(1, 2), target.Size);
            Assert.AreEqual(new Vector2(4, 6), target.Offset);

            // Extra size should be truncated
            layoutText = @"<TestWidget  WidgetAlignment=""Left,Top""  Size=""1000,2000"" Margin="".5"" />";
            target = (TestWidget)TestUtils.LoadFromText(this, layoutText, "(root)");
            target.Prepare(null);
            target.UpdateFormatting(new Vector2(5, 8));
            Assert.AreEqual(new Vector2(4, 7), target.Size);
            Assert.AreEqual(new Vector2(.5f, .5f), target.Offset);
        }




        [TestMethod]
        public void Format_NoMargins_NoSize_NoStretch_LimitsByMax()
        {
            var layoutText =
                @"<TestWidget>
                    <Grid Name=""Bob"" WidgetAlignment=""Center,Top"" Size=""2,3"">
                    </Grid>
                </TestWidget>";
            var target = (TestWidget)TestUtils.LoadFromText(this, layoutText, "(root)");
            var grid = target.FindWidgetByName("Bob");

            target.Prepare(null);
            target.UpdateFormatting(new Vector2(10, 20));
            Assert.AreEqual(new Vector2(10, 20), target.Size);
            Assert.AreEqual(new Vector2(2, 3), grid.Size);
            Assert.AreEqual(new Vector2(4, 0), grid.Offset);

            // if child is bigger than parent, it gets truncated
            layoutText =
                @"<TestWidget>
                    <Grid Name=""Bob"" WidgetAlignment=""Center,Top"" Size=""200,3000"">
                    </Grid>
                </TestWidget>";
            target = (TestWidget)TestUtils.LoadFromText(this, layoutText, "(root)");
            grid = target.FindWidgetByName("Bob");

            target.Prepare(null);
            target.UpdateFormatting(new Vector2(10, 20));
            Assert.AreEqual(new Vector2(10, 20), target.Size);
            Assert.AreEqual(new Vector2(10, 20), grid.Size);
            Assert.AreEqual(new Vector2(0, 0), grid.Offset);
        }

        /*
         * 
        Formatting rules:
        Margins		Size specified		Align		New Size Behavior
DONE: 0				na			Stretch		Fill to max
            0				na			*			Fill to children, limited by max and position accordingly
            0				y			Stretch		Error - cannot specify both Size and Align==Stretch
            0				y			*			Fill to Size, limited by max and position accordingly
            1				na			Stretch		Fill to max - margin, position by margin
            1				na			*			Fill to children, limited by max - margin and position accordingly
            1				y			Stretch		Error - cannot specify both Size and Align==Stretch
            1				y			*			Fill to Size, limited by max - margin, positioned as if size = size + margin
            2				na			Stretch		Fill to max - margin, position by margin
            2				na			*			Fill to children, limited by max - margin and position accordingly
            2				y			Stretch		Error - cannot specify both Size and Align==Stretch
            2				y			*			Fill to Size, limited by max - margin, positioned as if size = size + margin

        Align is specified as one or two values.  E.g.:   Left |  Left,Stretch | ,Center			
        Size is specified as one or two values:  x,y			
        With no parent, max is screen size			
        With scroll viewer, max is arbitrary, but there must be a clipping stage when rendering			
        Labels adjust their size to the text in them.  If they wrap, they wrap to the max size width	

        Changing the size of a control in the middle of a game should properly affect parent and children
    */

        [TestMethod]
        public void GridInGrid_RespectsMargins()
        {
            var size = new Vector2(10, 20);

            var target = new TestWidget() {  };
            var child = new TestWidget() { Size = new Vector2(2, 2) };
            target.HorizontalContentAlignment = HorizontalContentAlignment.Left;
            target.VerticalContentAlignment = VerticalContentAlignment.Top;
            target.AddChild(child);
            target.Prepare(null);

            // ####################### Left/Top
            target.UpdateFormatting(size);
            Assert.AreEqual(0, child.Offset.X);
            Assert.AreEqual(0, child.Offset.Y);

            // Setting left/top margins should change the offset as expected
            child.Margin = new VarmintWidget.WidgetMargin() { Left = 1, Top = 2 };
            target.UpdateFormatting(size);
            Assert.AreEqual(1, child.Offset.X);
            Assert.AreEqual(2, child.Offset.Y);

            // Setting right/bottom should pin the child to the right/bottom
            child.Margin = new VarmintWidget.WidgetMargin() { Right=1,Bottom=2 };
            target.UpdateFormatting(size);
            Assert.AreEqual(7, child.Offset.X);
            Assert.AreEqual(16, child.Offset.Y);
        }
    }
}
