using Biz.Morsink.DataConvert.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Biz.Morsink.DataConvert.Test
{
    [TestClass]
    public class BooleanConverterTest
    {
        private DataConverter converter;

        [TestInitialize]
        public void Init()
        {
            converter = new DataConverter(BooleanConverter.Instance);
        }
        [TestMethod]
        public void FromBool_Happy()
        {
            Assert.IsTrue(converter.Convert(true).TryTo(out int x), "Conversion from boolean should always succeed.");
            Assert.AreEqual(1, x, "True should convert to 1.");
            Assert.IsTrue(converter.Convert(false).TryTo(out x), "Conversion from boolean should always succeed.");
            Assert.AreEqual(0, x, "False should convert to 0.");
            Assert.IsTrue(converter.Convert(true).TryTo(out short y), " Conversion from boolean should always succeed.");
            Assert.AreEqual(1, y, "True should convert to 1.");
            Assert.IsTrue(converter.Convert(false).TryTo(out y), " Conversion from boolean should always succeed.");
            Assert.AreEqual(0, y, "True should convert to 0.");
            Assert.IsTrue(converter.Convert(true).TryTo(out string s), "Conversion from boolean should always succeed.");
            Assert.AreEqual("true", s.ToLowerInvariant(), "True should convert to some casing of 'true'.");
            Assert.IsTrue(converter.Convert(false).TryTo(out s), "Conversion from boolean should always succeed.");
            Assert.AreEqual("false", s.ToLowerInvariant(), "False should convert to some casing of 'false'.");
        }

        [TestMethod]
        public void ToBool_Happy()
        {
            Assert.IsTrue(converter.Convert(1).TryTo(out bool x) && x, "1 should convert to bool value true.");
            Assert.IsTrue(converter.Convert(123).TryTo(out x) && x, "Non-zero integers should convert to bool value true.");
            Assert.IsTrue(converter.Convert(0).TryTo(out x) && !x, "0 should convert to bool value false.");
            Assert.IsTrue(converter.Convert((short)1).TryTo(out x) && x, "Short 1 should convert to bool value true.");
            Assert.IsTrue(converter.Convert((short)0).TryTo(out x) && !x, "Short 0 should convert to bool value false.");
            Assert.IsTrue(converter.Convert("TrUe").TryTo(out x) && x, "Some casing of true should convert to bool value true.");
            Assert.IsTrue(converter.Convert("FaLsE").TryTo(out x) && !x, "Some casing of false should convert to bool value false.");
            Assert.IsTrue(converter.Convert("1").TryTo(out x) && x, "1-string should convert to bool value true.");
            Assert.IsTrue(converter.Convert("123").TryTo(out x) && x, "Non-zero integer-strings should convert to bool value true.");
            Assert.IsTrue(converter.Convert("0").TryTo(out x) && !x, "0-string should convert to bool value false.");
        }

        [TestMethod]
        public void ToBool_Unhappy()
        {
            Assert.IsFalse(converter.Convert("sdf").TryTo(out bool x), "Unparseable strings should fail.");
            Assert.IsFalse(converter.Convert("").TryTo(out x), "Empty string should fail.");
        }
    }
}
