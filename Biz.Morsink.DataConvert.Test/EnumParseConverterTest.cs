using Biz.Morsink.DataConvert.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Biz.Morsink.DataConvert.Test
{
    [TestClass]
    public class EnumParseConverterTest
    {
        public enum TestEnum { Abc = 1, Def = 2, Ghi = 3 }
        [Flags]
        public enum TestFlags { One = 1, Two = 2, Three = 4, Four = 8, Five = 16 }
        private DataConverter converter;

        [TestInitialize]
        public void Init()
        {
            converter = new DataConverter(new EnumParseConverter(true));
        }

        [TestMethod]
        public void EnumParse_HappyNormal()
        {
            Assert.AreEqual(TestEnum.Abc, converter.Convert("Abc").To<TestEnum>(), "Enum string value should be parseable");
            Assert.AreEqual(TestEnum.Def, converter.Convert("def").To<TestEnum>(), "Enum string value should be case insensitive (lower)");
            Assert.AreEqual(TestEnum.Ghi, converter.Convert("GHI").To<TestEnum>(), "Enum string value should be case insensitive (upper)");
        }
        [TestMethod]
        public void EnumParse_HappyFlags()
        {
            Assert.AreEqual(TestFlags.One | TestFlags.Three | TestFlags.Five, converter.Convert("One, Three, Five").To<TestFlags>(), "Flags enum should parse comma separated flag names");
            Assert.AreEqual(TestFlags.Two | TestFlags.Four, converter.Convert("10").To<TestFlags>(), "Flags enum should parse numeric strings");
        }

        [TestMethod]
        public void EnumParse_Unhappy()
        {
            Assert.IsFalse(converter.DoConversion<string, TestEnum>("ghij").IsSuccessful, "Unresolved enum names should fail");
        }
    }
}
