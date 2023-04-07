using Biz.Morsink.DataConvert.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Biz.Morsink.DataConvert.Test
{
    [TestClass]
    public class ConverterClassConverterTest
    {
        public class Converter
        {
            public static ConversionResult<int> Convert(string str)
                => int.TryParse(str, out var result) ? new ConversionResult<int>(result) : default;

            public ConversionResult<uint> NonStaticWrongConvert(string str)
                => default;

            public static string Convert(int i)
                => i.ToString();
        }

        private DataConverter converter;

        [TestInitialize]
        public void Init()
        {
            converter= new DataConverter(ConverterClassConverter<Converter>.Instance);
        }
        [TestMethod]
        public void ConverterClassConverter_Happy()
        {
            Assert.IsTrue(converter.Convert("123").TryTo(out int result));
            Assert.AreEqual(123, result);
        }
        [TestMethod]
        public void ConverterClassConverter_Unhappy()
        {
            Assert.IsFalse(converter.Convert("hello").TryTo(out int _));
        }
        [TestMethod]
        public void ConverterClassConverter_DirectHappy()
        {
            Assert.IsTrue(converter.Convert(123).TryTo(out string x));
            Assert.AreEqual("123", x);
        }

        [TestMethod]
        public void ConverterClassConverter_WrongType()
        {
            Assert.IsFalse(converter.Convert("123").TryTo(out uint _));
            Assert.IsFalse(converter.Convert("123").TryTo(out long _));
        }
    }
}