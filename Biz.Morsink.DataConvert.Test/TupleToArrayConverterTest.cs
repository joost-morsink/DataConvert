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
            Assert.IsNotNull(arr);
            Assert.AreEqual(1, arr[0]);
            Assert.AreEqual(2, arr[1]);
            Assert.AreEqual(3, arr[2]);
            Assert.AreEqual(3, arr.Length);

            arr = converter.Convert((1, 2, 3)).To<int[]>();
            Assert.IsNotNull(arr);
            Assert.AreEqual(1, arr[0]);
            Assert.AreEqual(2, arr[1]);
            Assert.AreEqual(3, arr[2]);
            Assert.AreEqual(3, arr.Length);
        }
        [TestMethod]
        public void Tuple2Array_Unhappy()
        {
            var res = converter.DoConversion<Tuple<string, int, int>, int[]>(Tuple.Create("1", 2, 3));
            Assert.IsFalse(res.IsSuccessful);
            res = converter.DoConversion<(string, int, int), int[]>(("1", 2, 3));
            Assert.IsFalse(res.IsSuccessful);
        }
    }
}
