using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonoVarmint.Tools;
using MonoVarmint.Widgets;
using Microsoft.Xna.Framework;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.Xna.Framework.Input.Touch;
using System.Collections.Generic;

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
            public Vector2 SizeProperty { get { return new Vector2(2, 3); } }
            public VarmintWidget.EventHandledState HandleTap(VarmintWidget widget, Vector2 location)
            {
                FooCalls++;
                return VarmintWidget.EventHandledState.Handled;
            }
        }

        VarmintWidget LoadFromText(string vwml, string defaultName)
        {
            using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(vwml)))
            {
                var layout = VarmintWidget.PreloadFromVwml(memoryStream, defaultName);
                return VarmintWidget.HydrateLayout(this, layout, new Dictionary<string, VarmintWidget.LayoutItem>());
            }
        }

        [TestMethod]
        public void GeneralPropertySerializationWorks()
        {
            var layoutText =
                @"<TestWidget 
                    Parameters=""Foo=Bar,that`Blech=223""
                    OnTap=""HandleTap""
                    Offset=""0.1,0.2""
                    Size=""10,11""
                />";
            var bindToMe = new BindingThing();

            var target = LoadFromText(layoutText, "Barney");
            target.BindingContext = bindToMe;
            target.Prepare(new Dictionary<string, VarmintWidgetStyle>());
            Assert.AreEqual("Barney", target.Name);
            Assert.AreEqual("Bar,that", target.Parameters["Foo"]);
            Assert.AreEqual("223", target.Parameters["Blech"]);
            Assert.AreEqual(0.1f, target.Offset.X);
            Assert.AreEqual(0.2f, target.Offset.Y);
            Assert.AreEqual(10, target.Size.X);
            Assert.AreEqual(11, target.Size.Y);
            target.HandleGesture(GestureType.Tap, new Vector2(1, 1), null);
            Assert.AreEqual(1, bindToMe.FooCalls);
        }

        [TestMethod]
        public void StylingPrecedenceIsEnforced()
        {
            var styleText =
                @"
                <Style
                    Name=""Style1""
                    FooProp=""ShouldNotCrash""
                    Size=""{SizeProperty}""
                    ForegroundColor=""Red""
                >
                    <Style
                        Name=""SubStyle""
                    />
                </Style>";
            var globalStyleText =
                @"
                <Style
                    Name=""Style2""
                    BackgroundColor=""Yellow""
                >
                    <Style
                        ApplyTo=""TestWidget""
                    />
                    <Style
                        ApplyTo=""TW""
                        Margin=""7""
                    />
                </Style>";
            var layoutText =
                @"
                <TestWidget 
                    Style=""SubStyle""
                    ForegroundColor=""Blue""
                >
                    <Label Name=""MyLabel"" BackgroundColor=""Gray"" />
                </TestWidget>";
            var bindToMe = new BindingThing();

            var style = LoadFromText(styleText, "Fooz");
            var globalStyle = LoadFromText(globalStyleText, "Freee");
            var target = LoadFromText(layoutText, "StyleTest");
            target.BindingContext = bindToMe;
            var styleLibrary = new Dictionary<string, VarmintWidgetStyle>();
            foreach (var styleItem in style.FindWidgetsByType<VarmintWidgetStyle>())
            {
                styleLibrary.Add(styleItem.Name, styleItem);
            }
            foreach (var styleItem in globalStyle.FindWidgetsByType<VarmintWidgetStyle>())
            {
                styleLibrary.Add(styleItem.Name, styleItem);
            }
            target.Prepare(styleLibrary);

            Assert.AreEqual("StyleTest", target.Name);
            Assert.AreEqual(Color.Blue, target.ForegroundColor);
            Assert.AreEqual(Color.Yellow, target.BackgroundColor);
            Assert.AreEqual(7, target.Margin.Left);
            Assert.AreEqual(new Vector2(2,3), target.Size);

            var label = target.FindWidgetByName("MyLabel");
            Assert.AreEqual(Color.Gray, label.BackgroundColor);
            // Global style should only apply to certain types
            Assert.AreEqual(null, label.Margin.Left); 
        }


    }
}
