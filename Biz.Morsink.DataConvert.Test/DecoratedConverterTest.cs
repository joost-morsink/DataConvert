using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Biz.Morsink.DataConvert.Converters;

namespace Biz.Morsink.DataConvert.Test
{
    [TestClass]
    public class DecoratedConverterTest
    {
        private DataConverter converter;

        [TestInitialize]
        public void Init()
        {
            converter = new DataConverter(IdentityConverter.Instance, new ToNullableConverter().Restrict((f, t) => f != typeof(int)));
        }
        [TestMethod]
        public void Decorated_Happy()
        {
            Assert.IsTrue(converter.DoConversion<double, int?>(1.0).IsSuccessful);
            Assert.IsFalse(converter.Convert(1.0).To<int?>().HasValue);
        }
        [TestMethod]
        public void Decorated_Unhappy()
        {
            Assert.IsFalse(converter.DoConversion<int, int?>(1).IsSuccessful);
        }
    }
}
