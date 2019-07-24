using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Reflection;
using System.Drawing;

namespace SettingsProviderTests
{
    public class SettingsProviderTestsBase
    {
        protected static string settingsFile;

        // Check if saving and reloading works.
        [TestMethod]
        public void SaveAndReloadTest()
        {
            // Check default values
            Assert.AreEqual(3.141, Properties.Settings.Default.ARoamedNumber);
            Assert.AreEqual(new DateTime(1, 1, 1), Properties.Settings.Default.ADateTime);
            // Set new values
            Properties.Settings.Default.ARoamedNumber = -42;
            Properties.Settings.Default.ADateTime = new DateTime(2019, 01, 05);
            Properties.Settings.Default.Save();
            // Reload
            Properties.Settings.Default.Reload();
            Assert.AreEqual(-42, Properties.Settings.Default.ARoamedNumber);
            Assert.AreEqual(new DateTime(2019, 01, 05), Properties.Settings.Default.ADateTime);
        }

        [DataTestMethod]
        [DataRow("A new string")]
        [DataRow("<item><name>Text</name></item>")]
        [DataRow("<html>\r\n<body>\r\nHello\r\n</body>\r\n</html>")]
        public void SaveStringsTest(string s)
        {
            Assert.AreEqual("Hello World!", Properties.Settings.Default.AString);
            Properties.Settings.Default.AString = s;
            Properties.Settings.Default.Save();
            Properties.Settings.Default.Reload();
            Assert.AreEqual(s, Properties.Settings.Default.AString);
        }

        [TestMethod]
        public void ResetTest()
        {
            Assert.AreEqual("Hello World!", Properties.Settings.Default.AString);
            Properties.Settings.Default.AString = "An unwanted new string";
            Properties.Settings.Default.Save();
            Properties.Settings.Default.Reset();
            Assert.AreEqual("Hello World!", Properties.Settings.Default.AString);
        }

        // Serialize a custom object to xml.
        [TestMethod]
        public void SerializeObjectTest()
        {
            Properties.Settings.Default.APerson = new Person("John", "Doe", 42);
            Properties.Settings.Default.Save();
            Properties.Settings.Default.Reload();
            var person = Properties.Settings.Default.APerson;
            Assert.AreEqual("John", person.Name);
            Assert.AreEqual("Doe", person.LastName);
            Assert.AreEqual(42, person.Age);
        }

        // Serialize and reload System.Drawing.Color as binary.
        [TestMethod]
        public void BinarySerializeTest()
        {
            Properties.Settings.Default.AColorAsBinary = Color.FromArgb(0, 255, 0);
            Properties.Settings.Default.Save();
            Properties.Settings.Default.Reload();
            var color = Properties.Settings.Default.AColorAsBinary;
            Assert.AreEqual(0, color.R);
            Assert.AreEqual(255, color.G);
            Assert.AreEqual(0, color.B);
        }

        [TestMethod]
        // Test that no exception is thrown here.
        public void NullReferenceTest()
        {
            Properties.Settings.Default.ARoamedNumber = 333;
            Properties.Settings.Default.Save();
            Properties.Settings.Default.Reload();
            var person1 = Properties.Settings.Default.APerson;
            Properties.Settings.Default.Save();
        }

        // Delete the created settings file and reset the settings after every test.
        [TestCleanup]
        public void TestCleanup()
        {
            File.Delete(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), settingsFile));
            Properties.Settings.Default.Reset();
        }
    }
}
