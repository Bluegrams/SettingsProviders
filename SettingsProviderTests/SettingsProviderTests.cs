using System;
using Bluegrams.Application;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SettingsProviderTests
{
    [TestClass]
    public class PortableSettingsProviderTests : SettingsProviderTestsBase
    {
        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            settingsFile = "portable.config";
            PortableSettingsProvider.ApplyProvider(Properties.Settings.Default);
            Properties.Settings.Default.Reset();
        }
    }

    [TestClass]
    public class PortableJsonSettingsProviderTests : SettingsProviderTestsBase
    {
        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            settingsFile = "settings.json";
            PortableJsonSettingsProvider.ApplyProvider(Properties.Settings.Default);
            Properties.Settings.Default.Reset();
        }
    }
}
