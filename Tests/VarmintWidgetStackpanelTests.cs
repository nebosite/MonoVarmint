using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonoVarmint.Tools;
using MonoVarmint.Widgets;
using Microsoft.Xna.Framework;

namespace MonoVarmint.Tools.Tests
{
    [TestClass]
    public class VarmintWidgetsStackPanelTests
    {
        [TestMethod]
        public void StretchParameter_ExpandsChildren_Vertical()
        {
            var container = new VarmintWidgetStackPanel() { Size = new Vector2(10, 100) };
            var grid1 = new VarmintWidgetGrid() { Size = new Vector2(1, 1) };
            var grid2 = new VarmintWidgetGrid() { Size = new Vector2(1, 1) };
            var grid3 = new VarmintWidgetGrid() { Size = new Vector2(1, 1) };
            var grid4 = new VarmintWidgetGrid() { Size = new Vector2(1, 1) };
            container.AddChild(grid1);
            container.AddChild(grid2);
            container.AddChild(grid3);
            container.AddChild(grid4);

            // starting out with no stretch
            container.UpdateChildFormatting();
            Assert.AreEqual(new Vector2(1, 1), grid1.Size);
            Assert.AreEqual(new Vector2(1, 1), grid2.Size);
            Assert.AreEqual(new Vector2(1, 1), grid3.Size);
            Assert.AreEqual(new Vector2(1, 1), grid4.Size);
            Assert.AreEqual(new Vector2(0, 0), grid1.Offset);
            Assert.AreEqual(new Vector2(0, 1), grid2.Offset);
            Assert.AreEqual(new Vector2(0, 2), grid3.Offset);
            Assert.AreEqual(new Vector2(0, 3), grid4.Offset);

            // Horizontal only
            grid2.Stretch = new VarmintWidget.StretchParameter("1");
            container.UpdateChildFormatting();
            Assert.AreEqual(new Vector2(1, 1), grid1.Size);
            Assert.AreEqual(new Vector2(10, 1), grid2.Size);
            Assert.AreEqual(new Vector2(1, 1), grid3.Size);
            Assert.AreEqual(new Vector2(1, 1), grid4.Size);
            Assert.AreEqual(new Vector2(0, 0), grid1.Offset);
            Assert.AreEqual(new Vector2(0, 1), grid2.Offset);
            Assert.AreEqual(new Vector2(0, 2), grid3.Offset);
            Assert.AreEqual(new Vector2(0, 3), grid4.Offset);

            // Vertical only
            grid2.Stretch = new VarmintWidget.StretchParameter(",1");
            container.UpdateChildFormatting();
            Assert.AreEqual(new Vector2(1, 1), grid1.Size);
            Assert.AreEqual(new Vector2(1, 97), grid2.Size);
            Assert.AreEqual(new Vector2(1, 1), grid3.Size);
            Assert.AreEqual(new Vector2(1, 1), grid4.Size);
            Assert.AreEqual(new Vector2(0, 0), grid1.Offset);
            Assert.AreEqual(new Vector2(0, 1), grid2.Offset);
            Assert.AreEqual(new Vector2(0, 98), grid3.Offset);
            Assert.AreEqual(new Vector2(0, 99), grid4.Offset);

            // With Margins
            grid2.Margin = new VarmintWidget.WidgetMargin("2,2");
            grid2.Stretch = new VarmintWidget.StretchParameter();
            container.UpdateChildFormatting();
            Assert.AreEqual(new Vector2(1, 1), grid1.Size);
            Assert.AreEqual(new Vector2(1, 1), grid2.Size);
            Assert.AreEqual(new Vector2(1, 1), grid3.Size);
            Assert.AreEqual(new Vector2(1, 1), grid4.Size);
            Assert.AreEqual(new Vector2(0, 0), grid1.Offset);
            Assert.AreEqual(new Vector2(2, 3), grid2.Offset);
            Assert.AreEqual(new Vector2(0, 4), grid3.Offset);
            Assert.AreEqual(new Vector2(0, 5), grid4.Offset);

            // Two children, Stretch both proportionally
            grid2.Stretch = new VarmintWidget.StretchParameter("1,1"); ;
            grid4.Stretch = new VarmintWidget.StretchParameter("3,3"); ;
            container.UpdateChildFormatting();
            Assert.AreEqual(new Vector2(1, 1), grid1.Size);
            Assert.AreEqual(new Vector2(8, 24), grid2.Size);
            Assert.AreEqual(new Vector2(1, 1), grid3.Size);
            Assert.AreEqual(new Vector2(10, 72), grid4.Size);
            Assert.AreEqual(new Vector2(0, 0), grid1.Offset);
            Assert.AreEqual(new Vector2(2, 3), grid2.Offset);
            Assert.AreEqual(new Vector2(0, 27), grid3.Offset);
            Assert.AreEqual(new Vector2(0, 28), grid4.Offset);
            
        }

