using Biz.Morsink.DataConvert.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Biz.Morsink.DataConvert.Test
{
    [TestClass]
    public class StaticMethodConverterTest
    {
        private DataConverter converter;
        [TestInitialize]
        public void Init()
        {
            converter = new DataConverter(StaticMethodConverter.Instance);
        }
        
        [TestMethod]
        public void StaticMethod_Happy()
        {
            Assert.IsTrue(converter.Convert("hello@test.com").TryTo(out Email result));
            Assert.AreEqual("hello@test.com", result.Value);
        }
        [TestMethod]
        public void StaticMethod_Unhappy()
        {
            Assert.IsFalse(converter.Convert("hello").TryTo(out Email _));
        }
    }
}