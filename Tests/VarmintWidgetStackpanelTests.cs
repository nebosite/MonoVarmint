using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonoVarmint.Tools;
using MonoVarmint.Widgets;
using Microsoft.Xna.Framework;
using System.Reflection;
using System.Collections.Generic;

namespace MonoVarmint.Tools.Tests
{
    [TestClass]
    public class VarmintWidgetsStackPanelTests
    {

        [TestMethod]
        public void StackPanel_NormalVerical_Works()
        {
            var renderer = new MockRenderer(10, 20);
            renderer.MeasureTextReturn = new Vector2(4, 1);
            
            var layoutText =
                @"<Grid  ContentAlignment=""Left,Top"">
                    <StackPanel Name=""panel"" Orientation=""Vertical"" >
                        <Grid Size=""6,1""  Name=""gridChild"" />
                        <Label Content=""Hi"" Name=""labelChild""/>
                    </StackPanel>
                   </Grid>";

            var target = TestUtils.LoadFromText(renderer, layoutText, "Target");
            target.Prepare(new Dictionary<string, VarmintWidgetStyle>());
            target.UpdateFormatting(renderer.ScreenSize);



            var panel = target.FindWidgetByName("panel");
            var gridChild = target.FindWidgetByName("gridChild");
            var labelChild = target.FindWidgetByName("labelChild");

            // Should size to children
            Assert.AreEqual(new Vector2(6, 2), panel.Size);
            Assert.AreEqual(new Vector2(0, 0), panel.Offset);

            // children should have correct size and offset
            Assert.AreEqual(new Vector2(6, 1), gridChild.Size);
            Assert.AreEqual(new Vector2(0, 0), gridChild.Offset);
            Assert.AreEqual(new Vector2(4, 1), labelChild.Size);
            Assert.AreEqual(new Vector2(0, 1), labelChild.Offset);

        }

        [TestMethod]
        public void StackPanel_NormalHorizontal_Works()
        {
            var renderer = new MockRenderer(10, 20);
            renderer.MeasureTextReturn = new Vector2(1, 3);

            var layoutText =
                @"<Grid  ContentAlignment=""Left,Top"">
                    <StackPanel Name=""panel"" Orientation=""Horizontal"" >
                        <Grid Size=""2,5""  Name=""gridChild"" />
                        <Label Content=""Hi"" Name=""labelChild""/>
                    </StackPanel>
                   </Grid>";

            var target = TestUtils.LoadFromText(renderer, layoutText, "Target");
            target.Prepare(new Dictionary<string, VarmintWidgetStyle>());
            target.UpdateFormatting(renderer.ScreenSize);


            var panel = target.FindWidgetByName("panel");
            var gridChild = target.FindWidgetByName("gridChild");
            var labelChild = target.FindWidgetByName("labelChild");

            // Should size to children
            Assert.AreEqual(new Vector2(3,5), panel.Size);
            Assert.AreEqual(new Vector2(0, 0), panel.Offset);

            // children should have correct size and offset
            Assert.AreEqual(new Vector2(2, 5), gridChild.Size);
            Assert.AreEqual(new Vector2(0, 0), gridChild.Offset);
            Assert.AreEqual(new Vector2(1, 3), labelChild.Size);
            Assert.AreEqual(new Vector2(2, 0), labelChild.Offset);

        }


