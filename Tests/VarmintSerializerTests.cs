using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonoVarmint.Tools;

namespace Tests
{
    [TestClass]
    public class VarmintSerializerTests
    {
        T Crunch<T>(T serializeMe)
        {
            var data = VarmintSerializer.Serialize(serializeMe);
            System.Diagnostics.Debug.WriteLine(data);
            return (T)VarmintSerializer.DeSerialize(data);
        }

        class FooBag
        {
            public string Name {get; set;}
            public uint Age { get; set; }
            public int Speed { get; set; }
            public long BigNum { get; set; }
            public ulong BiggerNum { get; set; }
            public short LittleNum { get; set; }
            public ushort LittleNum2 { get; set; }
            public char Letter { get; set; }
            public byte Byte { get; set; }
            public DateTime Date { get; set; }
            public double MathNum { get; set; }
            public float GraphicsNum { get; set; }
            public bool BooleanValue { get; set; }
        }

        [TestMethod]
        public void Serializer_CanSerialize_SimpleTypes()
        {
            var thing1 = new FooBag() {
                Name = "Scooby \"with\" special characters\n\rWow!",
                Age = uint.MaxValue,
                Speed = int.MinValue,
                BigNum = long.MinValue,
                BiggerNum = ulong.MaxValue,
                LittleNum = short.MinValue,
                LittleNum2 = ushort.MaxValue,
                Letter = 'z',
                Byte = 253,
                Date = DateTime.Now,
                MathNum = 2.2,
                GraphicsNum = 1.1f,
                BooleanValue = true,
            };  

            var thing2 = Crunch<FooBag>(thing1);

            Assert.AreEqual(thing1.Name, thing2.Name);
            Assert.AreEqual(thing1.Age, thing2.Age);
            Assert.AreEqual(thing1.Speed, thing2.Speed);
            Assert.AreEqual(thing1.BigNum, thing2.BigNum);
            Assert.AreEqual(thing1.BiggerNum, thing2.BiggerNum);
            Assert.AreEqual(thing1.LittleNum, thing2.LittleNum);
            Assert.AreEqual(thing1.LittleNum2, thing2.LittleNum2);
            Assert.AreEqual(thing1.Letter, thing2.Letter);
            Assert.AreEqual(thing1.Byte, thing2.Byte);
            Assert.AreEqual(thing1.Date.ToString(), thing2.Date.ToString());
            Assert.AreEqual(thing1.MathNum, thing2.MathNum);
            Assert.AreEqual(thing1.GraphicsNum, thing2.GraphicsNum);
            Assert.AreEqual(thing1.BooleanValue, thing2.BooleanValue);
        }

        class FooGraph
        {
            public string Name { get; set; }
            public FooGraph Left { get; set; }
            public FooGraph Right { get; set; }

            public FooGraph() { }
            public FooGraph(string name) { Name = name; } 
        }

        [TestMethod]
        public void Serializer_CanSerialize_ObjectsAndGraphs()
        {
            var thingA = new FooGraph("A");
            var thingB = new FooGraph("B");
            var thingC = new FooGraph("C");
            thingA.Left = thingB;
            thingA.Right = thingC;
            thingA.Left.Right = thingA;
            thingA.Right.Left = thingA;
            thingA.Left.Left = thingB;

            var thing2 = Crunch<FooGraph>(thingA);

            Assert.AreEqual("A", thing2.Name);
            Assert.AreEqual("B", thing2.Left.Name);
            Assert.AreEqual("C", thing2.Right.Name);
            Assert.AreEqual("A", thing2.Left.Right.Name);
            Assert.AreEqual("A", thing2.Right.Left.Name);
            Assert.AreEqual("B", thing2.Left.Left.Name);
            Assert.AreEqual(null, thing2.Right.Right);
            Assert.AreSame(thing2, thing2.Left.Right);
        }

        [TestMethod]
        public void Serializer_CanSerialize_Arrays()
        {
            // Array of strings
            var thing1 = new string[] { "hi", null, "there" };
            var thing2 = Crunch<string[]>(thing1);

            Assert.AreEqual("hi", thing2[0]);
            Assert.AreEqual(null, thing2[1]);
            Assert.AreEqual("there", thing2[2]);
            Assert.AreEqual(3, thing2.Length);

            // Empty array
            thing1 = new string[0] ;
            thing2 = Crunch<string[]>(thing1);
            Assert.AreEqual(0, thing2.Length);

            // Array of numbers
            var numberArray1 = new int[] { 2, 4 };
            var numberArray2 = Crunch<int[]>(numberArray1);
            Assert.AreEqual(2, numberArray2[0]);
            Assert.AreEqual(4, numberArray2[1]);
            Assert.AreEqual(2, numberArray2.Length);

            // Array of objects
            var graphArray1 = new FooGraph[] { new FooGraph("X"), new FooGraph("Y") };
            var graphArray2 = Crunch<FooGraph[]>(graphArray1);
            Assert.AreEqual("X", graphArray2[0].Name);
            Assert.AreEqual("Y", graphArray2[1].Name);
            Assert.AreEqual(2, numberArray2.Length);
        }

        class ArrayBar
        {
            public int[] Stuff { get; set; }
            public int[] Things { get; set; }
        }

        [TestMethod]
        public void Serializer_CanDistinquish_NullAndEmptyArrays()
        {
            var thing1 = new ArrayBar { Stuff = null, Things = new int[] { 1, 2 } };
            var thing2 = Crunch<ArrayBar>(thing1);

            Assert.AreEqual(null, thing2.Stuff);
            Assert.AreEqual(2, thing2.Things.Length);
            Assert.AreEqual(1, thing2.Things[0]);
            Assert.AreEqual(2, thing2.Things[1]);
        }

        class ReadFoo
        {
            public string Name { get; set; }
            public string LowerName { get { return Name.ToLower(); } }
            public string ReadThing { get; private set; }

            public ReadFoo() { }
            public ReadFoo(string thing) { ReadThing = thing; }
        }

        [TestMethod]
        public void Serializer_Ignores_ReadOnlyProperties()
        {
            var thing1 = new ReadFoo("buzz") { Name = "Fred" };
            var thing2 = Crunch<ReadFoo>(thing1);

            Assert.AreEqual("Fred", thing2.Name);
            Assert.AreEqual("fred", thing2.LowerName);
            Assert.AreEqual(null, thing2.ReadThing);
        }

        class AttributeFoo
        {
            public string Name { get; set; }

            [VarmintSerializerIgnore]
            public string IgnoreMe { get; set; }
        }

        [TestMethod]
        public void Serializer_Ignores_PropertiesWithIgnoreAttribute()
        {
            var thing1 = new AttributeFoo() { Name = "Barney", IgnoreMe = "I said ignore me" };
            var thing2 = Crunch<AttributeFoo>(thing1);

            Assert.AreEqual("Barney", thing2.Name);
            Assert.AreEqual(null, thing2.IgnoreMe);
        }

        public enum Flintstone {
            Fred,
            Wilma,
            Pebbles,
            Dino
        }

        class HasEnum
        {
            public Flintstone Flinty { get; set; }
        }

        [TestMethod]
        public void Serializer_Handles_Enumerations()
        {
            var thing1 = new HasEnum() { Flinty = Flintstone.Wilma};
            var thing2 = Crunch<HasEnum>(thing1);

            Assert.AreEqual(Flintstone.Wilma, thing2.Flinty);
        }

    }
}
