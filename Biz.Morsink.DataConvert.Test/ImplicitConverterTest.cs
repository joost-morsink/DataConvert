using Biz.Morsink.DataConvert.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Biz.Morsink.DataConvert.Test
{
    [TestClass]
    public class ImplicitConverterTest
    {
        private DataConverter converter;

        [TestInitialize]
        public void Init()
        {
            converter = new DataConverter(ImplicitOperatorConverter.Instance);
        }

        [TestMethod]
        public void ImplicitOperatorFromOrigin()
        {
            var conv = converter.GetConverter<Email, string>();
            Assert.IsTrue(conv(new Email("hello")).IsSuccessful);
            Assert.AreEqual("hello", conv(new Email("hello")).Result);
        }
        [TestMethod]
        public void ImplicitOperatorFromDestination()
        {
            var conv = converter.GetConverter<string, Email>();
            Assert.IsTrue(conv("hello").IsSuccessful);
            Assert.AreEqual("hello", conv("hello").Result.Value);
        }
    }
}