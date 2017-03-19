using Biz.Morsink.DataConvert.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            converter = new DataConverter(new[] { new EnumParseConverter(true) });
        }

        [TestMethod]
        public void EnumParse_HappyNormal()
        {
            Assert.AreEqual(TestEnum.Abc, converter.Convert("Abc").To<TestEnum>());
            Assert.AreEqual(TestEnum.Def, converter.Convert("def").To<TestEnum>());
            Assert.AreEqual(TestEnum.Ghi, converter.Convert("GHI").To<TestEnum>());
        }
        [TestMethod]
        public void EnumParse_HappyFlags()
        {
            Assert.AreEqual(TestFlags.One | TestFlags.Three | TestFlags.Five, converter.Convert("One, Three, Five").To<TestFlags>());
            Assert.AreEqual(TestFlags.Two | TestFlags.Four, converter.Convert("10").To<TestFlags>());
        }

        [TestMethod]
        public void EnumParse_Unhappy()
        {
            Assert.IsFalse(converter.DoConversion<string, TestEnum>("ghij").IsSuccessful);
        }
    }
}
