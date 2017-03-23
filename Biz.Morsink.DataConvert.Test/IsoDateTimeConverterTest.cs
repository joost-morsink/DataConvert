using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Biz.Morsink.DataConvert.Converters;

namespace Biz.Morsink.DataConvert.Test
{
    [TestClass]
    public class IsoDateTimeConverterTest
    {
        private DataConverter converter;

        [TestInitialize]
        public void Init()
        {
            converter = new DataConverter(IsoDateTimeConverter.Instance);
        }
        [TestMethod]
        public void IsoDate_Happy()
        {
            Assert.AreEqual(new DateTime(2017, 3, 16, 19, 31, 0, DateTimeKind.Utc),
                converter.Convert("2017-03-16T19:31:00Z").To<DateTime>());
            Assert.AreEqual(new DateTimeOffset(2017, 3, 16, 20, 31, 0, TimeSpan.FromHours(1.0)),
                converter.Convert("2017-03-16T20:31:00+01:00").To<DateTime>());
            Assert.AreEqual("2017-03-16T19:31:00.000Z",
                converter.Convert(new DateTime(2017, 3, 16, 19, 31, 0, DateTimeKind.Utc)).To<string>());
            Assert.AreEqual("2017-03-16T20:31:00.000+01:00",
                converter.Convert(new DateTimeOffset(2017, 3, 16, 20, 31, 0, TimeSpan.FromHours(1.0))).To<string>());

        }
    }
}
