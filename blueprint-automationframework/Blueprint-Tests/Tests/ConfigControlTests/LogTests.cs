using CustomAttributes;
using Model;
using Model.Factories;
using NUnit.Framework;

namespace ConfigControlTests
{
    [TestFixture]
    [Category(Categories.ConfigControl)]
    public static class LogTests
    {
        /// <summary>
        /// Runs a common set of asserts on the given file.
        /// </summary>
        /// <param name="file">The file to check.</param>
        private static void AssertLogFile(IFile file)
        {
            Assert.NotNull(file, "ConfigControl returned a null file!");

            const string expectedFilename = "AdminStore.csv";
            const string expectedFileType = "text/csv";

            Assert.That(file.FileName == expectedFilename,
                "ConfigControl.GetLog returned a file named '{0}', but it should be '{1}'!", file.FileName, expectedFilename);

            Assert.That(file.FileType == expectedFileType,
                "ConfigControl.GetLog returned File Type '{0}', but it should be '{1}'!", file.FileType, expectedFileType);

            Assert.That(file.Content.ToString().Length > 0, "ConfigControl.GetLog returned an empty file!");
        }

        [TestCase]
        [Description("Calls the GetLog method of ConfigControl with no authentication.  Verify log file is returned.")]
        public static void GetLog_NoToken_VerifyLogFile()
        {
            IConfigControl configControl = ConfigControlFactory.GetConfigControlFromTestConfig();
            IFile file = configControl.GetLog();

            AssertLogFile(file);
        }
    }
}
