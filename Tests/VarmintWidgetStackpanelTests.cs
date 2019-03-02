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
    public class VarmintWidgetsStackPanelTests : IVarmintWidgetInjector
    {
        public object GetInjectedValue(VarmintWidgetInjectAttribute attribute, PropertyInfo property)
        {
            return null;
        }

        [TestMethod]
        public void StackPanel_WithUnsizedLables_StacksAccordingToContent()
        {
            var container = new VarmintWidgetStackPanel() { Size = new Vector2(500, 100) };
            var label1 = new VarmintWidgetLabel() { Content = "Hi There" };
            var label2 = new VarmintWidgetLabel() { Content = "More COntent" };
            container.AddChild(label1);
            container.AddChild(label2);

            container.Prepare(null);

            // starting out with no stretch
            container.UpdateFormatting(new Vector2(500, 100));
            Assert.AreEqual(new Vector2(1, 1), label1.Size);
            Assert.AreEqual(new Vector2(1, 1), label2.Size);
        }

        [TestMethod]
        public void StackPanelCenteredOnAGrid_WorksFromSerialized()
        {
            var layoutText =
                @"<Grid  ContentAlignment=""Center,"" Size=""10,20"">
                    <StackPanel Name=""panel"">
                        <Grid Size=""6,1""  Name=""panelChild"" />
                    </StackPanel>
                   </Grid>";

            var target = TestUtils.LoadFromText(this, layoutText, "Barney");
            target.Prepare(new Dictionary<string, VarmintWidgetStyle>());
            target.UpdateFormatting(new Vector2(10,20));

            var panel = target.FindWidgetByName("panel");
            var panelChild1 = target.FindWidgetByName("panelChild");
            Assert.AreEqual(new Vector2(6, 1), panel.Size);
            Assert.AreEqual(new Vector2(2, 0), panel.Offset);
            Assert.AreEqual(new Vector2(0, 0), panelChild1.Offset);

        }
    }
}
