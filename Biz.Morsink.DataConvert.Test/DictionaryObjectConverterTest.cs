using Biz.Morsink.DataConvert.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.DataConvert.Test
{
    [TestClass]
    public class DictionaryObjectConverterTest
    {
        private DataConverter converter;

        private class PersonC
        {
            public PersonC(string firstName, string lastName, int age)
            {
                FirstName = firstName;
                LastName = lastName;
                Age = age;
            }
            public string FirstName { get; }
            public string LastName { get; }
            public int Age { get; }
        }
        public class PersonS
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public int Age { get; set; }
        }
        [TestInitialize]
        public void Init()
        {
            converter = new DataConverter(
                IdentityConverter.Instance,
                new TryParseConverter(),
                new ToStringConverter(true),
                new DictionaryObjectConverter(),
                new DynamicConverter());
        }
        [TestMethod]
        public void DictObj_HappyConstructor()
        {
            var d = new Dictionary<string, object>
            {
                ["FirstName"] = "Joost",
                ["LastName"] = "Morsink",
                ["Age"] = "37"
            };
            Assert.IsTrue(converter.Convert(d).TryTo(out PersonC p), "Conforming dictionary should be convertible using parameterized constructor.");
            Assert.AreEqual("Joost", p.FirstName, "Conforming dictionary should be preserve value of FirstName");
            Assert.AreEqual("Morsink", p.LastName, "Conforming dictionary should be preserve value of LastName");
            Assert.AreEqual(37, p.Age, "Conforming dictionary should convert the value of Age");
        }
        [TestMethod]
        public void DictObj_HappySetter()
        {
            var d = new Dictionary<string, string>
            {
                ["FirstName"] = "Joost",
                ["LastName"] = "Morsink",
                ["Age"] = "37"
            };

            Assert.IsTrue(converter.Convert(d).TryTo(out PersonS p), "Conforming dictionary should be convertible using setters");
            Assert.AreEqual("Joost", p.FirstName, "Conforming dictionary should be preserve value of FirstName");
            Assert.AreEqual("Morsink", p.LastName, "Conforming dictionary should be preserve value of LastName");
            Assert.AreEqual(37, p.Age, "Conforming dictionary should convert the value of Age");
        }
        [TestMethod]
        public void DictObj_HappyToDict()
        {
            var p = new PersonC("Joost", "Morsink", 37);
            Assert.IsTrue(converter.Convert(p).TryTo(out Dictionary<string, string> d), "An object should be convertible to a dictionary");
            Assert.AreEqual(3, d.Count, "Number of readable properties should equal the number of entries in the dictionary");
            Assert.AreEqual(p.FirstName, d["FirstName"], "Property value FirstName should be preserved in dictionary");
            Assert.AreEqual(p.LastName, d["LastName"], "Property value LastName should be preserved in dictionary");
            Assert.AreEqual(p.Age.ToString(), d["Age"], "Property value Age should be converted in dictionary");
        }
        [TestMethod]
        public void DictObj_UnhappyValue()
        {
            var d = new Dictionary<string, string>
            {
                ["FirstName"] = "Joost",
                ["LastName"] = "Morsink",
                ["Age"] = "37x"
            };
            Assert.IsFalse(converter.Convert(d).TryTo(out PersonC p), "Unparseable int should fail entire conversion for parameterized constructor types");
            Assert.IsFalse(converter.Convert(d).TryTo(out PersonS q), "Unparseable int should fail entire conversion for property setter types");
        }
        [TestMethod]
        public void DictObj_UnhappyKey()
        {
            var d = new Dictionary<string, string>
            {
                ["FirstName"] = "Joost",
                ["LastName"] = "Morsink",
                ["Ages"] = "37"
            };
            Assert.IsFalse(converter.Convert(d).TryTo(out PersonC p), "Non-existent key should fail entire conversion for parameterized constructor types");
            Assert.IsFalse(converter.Convert(d).TryTo(out PersonS q), "Non-existent key should fail entire conversion for property setter types");
        }
    }
}
