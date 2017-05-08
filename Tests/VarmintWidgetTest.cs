using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonoVarmint.Tools;
using MonoVarmint.Widgets;
using Microsoft.Xna.Framework;

namespace Tests
{
    [TestClass]
    public class VarmintWidgetsTests
    {
        /// <summary>
        /// Vanilla widget to test just VarmintWidget code
        /// </summary>
        class TestWidget : VarmintWidget { }

        [TestMethod]
        public void ForeGroundColorPropertyWorks()
        {
            var target = new TestWidget();

            // Default color is black
            Assert.AreEqual(Color.Black, target.ForegroundColor);

            // If not specified, color is inherited
            var parent = new TestWidget();
            parent.ForegroundColor = Color.Green;
            parent.AddChild(target);
            Assert.AreEqual(Color.Green, parent.ForegroundColor);
            Assert.AreEqual(Color.Green, target.ForegroundColor);
            target.ForegroundColor = Color.Yellow;
            Assert.AreEqual(Color.Green, parent.ForegroundColor);
            Assert.AreEqual(Color.Yellow, target.ForegroundColor);
        }

    }
}
