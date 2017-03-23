using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Biz.Morsink.DataConvert.Converters;

namespace Biz.Morsink.DataConvert.Test
{
    [TestClass]
    public class ToNullableConverterTest
    {
        private DataConverter converter;

        [TestInitialize]
        public void Init()
        {
            converter = new DataConverter(IdentityConverter.Instance, new ToNullableConverter());
        }
        [TestMethod]
        public void ToNullable_Happy()
        {
            Assert.AreEqual(42, converter.Convert(42).To<int?>());
            Assert.AreEqual(3.14159m, converter.Convert(3.14159m).To<decimal?>());
        }
        [TestMethod]
        public void ToNullable_NoUnhappys()
        {
            Assert.IsTrue(converter.DoConversion<double, int?>(3.14159).IsSuccessful);
            Assert.AreEqual(default(int?), converter.Convert<short>(42).To<int?>());
        }

    }
}
