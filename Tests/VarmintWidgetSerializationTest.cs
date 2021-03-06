﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonoVarmint.Tools;
using MonoVarmint.Widgets;
using Microsoft.Xna.Framework;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.Xna.Framework.Input.Touch;
using System.Collections.Generic;
using System.Linq;

namespace MonoVarmint.Tools.Tests
{
    [TestClass]
    public class VarmintWidgetSerializationTests: IVarmintWidgetInjector
    {
        public object GetInjectedValue(VarmintWidgetInjectAttribute attribute, PropertyInfo property)
        {
            return null;
        }

        public class ChildPropertyWidget: VarmintWidget
        {
            public ChildPropertyWidget Foo { get; set; }
        }

        [TestMethod]
        public void ChildPropertySettingWorks()
        {
            var layoutText =
                @"
<ChildPropertyWidget>
    <Grid Name=""Gumby"" />
    <ChildPropertyWidget.Foo>
        <ChildPropertyWidget Name=""greg"" />
    </ChildPropertyWidget.Foo>
</ChildPropertyWidget>";
            var bindToMe = new BindingThing();

            var target = (ChildPropertyWidget) TestUtils.LoadFromText(this, layoutText, "CP");
            Assert.AreEqual(true, target.HasChildren);
            Assert.AreEqual("Gumby", target.Children.First().Name);
            Assert.AreEqual("greg", target.Foo.Name);
        }

        [TestMethod]
        public void ChildPropertyThrowsIfMoreThanOneDeep()
        {
            var layoutText =
                @"
<ChildPropertyWidget>
    <ChildPropertyWidget.Foo.Foo>
        <Grid Name=""greg"" />
    </ChildPropertyWidget.Foo.Foo>
</ChildPropertyWidget>";
            var bindToMe = new BindingThing();

            Assert.AreEqual("Property setter specification is too deep.  Only one dot allowed! (ChildPropertyWidget.Foo.Foo)",
                Assert.ThrowsException<ApplicationException>(() => { TestUtils.LoadFromText(this, layoutText, "CP"); }).Message);
        }

        [TestMethod]
        public void ChildPropertyIsNullWhenChildContentNotSpecified()
        {
            var layoutText =
                @"
<ChildPropertyWidget>
    <ChildPropertyWidget.Foo>
    </ChildPropertyWidget.Foo>
</ChildPropertyWidget>";
            var bindToMe = new BindingThing();

            var target = (ChildPropertyWidget)TestUtils.LoadFromText(this, layoutText, "CP");
            Assert.AreEqual(false, target.HasChildren);
            Assert.AreEqual(null, target.Foo);
        }

        [TestMethod]
        public void ChildPropertyThrowsIfTooManyChildren()
        {
            var layoutText =
                @"
<ChildPropertyWidget>
    <ChildPropertyWidget.Foo>
        <ChildPropertyWidget Name=""greg"" />
        <ChildPropertyWidget Name=""bob"" />
    </ChildPropertyWidget.Foo>
</ChildPropertyWidget>";
            var bindToMe = new BindingThing();

            Assert.AreEqual("Too many child nodes on a property setter.  You only get one! (ChildPropertyWidget.Foo)",
                Assert.ThrowsException<ApplicationException>(() => { TestUtils.LoadFromText(this, layoutText, "CP"); }).Message);
        }

        [TestMethod]
        public void ChildPropertyThrowsIfBadType()
        {
            var layoutText =
                @"
<ChildPropertyWidget>
    <ChildPropertyWidget.Name>
        <ChildPropertyWidget Name=""greg"" />
    </ChildPropertyWidget.Name>
</ChildPropertyWidget>";
            var bindToMe = new BindingThing();

            Assert.AreEqual("Property ChildPropertyWidget.Name cannot be assigned Type ChildPropertyWidget",
                Assert.ThrowsException<ApplicationException>(() => { TestUtils.LoadFromText(this, layoutText, "CP"); }).Message);
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

            var target = TestUtils.LoadFromText(this,layoutText, "Barney");
            target.BindingContext = bindToMe;
            target.Prepare(new Dictionary<string, VarmintWidgetStyle>());
            Assert.AreEqual("Barney", target.Name);
            Assert.AreEqual("Bar,that", target.Parameters["Foo"]);
            Assert.AreEqual("223", target.Parameters["Blech"]);
            Assert.AreEqual(0.1f, target.Offset.X);
            Assert.AreEqual(0.2f, target.Offset.Y);
            Assert.AreEqual(10, target.SpecifiedSize.Item1);
            Assert.AreEqual(11, target.SpecifiedSize.Item2);
            target.HandleTap(new Vector2(1, 1));
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

            var widgetSpace = new VarmintWidgetSpace(new MockRenderer(100,100));
            widgetSpace.AddContent("Fooz", styleText);
            widgetSpace.AddContent("Freee", globalStyleText);
            widgetSpace.AddContent("StyleTest", layoutText);

            var target = widgetSpace.FindWidgetByName("StyleTest");
            target.BindingContext = bindToMe;
            target.Prepare(widgetSpace.StyleLibrary);
            target.UpdateFormatting(new Vector2(100, 100));

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
