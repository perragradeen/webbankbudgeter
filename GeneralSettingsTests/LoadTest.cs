using GeneralSettingsHandler;
using XmlSerializer;

namespace GeneralSettingsTests
{
    [TestClass]
    public class LoadTest
    {
        [TestMethod]
        public void LoadAndSerializeTest()
        {
            var filePath = @"Data\GeneralSettings.xml";
            var result = SerializationFunctions
                .DeserializeObject<GeneralSettings>(filePath);

            Assert.IsTrue(result != null);
            Assert.IsTrue(result.Any());
            Assert.AreEqual(5, result.Count);

            var last = result[4];
            Assert.AreEqual("swedbank", last.Value);
            Assert.AreEqual("BankUrl", last.Name);
        }

        [TestMethod]
        public void GetSettingTest()
        {
            var filePath = @"Data\GeneralSettings.xml";
            var target = new GeneralSettingsGetter(filePath);
            var result = target.AllGeneralSettings;

            Assert.IsTrue(result != null);
            Assert.IsTrue(result.Any());
            Assert.AreEqual(5, result.Count);

            var last = result[4];
            Assert.AreEqual("swedbank", last.Value);
            Assert.AreEqual("BankUrl", last.Name);
        }

        [TestMethod]
        public void GetFileSettingsTest()
        {
            var filePath = @"Data\GeneralSettings.xml";
            var target = new GeneralSettingsGetter(filePath);
            var result = target.GetTextFileStringSetting("BankUrl");

            Assert.IsTrue(result != null);

            Assert.IsTrue(result.Length > 0);
        }

    }
}
