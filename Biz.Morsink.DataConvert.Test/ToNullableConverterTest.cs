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
            Assert.AreEqual(42, converter.Convert(42).To<int?>(), "ToNullableConverter should preserve value of inner int conversion");
            Assert.AreEqual(3.14159m, converter.Convert(3.14159m).To<decimal?>(), "ToNullableConverter should preserve value of inner decimal conversion");
        }
        [TestMethod]
        public void ToNullable_NoUnhappys()
        {
            Assert.IsTrue(converter.DoConversion<double, int?>(3.14159).IsSuccessful, "ToNullableConverter should not convert by itself (double to int)");
            Assert.AreEqual(default(int?), converter.Convert<short>(42).To<int?>(), "ToNullableConverter should not convert by itself (short to int)");
        }

    }
}