        [TestMethod]
        public void StackPanel_NormalVerical_StretchedContent_Works()
        {
            var renderer = new MockRenderer(10, 20);
            renderer.MeasureTextReturn = new Vector2(4, 1);

            var layoutText =
                @"<Grid  ContentAlignment=""Left,Top"">
                    <StackPanel>
                        <StackPanel Name=""panel1"" Orientation=""Vertical"" ContentAlignment=""Left,"">
                            <Grid Size=""6,1""  Name=""gridChild1"" />
                            <Label Content=""Hi"" Name=""labelChild1""/>
                        </StackPanel>
                        <StackPanel Name=""panel2"" Orientation=""Vertical"" ContentAlignment=""Center,"">
                            <Grid Size=""6,1""  Name=""gridChild2"" />
                            <Label Content=""Hi"" Name=""labelChild2""/>
                        </StackPanel>
                        <StackPanel Name=""panel3"" Orientation=""Vertical"" ContentAlignment=""Right,"">
                            <Grid Size=""6,1""  Name=""gridChild3"" />
                            <Label Content=""Hi"" Name=""labelChild3""/>
                        </StackPanel>
                    </StackPanel>
                   </Grid>";

            var target = TestUtils.LoadFromText(renderer, layoutText, "Target");
            target.Prepare(new Dictionary<string, VarmintWidgetStyle>());
            target.UpdateFormatting(renderer.ScreenSize);


            var panel = target.FindWidgetByName("panel1");
            var gridChild = target.FindWidgetByName("gridChild1");
            var labelChild = target.FindWidgetByName("labelChild1");

            // Should size to children
            Assert.AreEqual(new Vector2(6, 2), panel.Size);
            Assert.AreEqual(new Vector2(0, 0), panel.Offset);

            // children should have correct size and offset
            Assert.AreEqual(new Vector2(6, 1), gridChild.Size);
            Assert.AreEqual(new Vector2(0, 0), gridChild.Offset);
            Assert.AreEqual(new Vector2(4, 1), labelChild.Size);
            Assert.AreEqual(new Vector2(0, 1), labelChild.Offset);


            panel = target.FindWidgetByName("panel2");
            gridChild = target.FindWidgetByName("gridChild2");
            labelChild = target.FindWidgetByName("labelChild2");

            // Should size to children
            Assert.AreEqual(new Vector2(6, 2), panel.Size);
            Assert.AreEqual(new Vector2(0, 2), panel.Offset);

            // children should have correct size and offset
            Assert.AreEqual(new Vector2(6, 1), gridChild.Size);
            Assert.AreEqual(new Vector2(0, 0), gridChild.Offset);
            Assert.AreEqual(new Vector2(4, 1), labelChild.Size);
            Assert.AreEqual(new Vector2(1, 1), labelChild.Offset);


            panel = target.FindWidgetByName("panel3");
            gridChild = target.FindWidgetByName("gridChild3");
            labelChild = target.FindWidgetByName("labelChild3");

            // Should size to children
            Assert.AreEqual(new Vector2(6, 2), panel.Size);
            Assert.AreEqual(new Vector2(0, 4), panel.Offset);

            // children should have correct size and offset
            Assert.AreEqual(new Vector2(6, 1), gridChild.Size);
            Assert.AreEqual(new Vector2(0, 0), gridChild.Offset);
            Assert.AreEqual(new Vector2(4, 1), labelChild.Size);
            Assert.AreEqual(new Vector2(2, 1), labelChild.Offset);


        }

        [TestMethod]
        public void StackPanel_NormalHorizontal_StretchedContent_Works()
        {
            var renderer = new MockRenderer(10, 20);
            renderer.MeasureTextReturn = new Vector2(1, 3);

            var layoutText =
                @"<Grid  ContentAlignment=""Left,Top"">
                    <StackPanel>
                        <StackPanel Name=""panel1"" Orientation=""Horizontal"" ContentAlignment="",Top"">
                            <Grid Size=""2,5""  Name=""gridChild1"" />
                            <Label Content=""Hi"" Name=""labelChild1""/>
                        </StackPanel>
                        <StackPanel Name=""panel2"" Orientation=""Horizontal"" ContentAlignment="",Center"">
                            <Grid Size=""2,5""  Name=""gridChild2"" />
                            <Label Content=""Hi"" Name=""labelChild2""/>
                        </StackPanel>
                        <StackPanel Name=""panel3"" Orientation=""Horizontal"" ContentAlignment="",Bottom"">
                            <Grid Size=""2,5""  Name=""gridChild3"" />
                            <Label Content=""Hi"" Name=""labelChild3""/>
                        </StackPanel>
                    </StackPanel>
                   </Grid>";

            var target = TestUtils.LoadFromText(renderer, layoutText, "Target");
            target.Prepare(new Dictionary<string, VarmintWidgetStyle>());
            target.UpdateFormatting(renderer.ScreenSize);

            var panel = target.FindWidgetByName("panel1");
            var gridChild = target.FindWidgetByName("gridChild1");
            var labelChild = target.FindWidgetByName("labelChild1");

            // Should size to children
            Assert.AreEqual(new Vector2(3, 5), panel.Size);
            Assert.AreEqual(new Vector2(0, 0), panel.Offset);

            // children should have correct size and offset
            Assert.AreEqual(new Vector2(2, 5), gridChild.Size);
            Assert.AreEqual(new Vector2(0, 0), gridChild.Offset);
            Assert.AreEqual(new Vector2(1, 3), labelChild.Size);
            Assert.AreEqual(new Vector2(2, 0), labelChild.Offset);


            panel = target.FindWidgetByName("panel2");
            gridChild = target.FindWidgetByName("gridChild2");
            labelChild = target.FindWidgetByName("labelChild2");

            // Should size to children
            Assert.AreEqual(new Vector2(3, 5), panel.Size);
            Assert.AreEqual(new Vector2(0, 5), panel.Offset);

            // children should have correct size and offset
            Assert.AreEqual(new Vector2(2, 5), gridChild.Size);
            Assert.AreEqual(new Vector2(0, 0), gridChild.Offset);
            Assert.AreEqual(new Vector2(1, 3), labelChild.Size);
            Assert.AreEqual(new Vector2(2, 1), labelChild.Offset);


            panel = target.FindWidgetByName("panel3");
            gridChild = target.FindWidgetByName("gridChild3");
            labelChild = target.FindWidgetByName("labelChild3");

            // Should size to children
            Assert.AreEqual(new Vector2(3, 5), panel.Size);
            Assert.AreEqual(new Vector2(0, 10), panel.Offset);

            // children should have correct size and offset
            Assert.AreEqual(new Vector2(2, 5), gridChild.Size);
            Assert.AreEqual(new Vector2(0, 0), gridChild.Offset);
            Assert.AreEqual(new Vector2(1, 3), labelChild.Size);
            Assert.AreEqual(new Vector2(2, 2), labelChild.Offset);
        }


