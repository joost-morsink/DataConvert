using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Biz.Morsink.DataConvert.Converters;

namespace Biz.Morsink.DataConvert.Test
{
    [TestClass]
    public class TupleToArrayConverterTest
    {
        private DataConverter converter;

        [TestInitialize]
        public void Init()
        {
            converter = new DataConverter(IdentityConverter.Instance, TupleToArrayConverter.Instance);
        }
        [TestMethod]
        public void Tuple2Array_Happy()
        {
            var arr = converter.Convert(Tuple.Create(1, 2, 3)).To<int[]>();
            Assert.IsNotNull(arr, "Tuple should convert to an array");
            Assert.AreEqual(1, arr[0],"First component value of Tuple should convert to first array element");
            Assert.AreEqual(2, arr[1], "Second component value of Tuple should convert to second array element");
            Assert.AreEqual(3, arr[2], "Third component value of Tuple should convert to third array element");
            Assert.AreEqual(3, arr.Length, "Array length should match Tuple arity");

            arr = converter.Convert((1, 2, 3)).To<int[]>();
            Assert.IsNotNull(arr, "ValueTuple should convert to an array");
            Assert.AreEqual(1, arr[0], "First component value of ValueTuple should convert to first array element");
            Assert.AreEqual(2, arr[1], "Second component value of ValueTuple should convert to second array element");
            Assert.AreEqual(3, arr[2], "Third component value of ValueTuple should convert to third array element");
            Assert.AreEqual(3, arr.Length, "Array length should match ValueTuple arity");
        }
        [TestMethod]
        public void Tuple2Array_Unhappy()
        {
            var res = converter.DoConversion<Tuple<string, int, int>, int[]>(Tuple.Create("1", 2, 3));
            Assert.IsFalse(res.IsSuccessful, "Component failure should fail Tuple to array conversion");
            res = converter.DoConversion<(string, int, int), int[]>(("1", 2, 3));
            Assert.IsFalse(res.IsSuccessful, "Component failure should fail ValueTuple to array conversion");
        }
    }
}
