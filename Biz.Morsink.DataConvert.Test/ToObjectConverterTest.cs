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
            converter = new DataConverter(ToObjectConverter.Instance);
        }

        [TestMethod]
        public void ToObj_Happy()
        {
            Assert.IsTrue(converter.Convert(42).TryTo(out object x));
            Assert.AreEqual(42, x);
            Assert.IsTrue(converter.Convert<string>(null).TryTo(out x));
            Assert.IsNull(x);
        }

    }
}