        [TestMethod]
        public void StretchParameter_ExpandsChildren_Horizontal()
        {
            var container = new VarmintWidgetStackPanel() { Size = new Vector2(100, 10) };
            container.Orientation = Orientation.Horizontal;
            var grid1 = new VarmintWidgetGrid() { Size = new Vector2(1, 1) };
            var grid2 = new VarmintWidgetGrid() { Size = new Vector2(1, 1) };
            var grid3 = new VarmintWidgetGrid() { Size = new Vector2(1, 1) };
            var grid4 = new VarmintWidgetGrid() { Size = new Vector2(1, 1) };
            container.AddChild(grid1);
            container.AddChild(grid2);
            container.AddChild(grid3);
            container.AddChild(grid4);

            // starting out with no stretch
            container.UpdateChildFormatting();
            Assert.AreEqual(new Vector2(1, 1), grid1.Size);
            Assert.AreEqual(new Vector2(1, 1), grid2.Size);
            Assert.AreEqual(new Vector2(1, 1), grid3.Size);
            Assert.AreEqual(new Vector2(1, 1), grid4.Size);
            Assert.AreEqual(new Vector2(0, 0), grid1.Offset);
            Assert.AreEqual(new Vector2(1, 0), grid2.Offset);
            Assert.AreEqual(new Vector2(2, 0), grid3.Offset);
            Assert.AreEqual(new Vector2(3, 0), grid4.Offset);

            // Veritcal only
            grid2.Stretch = new VarmintWidget.StretchParameter(",1");
            container.UpdateChildFormatting();
            Assert.AreEqual(new Vector2(1, 1), grid1.Size);
            Assert.AreEqual(new Vector2(1, 10), grid2.Size);
            Assert.AreEqual(new Vector2(1, 1), grid3.Size);
            Assert.AreEqual(new Vector2(1, 1), grid4.Size);
            Assert.AreEqual(new Vector2(0, 0), grid1.Offset);
            Assert.AreEqual(new Vector2(1, 0), grid2.Offset);
            Assert.AreEqual(new Vector2(2, 0), grid3.Offset);
            Assert.AreEqual(new Vector2(3, 0), grid4.Offset);

            // Horizontal only
            grid2.Stretch = new VarmintWidget.StretchParameter("1");
            container.UpdateChildFormatting();
            Assert.AreEqual(new Vector2(1, 1), grid1.Size);
            Assert.AreEqual(new Vector2(97, 1), grid2.Size);
            Assert.AreEqual(new Vector2(1, 1), grid3.Size);
            Assert.AreEqual(new Vector2(1, 1), grid4.Size);
            Assert.AreEqual(new Vector2(0, 0), grid1.Offset);
            Assert.AreEqual(new Vector2(1, 0), grid2.Offset);
            Assert.AreEqual(new Vector2(98, 0), grid3.Offset);
            Assert.AreEqual(new Vector2(99, 0), grid4.Offset);

            // With Margins
            grid2.Margin = new VarmintWidget.WidgetMargin("2,2");
            grid2.Stretch = new VarmintWidget.StretchParameter();
            container.UpdateChildFormatting();
            Assert.AreEqual(new Vector2(1, 1), grid1.Size);
            Assert.AreEqual(new Vector2(1, 1), grid2.Size);
            Assert.AreEqual(new Vector2(1, 1), grid3.Size);
            Assert.AreEqual(new Vector2(1, 1), grid4.Size);
            Assert.AreEqual(new Vector2(0, 0), grid1.Offset);
            Assert.AreEqual(new Vector2(3, 2), grid2.Offset);
            Assert.AreEqual(new Vector2(4, 0), grid3.Offset);
            Assert.AreEqual(new Vector2(5, 0), grid4.Offset);

            // Two children, Stretch both proportionally
            grid2.Stretch = new VarmintWidget.StretchParameter("1,1"); ;
            grid4.Stretch = new VarmintWidget.StretchParameter("3,3"); ;
            container.UpdateChildFormatting();
            Assert.AreEqual(new Vector2(1, 1), grid1.Size);
            Assert.AreEqual(new Vector2(24, 8), grid2.Size);
            Assert.AreEqual(new Vector2(1, 1), grid3.Size);
            Assert.AreEqual(new Vector2(72, 10), grid4.Size);
            Assert.AreEqual(new Vector2(0, 0), grid1.Offset);
            Assert.AreEqual(new Vector2(3, 2), grid2.Offset);
            Assert.AreEqual(new Vector2(27, 0), grid3.Offset);
            Assert.AreEqual(new Vector2(28, 0), grid4.Offset);

        }
    }
}
