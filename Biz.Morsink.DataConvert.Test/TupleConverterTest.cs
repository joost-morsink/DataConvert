using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Biz.Morsink.DataConvert.Converters;

namespace Biz.Morsink.DataConvert.Test
{
    [TestClass]
    public class TupleConverterTest
    {
        private DataConverter converter;

        [TestInitialize]
        public void Init()
        {
            converter = new DataConverter(new IdentityConverter(), new ToStringConverter(true), new TupleConverter());
        }
        [TestMethod]
        public void Tuple_Happy()
        {
            Assert.IsTrue(converter.Convert(Tuple.Create(1, 2.3, "4", true, 5)).TryTo<Tuple<string, string, string, string, int>>(out var res));
            Assert.AreEqual("1", res.Item1);
            Assert.AreEqual("2.3", res.Item2);
            Assert.AreEqual("4", res.Item3);
            Assert.AreEqual("True", res.Item4);
            Assert.AreEqual(5, res.Item5);
        }
        [TestMethod]
        public void Tuple_ValueTuples()
        {
            Assert.IsTrue(converter.Convert((42, 3.14)).TryTo<ValueTuple<string, string>>(out var res));
            Assert.AreEqual("42", res.Item1);
            Assert.AreEqual("3.14", res.Item2);
        }
        [TestMethod]
        public void Tuple_DifferentTuples()
        {
            Assert.IsTrue(converter.Convert((42, 43)).TryTo<Tuple<int, int>>(out var res));
            Assert.AreEqual(42, res.Item1);
            Assert.AreEqual(43, res.Item2);
            (var x, var y) = converter.Convert(Tuple.Create(37, 38)).To<(int, int)>();
            Assert.IsTrue(converter.Convert(Tuple.Create(37, 38)).TryTo<(int, int)>(out var vres));
            Assert.AreEqual(37, vres.Item1);
            Assert.AreEqual(38, vres.Item2);
        }
        [TestMethod]
        public void Tuple_Unhappy()
        {
            Assert.IsFalse(converter.Convert(Tuple.Create(42, "3.14")).TryTo<Tuple<string, double>>(out var res));
        }
    }
}
