using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonoVarmint.Tools;
using MonoVarmint.Widgets;
using Microsoft.Xna.Framework;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.Xna.Framework.Input.Touch;

namespace MonoVarmint.Tools.Tests
{
    [TestClass]
    public class VarmintWidgetSerializationTests: IVarmintWidgetInjector
    {
        public object GetInjectedValue(VarmintWidgetInjectAttribute attribute, PropertyInfo property)
        {
            return null;
        }

        class BindingThing
        {
            public int FooCalls = 0;
            public VarmintWidget.EventHandledState HandleTap(VarmintWidget widget, Vector2 location)
            {
                FooCalls++;
                return VarmintWidget.EventHandledState.Handled;
            }
        }

        [TestMethod]
        public void GeneralPropertySerializationWorks()
        {
            var layoutText =
@"<TestWidget 
    Parameters=""Foo=Bar,that""
    OnTap=""HandleTap""
    Offset=""0.1,0.2""
    Size=""10,11""
/>";
            var bindToMe = new BindingThing();

            var target = VarmintWidget.LoadLayoutFromVwml(this,  new MemoryStream(Encoding.UTF8.GetBytes(layoutText)), "Barney");
            target.BindingContext = bindToMe;
            target.Init();
            Assert.AreEqual("Barney", target.Name);
            Assert.AreEqual("Bar,that", target.Parameters["Foo"]);
            Assert.AreEqual(0.1f, target.Offset.X);
            Assert.AreEqual(0.2f, target.Offset.Y);
            Assert.AreEqual(10, target.Size.X);
            Assert.AreEqual(11, target.Size.Y);
            target.HandleGesture(GestureType.Tap, new Vector2(1, 1), null);
            Assert.AreEqual(1, bindToMe.FooCalls);
        }

    }
}
