using Biz.Morsink.DataConvert.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.DataConvert.Test
{
    [TestClass]
    public class ToObjectConverterTest
    {
        private DataConverter converter;

        [TestInitialize]
        public void Init()
        {
            converter = new DataConverter(ToObjectConverter.Instance, TupleToArrayConverter.Instance);
        }

        [TestMethod]
        public void ToObj_Happy()
        {
            Assert.IsTrue(converter.Convert(42).TryTo(out object x));
            Assert.AreEqual(42, x);
            Assert.IsTrue(converter.Convert<string>(null).TryTo(out x));
            Assert.IsNull(x);
        }
        [TestMethod]
        public void ToObj_Indirect()
        {
            Assert.IsTrue(converter.Convert((1, 2, 3)).TryTo(out object[] x));
            Assert.AreEqual(3, x.Length);
            Assert.AreEqual(1, x[0]);
            Assert.AreEqual(2, x[1]);
            Assert.AreEqual(3, x[2]);
            Assert.IsTrue(converter.Convert((42, "hoi", new Dictionary<string, string>())).TryTo(out x));
            Assert.AreEqual(3, x.Length);
            Assert.AreEqual(42, x[0]);
            Assert.AreEqual("hoi", x[1]);
            Assert.IsInstanceOfType(x[2], typeof(Dictionary<string, string>));

        }

    }
}
