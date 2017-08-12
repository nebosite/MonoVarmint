using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonoVarmint.Widgets;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace MonoVarmint.Tools.Tests
{
    [TestClass]
    public class UIHelpersTests
    {
        [TestMethod]
        public void GetValueFromText_HandlesSimpleValues()
        {
            Assert.AreEqual("hi", UIHelpers.GetValueFromText(typeof(String), "hi"));
            Assert.AreEqual(new Vector2(1, 2), UIHelpers.GetValueFromText(typeof(Vector2), "1,2"));
            Assert.AreEqual(new Point(3, 4), UIHelpers.GetValueFromText(typeof(Point), "3,4"));
            Assert.AreEqual(0.1f, UIHelpers.GetValueFromText(typeof(Single), ".1"));
            Assert.AreEqual(0.2f, UIHelpers.GetValueFromText(typeof(float), "0.2"));
            Assert.AreEqual(0.33, UIHelpers.GetValueFromText(typeof(double), "0.33"));
            Assert.AreEqual(4, UIHelpers.GetValueFromText(typeof(Int32), "4"));
            Assert.AreEqual(5, UIHelpers.GetValueFromText(typeof(int), "5"));
            Assert.AreEqual(60000000000, UIHelpers.GetValueFromText(typeof(Int64), "60000000000"));
            Assert.AreEqual(60000000000, UIHelpers.GetValueFromText(typeof(long), "60000000000"));
            Assert.AreEqual(true, UIHelpers.GetValueFromText(typeof(Boolean), "true"));
            Assert.AreEqual(false, UIHelpers.GetValueFromText(typeof(bool), "false"));
            Assert.AreEqual(Color.Red, UIHelpers.GetValueFromText(typeof(Color), "#ff0000"));
            Assert.AreEqual("hi", UIHelpers.GetValueFromText(typeof(Object), "hi"));
        }


        [TestMethod]
        public void GetValueFromText_HandlesEnums()
        {
            Assert.AreEqual(VarmintWidget.EventHandledState.NotHandled, UIHelpers.GetValueFromText(typeof(VarmintWidget.EventHandledState), "NotHandled"));
        }

        class FooClass
        {
            public string Stuff;
            public FooClass(string stuff)
            {
                Stuff = stuff;
            }
        }

        [TestMethod]
        public void GetValueFromText_HandlesClasses()
        {
            FooClass item = (FooClass)(UIHelpers.GetValueFromText(typeof(FooClass), "zed"));
            Assert.AreEqual("zed", item.Stuff);
        }


        [TestMethod]
        public void GetValueFromText_HandlesTuples()
        {
            var expectedValue = new Tuple<int, float>(5, 1.6f);
            Assert.AreEqual(expectedValue, UIHelpers.GetValueFromText(expectedValue.GetType(), "5:1.6"));

            var expectedValue2 = new Tuple<float, int>(3.1f, 33);
            Assert.AreEqual(expectedValue2, UIHelpers.GetValueFromText(expectedValue2.GetType(), "3.1:33"));

            var expectedValue3 = new Tuple<float, int>(4.1f, 18);
            Assert.AreEqual(expectedValue3, UIHelpers.GetValueFromText(expectedValue2.GetType(), "(4.1:18)"));
        }

        [TestMethod]
        public void GetValueFromText_HandlesArrays()
        {
            AssertArraysEqual(new int[] { 1, 2, 3 }, (Array)UIHelpers.GetValueFromText(typeof(int[]), "1,2,3"));

            AssertArraysEqual(new float[] { 3.2f, 7.7f, 8.9f }, (Array)UIHelpers.GetValueFromText(typeof(float[]), "3.2, 7.7, 8.9"));

            var values = new List<Tuple<int, float>>();
            values.Add(new Tuple<int, float>(1, 1.2f));
            values.Add(new Tuple<int, float>(99, 88.8f));
            var expected = values.ToArray();
            AssertArraysEqual(expected, (Array)UIHelpers.GetValueFromText(expected.GetType(), "(1:1.2),(99:88.8)"));
        }

        void AssertArraysEqual(Array array1, Array array2)
        {
            if(array1.Length != array2.Length)
            {
                Assert.Fail("Arrays are not the same length");
            }

            for(int i = 0; i < array1.Length; i++)
            {
                if (!array1.GetValue(i).Equals(array2.GetValue(i))) Assert.Fail($"Arrays different at index {i}");
            }
        }
    }
}
