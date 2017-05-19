using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonoVarmint.Tools;
using MonoVarmint.Widgets;
using Microsoft.Xna.Framework;

namespace MonoVarmint.Tools.Tests
{
    [TestClass]
    public class VarmintWidgetsFormattingTests
    {
        [TestMethod]
        public void StretchParamter_WorksWithGridlikeItems()
        {
            var container = new TestWidget() { Size = new Vector2(10, 20) };
            var grid = new VarmintWidgetGrid() { Size = new Vector2(1, 1) };
            container.AddChild(grid);

            // starting out with no stretch
            container.Prepare(null);
            Assert.AreEqual(new Vector2(1, 1), grid.Size);

            // Horizontal only
            grid.Stretch = new VarmintWidget.StretchParameter("1");
            container.UpdateChildFormatting();
            Assert.AreEqual(new Vector2(10, 1), grid.Size);
            Assert.AreEqual(new Vector2(0,0), grid.Offset);

            // Vertical only
            grid.Stretch = new VarmintWidget.StretchParameter(",1");
            container.UpdateChildFormatting();
            Assert.AreEqual(new Vector2(1, 20), grid.Size);
            Assert.AreEqual(new Vector2(0, 0), grid.Offset);

            // With Margins
            grid.Margin = new VarmintWidget.WidgetMargin("2,2");
            grid.Stretch = new VarmintWidget.StretchParameter();
            container.UpdateChildFormatting();
            Assert.AreEqual(new Vector2(1, 1), grid.Size);
            Assert.AreEqual(new Vector2(2, 2), grid.Offset);
            grid.Stretch = new VarmintWidget.StretchParameter("1,1"); ;
            container.UpdateChildFormatting();
            Assert.AreEqual(new Vector2(8, 18), grid.Size);
            Assert.AreEqual(new Vector2(2, 2), grid.Offset);

            // Two children, both should stretch to the same dimensions
            var grid2 = new VarmintWidgetGrid() { Size = new Vector2(2, 2) };
            grid2.Stretch = new VarmintWidget.StretchParameter("1,1"); ;
            container.AddChild(grid2);
            container.UpdateChildFormatting();
            Assert.AreEqual(new Vector2(8,18), grid.Size);
            Assert.AreEqual(new Vector2(2, 2), grid.Offset);
            Assert.AreEqual(new Vector2(10,20), grid2.Size);
            Assert.AreEqual(new Vector2(0,0), grid2.Offset);

        }
        [TestMethod]
        public void GridInGrid_RespectsMargins()
        {
            var target = new TestWidget() { Size = new Vector2(10, 20) };
            var child = new TestWidget() { Size = new Vector2(2, 2) };
            target.HorizontalContentAlignment = HorizontalContentAlignment.Left;
            target.VerticalContentAlignment = VerticalContentAlignment.Top;
            target.AddChild(child);
            target.Prepare(null);

            // ####################### Left/Top
            target.UpdateChildFormatting();
            Assert.AreEqual(0, child.Offset.X);
            Assert.AreEqual(0, child.Offset.Y);

            // Setting left/top margins should change the offset as expected
            child.Margin = new VarmintWidget.WidgetMargin() { Left = 1, Top = 2 };
            target.UpdateChildFormatting();
            Assert.AreEqual(1, child.Offset.X);
            Assert.AreEqual(2, child.Offset.Y);

            // Setting right/bottom should pin the child to the right/bottom
            child.Margin = new VarmintWidget.WidgetMargin() { Right=1,Bottom=2 };
            target.UpdateChildFormatting();
            Assert.AreEqual(7, child.Offset.X);
            Assert.AreEqual(16, child.Offset.Y);

            // ####################### Center
            target.HorizontalContentAlignment = HorizontalContentAlignment.Center;
            target.VerticalContentAlignment = VerticalContentAlignment.Center;
            child.Margin = new VarmintWidget.WidgetMargin();
            target.UpdateChildFormatting();
            Assert.AreEqual(4, child.Offset.X);
            Assert.AreEqual(9, child.Offset.Y);

            // Left/Top margin should pad the child
            child.Margin = new VarmintWidget.WidgetMargin() { Left = 2, Top = 4 };
            target.UpdateChildFormatting();
            Assert.AreEqual(5, child.Offset.X);
            Assert.AreEqual(11, child.Offset.Y);

            // Right/Bottom margin should pad the child
            child.Margin = new VarmintWidget.WidgetMargin() { Left = 2, Top = 4, Right=4, Bottom= 6 };
            target.UpdateChildFormatting();
            Assert.AreEqual(3, child.Offset.X);
            Assert.AreEqual(8, child.Offset.Y);

            // ####################### Right/Bottom
            target.HorizontalContentAlignment = HorizontalContentAlignment.Right;
            target.VerticalContentAlignment = VerticalContentAlignment.Bottom;
            child.Margin = new VarmintWidget.WidgetMargin();
            target.UpdateChildFormatting();
            Assert.AreEqual(8, child.Offset.X);
            Assert.AreEqual(18, child.Offset.Y);
        }

    }
}
