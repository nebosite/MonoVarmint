using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonoVarmint.Tools;
using MonoVarmint.Widgets;
using Microsoft.Xna.Framework;
using System.Reflection;
using System.Collections.Concurrent;

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
        public void Format_Label_Autosizes_ToText()
        {
            var layoutText = @"
<TestWidget MyAlignment=""Stretch,Stretch"" >
    <Grid Name=""TheGrid"">
        <Label Name=""TheLabel"" Content=""Hi"" />
    </Grid>
</TestWidget>";
            var target = (TestWidget)TestUtils.LoadFromText(this, layoutText, "(root)");
            var label = (VarmintWidgetLabel) target.FindWidgetByName("TheLabel");

            var renderer = new MockRenderer(10,10);
            renderer.MeasureTextReturn = new Vector2(.5f, .2f);
            label.Renderer = renderer;
            target.Prepare(null);
            target.UpdateFormatting(new Vector2(5, 8));
            Assert.AreEqual(new Vector2(5, 8), label.Size);
            Assert.AreEqual(new Vector2(0,0), label.Offset);

            label.MyAlignment = new AlignmentTuple(Alignment.Center, Alignment.Center);
            target.Prepare(null);
            target.UpdateFormatting(new Vector2(5, 8));
            Assert.AreEqual(new Vector2(.5f, .2f), label.Size);
            Assert.AreEqual(new Vector2(2.25f, 3.9f), label.Offset);

        }

        [TestMethod]
        public void Format_NoMargins_NoSize_Stretch_FillsToMax()
        {
            var root = new TestWidget() { };
            root.UpdateFormatting(new Vector2(1.1f, 2.2f));
            Assert.AreEqual(new Vector2(1.1f, 2.2f), root.Size);

            var grid = new VarmintWidgetGrid() { };
            root.AddChild(grid);
            root.Prepare(null);
            root.UpdateFormatting(new Vector2(10, 20));
            Assert.AreEqual(new Vector2(10, 20), grid.Size);
        }

        [TestMethod]
        public void Format_Handles_StretchAlignment()
        {
            var layoutText = @"<TestWidget MyAlignment=""Stretch,Stretch"" />";
            var target = (TestWidget)TestUtils.LoadFromText(this, layoutText, "(root)");

            target.Prepare(null);
            target.UpdateFormatting(new Vector2(5, 8));
            Assert.AreEqual(new Vector2(5, 8), target.Size);
            Assert.AreEqual(new Vector2(0, 0), target.Offset);

            // margins should constrain the stretch
            layoutText = @"<TestWidget MyAlignment=""Stretch,Stretch"" Margin=""1"" />";
            target = (TestWidget)TestUtils.LoadFromText(this, layoutText, "(root)");
            target.Prepare(null);
            target.UpdateFormatting(new Vector2(5, 8));
            Assert.AreEqual(new Vector2(3, 6), target.Size);
            Assert.AreEqual(new Vector2(1, 1), target.Offset);

            layoutText = @"<TestWidget MyAlignment=""Stretch,Stretch"" Margin=""1.5,2"" />";
            target = (TestWidget)TestUtils.LoadFromText(this, layoutText, "(root)");
            target.Prepare(null);
            target.UpdateFormatting(new Vector2(5, 8));
            Assert.AreEqual(new Vector2(2, 4), target.Size);
            Assert.AreEqual(new Vector2(1.5f, 2), target.Offset);

            layoutText = @"<TestWidget MyAlignment=""Stretch,Stretch"" Margin="",,.5"" />";
            target = (TestWidget)TestUtils.LoadFromText(this, layoutText, "(root)");
            target.Prepare(null);
            target.UpdateFormatting(new Vector2(5, 8));
            Assert.AreEqual(new Vector2(4.5f, 8), target.Size);
            Assert.AreEqual(new Vector2(0, 0), target.Offset);

            // Should throw if Size is specified with stretch
            layoutText = @"<TestWidget MyAlignment=""Stretch,Stretch"" Size=""1,1"" />";
            target = (TestWidget)TestUtils.LoadFromText(this, layoutText, "(root)");
            target.Prepare(null);           
            Assert.ThrowsException<ArgumentException>(() => target.UpdateFormatting(new Vector2(5, 8)));

            layoutText = @"<TestWidget MyAlignment="",Stretch"" Size="",1"" />";
            target = (TestWidget)TestUtils.LoadFromText(this, layoutText, "(root)");
            target.Prepare(null);
            Assert.ThrowsException<ArgumentException>(() => target.UpdateFormatting(new Vector2(5, 8)));

            layoutText = @"<TestWidget MyAlignment=""Stretch,"" Size=""1,"" />";
            target = (TestWidget)TestUtils.LoadFromText(this, layoutText, "(root)");
            target.Prepare(null);
            Assert.ThrowsException<ArgumentException>(() => target.UpdateFormatting(new Vector2(5, 8)));

            // Specifying no alignment and a size should default to Left or Top
            layoutText = @"<TestWidget MyAlignment="",Stretch"" Size=""1,"" />";
            target = (TestWidget)TestUtils.LoadFromText(this, layoutText, "(root)");
            target.Prepare(null);
            target.UpdateFormatting(new Vector2(5, 8));
            Assert.AreEqual(null, target.MyAlignment.X);
            Assert.AreEqual(Alignment.Stretch, target.MyAlignment.Y);
            Assert.AreEqual(new Vector2(0, 0), target.Offset);

            layoutText = @"<TestWidget MyAlignment=""Stretch,"" Size="",1"" />";
            target = (TestWidget)TestUtils.LoadFromText(this, layoutText, "(root)");
            target.Prepare(null);
            target.UpdateFormatting(new Vector2(5, 8));
            Assert.AreEqual(Alignment.Stretch, target.MyAlignment.X);
            Assert.AreEqual(null, target.MyAlignment.Y);
            Assert.AreEqual(new Vector2(0, 0), target.Offset);
        }


        [TestMethod]
        public void Format_Handles_LowAlignment()
        {
            var layoutText = @"<TestWidget MyAlignment=""Left,Top"" Size=""1,2"" />";
            var target = (TestWidget)TestUtils.LoadFromText(this, layoutText, "(root)");

            target.Prepare(null);
            target.UpdateFormatting(new Vector2(5, 8));
            Assert.AreEqual(new Vector2(1, 2), target.Size);
            Assert.AreEqual(new Vector2(0, 0), target.Offset);

            // Extra size should be truncated
            layoutText = @"<TestWidget  MyAlignment=""Left,Top""  Size=""1000,2000"" Margin=""1"" />";
            target = (TestWidget)TestUtils.LoadFromText(this, layoutText, "(root)");
            target.Prepare(null);
            target.UpdateFormatting(new Vector2(5, 8));
            Assert.AreEqual(new Vector2(3, 6), target.Size);
            Assert.AreEqual(new Vector2(1, 1), target.Offset);
        }


        [TestMethod]
        public void Format_Handles_CenterAlignment()
        {
            var layoutText = @"<TestWidget MyAlignment=""Center,Center"" Size=""1,2"" Margin=""1,2,.5,1.5"" />";
            var target = (TestWidget)TestUtils.LoadFromText(this, layoutText, "(root)");

            target.Prepare(null);
            target.UpdateFormatting(new Vector2(5, 8));
            Assert.AreEqual(new Vector2(1, 2), target.Size);
            Assert.AreEqual(new Vector2(2.25f, 3.25f), target.Offset);

            // Extra size should be truncated
            layoutText = @"<TestWidget  MyAlignment=""Center,Center""  Size=""1000,2000"" Margin="".5"" />";
            target = (TestWidget)TestUtils.LoadFromText(this, layoutText, "(root)");
            target.Prepare(null);
            target.UpdateFormatting(new Vector2(5, 8));
            Assert.AreEqual(new Vector2(4, 7), target.Size);
            Assert.AreEqual(new Vector2(.5f, .5f), target.Offset);

        }

        [TestMethod]
        public void Format_Handles_HighAlignment()
        {
            var layoutText = @"<TestWidget MyAlignment=""Right,Bottom"" Size=""1,2"" />";
            var target = (TestWidget)TestUtils.LoadFromText(this, layoutText, "(root)");

            target.Prepare(null);
            target.UpdateFormatting(new Vector2(5, 8));
            Assert.AreEqual(new Vector2(1, 2), target.Size);
            Assert.AreEqual(new Vector2(4, 6), target.Offset);

            // Extra size should be truncated
            layoutText = @"<TestWidget  MyAlignment=""Left,Top""  Size=""1000,2000"" Margin="".5"" />";
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
                    <Grid Name=""Bob"" MyAlignment=""Center,Top"" Size=""2,3"">
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
                    <Grid Name=""Bob"" MyAlignment=""Center,Top"" Size=""200,3000"">
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

        [TestMethod]
        public void Format_ExtremeMargins_AffectsAlignment()
        {
            var layoutText =
                @"<Grid>
                      <Grid Name=""BigGrid"" Margin=""0,0,0,0""  />
                      <Grid Name=""SmallGrid"" Margin="",,0,0"" Size=""12,3"" >
                        <StackPanel Name=""StackPanel""  >
                          <Grid Size=""4,1""/>
                        </StackPanel>  
                      </Grid>
                    </Grid>";
            var target = (VarmintWidgetGrid)TestUtils.LoadFromText(this, layoutText, "(root)");
            var bigGrid = target.FindWidgetByName("BigGrid");
            var smallGrid = target.FindWidgetByName("SmallGrid");
            var stackPanel = target.FindWidgetByName("StackPanel");

            target.Prepare(null);
            target.UpdateFormatting(new Vector2(20, 40));
            Assert.AreEqual(new Vector2(20, 40), target.Size);
            Assert.AreEqual(new Vector2(20, 40), bigGrid.Size);
            Assert.AreEqual(new Vector2(0, 0), bigGrid.Offset);
            Assert.AreEqual(new Vector2(12, 3), smallGrid.Size);
            Assert.AreEqual(new Vector2(8, 37), smallGrid.Offset);
            Assert.AreEqual(new Vector2(12, 3), stackPanel.Size);
            Assert.AreEqual(new Vector2(0, 0), stackPanel.Offset);

        }


        [TestMethod]
        public void Format_ContentIsPreserved()
        {
            var mockRenderer = new MockRenderer(10, 20);
            mockRenderer.MeasureTextReturn = new Vector2(1, 1);
            var layoutText =@"<Label Content=""Free Balloons"" />";
            var target = (VarmintWidgetLabel)TestUtils.LoadFromText(mockRenderer, layoutText, "(root)");
            target.Prepare(null);
            target.UpdateFormatting(new Vector2(20, 40));
            Assert.AreEqual("Free Balloons", target.Content);
        }


        [TestMethod]
        public void Format_ImageAlignment_Works()
        {
            var mockRenderer = new MockRenderer(10, 20);
            mockRenderer.MeasureTextReturn = new Vector2(1, 1);
            var layoutText = @"
                <Grid>
                    <Image
                        Name=""TextLogo""
                        Margin = ""0,6""
                        Size = ""6,2""
                        ContentAlignment = ""Left,""
                        Content = ""TextLogo"" >
                        <Label Content=""foo"" />
                    </Image> 
                </Grid>
                ";
            var target = (VarmintWidgetGrid)TestUtils.LoadFromText(mockRenderer, layoutText, "(root)");
            var image = target.FindWidgetByName("TextLogo");
            target.Prepare(null);
            target.UpdateFormatting(new Vector2(20, 40));
            Assert.AreEqual(new Vector2(6, 2), image.Size);
            Assert.AreEqual(new Vector2(0,6), image.Offset);
        }


        /*
              <Image
        Name="TextLogo"
        Margin="0,.26"
        Size=".8,.24"
        ContentAlignment="Left,"
        Content="TextLogo">
        <Label Content="{Version}" ForegroundColor="#C94242" FontSize=".05" Size =".2,.04" Margin=".325,.195" ContentAlignment="Center" />
      </Image>

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
        }
    }
}