        // Stretched objects should share the remaining space equally (Later maybe we can add a stretch parameter "StretchPortion")
        [TestMethod]
        public void StackPanel_NormalVerical_AlignedContent_Works()
        {
            var renderer = new MockRenderer(10, 20);
            renderer.MeasureTextReturn = new Vector2(4, 1);

            var layoutText =
                @"<Grid  ContentAlignment=""Left,Top"">
                    <StackPanel Name=""FirstPanel"">
                        <StackPanel Name=""panel1"" Orientation=""Vertical"" ContentAlignment=""Center"">
                            <Grid Size=""6,1""  Name=""grid1"" />
                            <Grid  Name=""gridStretched1"" WidgetAlignment=""Stretch"" />
                            <Label Content=""Hi"" Name=""labelChild1"" Margin=""1""/>
                            <Grid  Name=""gridStretched2"" WidgetAlignment=""Stretch"" />
                       </StackPanel>
                    </StackPanel>
                   </Grid>";

            var target = TestUtils.LoadFromText(renderer, layoutText, "Target");
            target.Prepare(new Dictionary<string, VarmintWidgetStyle>());
            target.UpdateFormatting(renderer.ScreenSize);

            var panel = target.FindWidgetByName("panel1");
            var child1 = target.FindWidgetByName("grid1");
            var child2 = target.FindWidgetByName("gridStretched1");
            var child3 = target.FindWidgetByName("labelChild1");
            var child4 = target.FindWidgetByName("gridStretched2");

            // Should size to children, which will push it to the max extent
            Assert.AreEqual(new Vector2(10, 20), panel.Size);
            Assert.AreEqual(new Vector2(0, 0), panel.Offset);

            // children should have correct size and offset
            Assert.AreEqual(new Vector2(6, 1), child1.Size);
            Assert.AreEqual(new Vector2(10, 8), child2.Size);
            Assert.AreEqual(new Vector2(4, 1), child3.Size);
            Assert.AreEqual(new Vector2(10, 8), child4.Size);

            Assert.AreEqual(new Vector2(2, 0), child1.Offset);
            Assert.AreEqual(new Vector2(0, 1), child2.Offset);
            Assert.AreEqual(new Vector2(3, 10), child3.Offset);
            Assert.AreEqual(new Vector2(0, 12), child4.Offset);
        }

        [TestMethod]
        public void StackPanel_NormalHorizontal_AlignedContent_Works()
        {
            var renderer = new MockRenderer(10, 20);
            renderer.MeasureTextReturn = new Vector2(1, 3);

            var layoutText =
                @"<Grid  ContentAlignment=""Left,Top"">
                    <StackPanel Name=""FirstPanel"">
                        <StackPanel Name=""panel1"" Orientation=""Horizontal"" ContentAlignment=""Center"">
                            <Grid Size=""2,5""  Name=""grid1"" />
                            <Grid  Name=""gridStretched1"" WidgetAlignment=""Stretch"" />
                            <Label Content=""Hi"" Name=""labelChild1"" Margin=""1""/>
                            <Grid  Name=""gridStretched2"" WidgetAlignment=""Stretch"" />
                       </StackPanel>
                    </StackPanel>
                   </Grid>";

            var target = TestUtils.LoadFromText(renderer, layoutText, "Target");
            target.Prepare(new Dictionary<string, VarmintWidgetStyle>());
            target.UpdateFormatting(renderer.ScreenSize);

            var panel = target.FindWidgetByName("panel1");
            var child1 = target.FindWidgetByName("grid1");
            var child2 = target.FindWidgetByName("gridStretched1");
            var child3 = target.FindWidgetByName("labelChild1");
            var child4 = target.FindWidgetByName("gridStretched2");

            // Should size to children, which will push it to the max extent
            Assert.AreEqual(new Vector2(10, 20), panel.Size);
            Assert.AreEqual(new Vector2(0, 0), panel.Offset);

            // children should have correct size and offset
            Assert.AreEqual(new Vector2(2, 5), child1.Size);
            Assert.AreEqual(new Vector2(2.5f, 20), child2.Size);
            Assert.AreEqual(new Vector2(1, 3), child3.Size);
            Assert.AreEqual(new Vector2(2.5f, 20), child4.Size);

            Assert.AreEqual(new Vector2(0, 7.5f), child1.Offset);
            Assert.AreEqual(new Vector2(2, 0), child2.Offset);
            Assert.AreEqual(new Vector2(5.5f, 8.5f), child3.Offset);
            Assert.AreEqual(new Vector2(7.5f, 0), child4.Offset);
        }

    }
}
