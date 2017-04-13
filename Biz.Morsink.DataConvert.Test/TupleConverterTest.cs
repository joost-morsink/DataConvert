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
            Assert.IsTrue(converter.Convert(Tuple.Create(1, 2.3, "4", true, 5)).TryTo<Tuple<string, string, string, string, int>>(out var res), "Tuple conversion should succeed if all components succeed");
            Assert.AreEqual("1", res.Item1, "First component int->string should succeed.");
            Assert.AreEqual("2.3", res.Item2, "Second component double->string should succeed.");
            Assert.AreEqual("4", res.Item3, "Third component string->string should succeed.");
            Assert.AreEqual("True", res.Item4, "Fourth component bool->string should succeed.");
            Assert.AreEqual(5, res.Item5, "Fifth component int->int should succeed.");
        }
        [TestMethod]
        public void Tuple_ValueTuples()
        {
            Assert.IsTrue(converter.Convert((42, 3.14)).TryTo<ValueTuple<string, string>>(out var res), "ValueTuple conversion should succeed if all components succeed");
            Assert.AreEqual("42", res.Item1, "First ValueTuple component int->string should succeed");
            Assert.AreEqual("3.14", res.Item2, "Second ValueTuple component double->string should succeed");
        }
        [TestMethod]
        public void Tuple_DifferentTuples()
        {
            Assert.IsTrue(converter.Convert((42, 43)).TryTo<Tuple<int, int>>(out var res), "ValueTuple should convert to Tuple");
            Assert.AreEqual(42, res.Item1, "ValueTuple to Tuple conversion should preserve first component.");
            Assert.AreEqual(43, res.Item2, "ValueTuple to Tuple conversion should preserve second component.");
            (var x, var y) = converter.Convert(Tuple.Create(37, 38)).To<(int, int)>();
            Assert.IsTrue(converter.Convert(Tuple.Create(37, 38)).TryTo<(int, int)>(out var vres));
            Assert.AreEqual(37, vres.Item1, "Tuple to ValueTuple conversion should preserve first component.");
            Assert.AreEqual(38, vres.Item2, "Tuple to ValueTuple conversion should preserve second component.");
        }
        [TestMethod]
        public void Tuple_Unhappy()
        {
            Assert.IsFalse(converter.Convert(Tuple.Create(42, "3.14")).TryTo<Tuple<string, double>>(out var res), "Component conversion failure should fail tuple conversion");
        }
    }
}
