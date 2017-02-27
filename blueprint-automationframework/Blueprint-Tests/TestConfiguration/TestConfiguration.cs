using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Common;

namespace TestConfig
{
    public class Database
    {
        // Example:
        // Data Source=cjust-test02;Initial Catalog=Blueprint;Integrated Security=False;MultipleActiveResultSets=True;User ID=sa;Password=Welcome1;
        public string ConnectionString { get; set; }
        public string Name { get; set; }
    }

    public class Service
    {
        public string Address { get; set; }
        public string Name { get; set; }
    }

    public class TestConfiguration
    {
        private static TestConfiguration _instance = null;

        private Dictionary<string, Database> _databases = new Dictionary<string, Database>();
        private Dictionary<string, Service> _services = new Dictionary<string, Service>();

        #region Properties

        public string BlueprintServerAddress { get; set; }

        public Dictionary<string, Database> Databases { get { return _databases; } }
        public Dictionary<string, Service> Services { get { return _services; } }

        public string Username { get; set; }

        public string Password { get; set; }

        public Logger.LogLevels LogLevel { get; set; }

        #endregion Properties


        #region Constructors
        
        /// <summary>
        /// Constructs an empty TestConfiguration.
        /// </summary>
        private TestConfiguration() { }

        #endregion Constructors

        #region Private functions

        /// <summary>
        /// Reads the BlueprintServer tag in the XML and adds the address & credentials to the specified TestConfiguration.
        /// </summary>
        /// <param name="testConfig">This will be updated with the Blueprint Server info found in the XML.</param>
        /// <param name="root">The root XML node.</param>
        private static void ReadBlueprintServerNode(TestConfiguration testConfig, XmlNode root)
        {
            XmlNode blueprintServerNode = root.SelectSingleNode("BlueprintServer");
            testConfig.BlueprintServerAddress = blueprintServerNode.Attributes["address"].Value;
            testConfig.Username = blueprintServerNode.Attributes["username"].Value;
            testConfig.Password = blueprintServerNode.Attributes["password"].Value;
        }

        /// <summary>
        /// Reads the Service tags in the BlueprintServices XML node and adds them to the specified TestConfiguration.
        /// </summary>
        /// <param name="testConfig">This will be updated with the Blueprint Services found in the XML.</param>
        /// <param name="root">The root XML node.</param>
        private static void ReadBlueprintServicesNodes(TestConfiguration testConfig, XmlNode root)
        {
            XmlNode blueprintServicesNode = root.SelectSingleNode("BlueprintServices");

            foreach (XmlNode node in blueprintServicesNode.ChildNodes)
            {
                Service service = new Service();
                service.Name = node.Attributes["name"].Value;
                service.Address = node.Attributes["address"].Value;
                testConfig.Services.Add(service.Name, service);
            }
        }

        /// <summary>
        /// Reads the Database tags in the Databases XML node and adds them to the specified TestConfiguration.
        /// </summary>
        /// <param name="testConfig">This will be updated with the Databases found in the XML.</param>
        /// <param name="root">The root XML node.</param>
        private static void ReadDatabasesNodes(TestConfiguration testConfig, XmlNode root)
        {
            XmlNode databasesNode = root.SelectSingleNode("Databases");

            foreach (XmlNode node in databasesNode.ChildNodes)
            {
                Database database = new Database();
                database.Name = node.Attributes["name"].Value;
                database.ConnectionString = node.Attributes["connection_string"].Value;
                testConfig.Databases.Add(database.Name, database);
            }
        }

        #endregion Private functions

        #region Public functions

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        /// <param name="path">(optional) The path to the TestConfiguration.xml file.</param>
        /// <returns>The TestConfiguration object.</returns>
        public static TestConfiguration GetInstance(string path = null)
        {
            if (path == null)
            {
                var testConfig = Environment.GetEnvironmentVariable("TEST_CONFIG_FILE", EnvironmentVariableTarget.Process);

                path = testConfig ?? Path.Combine(Directory.GetCurrentDirectory(), "TestConfiguration.xml");
            }

            return _instance ?? (_instance = ReadTestConfigFile(path));
        }

        /// <summary>
        /// Reads the test configuration from the specified file path and returns a TestConfiguration object.
        /// </summary>
        /// <param name="path">The path to the TestConfiguration.xml file.</param>
        /// <returns>The TestConfiguration that was read.</returns>
        public static TestConfiguration ReadTestConfigFile(string path)
        {
            Logger.LogLevel = Logger.LogLevels.INFO;
            Logger.WriteInfo("Reading test configuration from: '{0}'.", path);

            XmlReaderSettings readerSettings = new XmlReaderSettings();
            readerSettings.IgnoreComments = true;
            TestConfiguration testConfig = new TestConfiguration();

            using (XmlReader xmlReader = XmlReader.Create(path, readerSettings))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(xmlReader);

                XmlNode root = doc.SelectSingleNode("/TestConfiguration");

                testConfig.LogLevel = (Logger.LogLevels)Enum.Parse(typeof(Logger.LogLevels), root.SelectSingleNode("LogLevel").InnerText);
                Logger.LogLevel = testConfig.LogLevel;

                ReadBlueprintServerNode(testConfig, root);
                ReadDatabasesNodes(testConfig, root);
                ReadBlueprintServicesNodes(testConfig, root);
            }

            return testConfig;
        }

        /// <summary>
        /// Returns a string representation of this object.
        /// </summary>
        /// <returns>A string representation of this object.</returns>
        public override string ToString()
        {
            string thisObject = I18NHelper.FormatInvariant("[TestConfiguration]: BlueprintServerAddress='{0}', Username='{1}', Password='{2}', LogLevel='{3}'", BlueprintServerAddress, Username, Password, LogLevel.ToString());

            if (Databases.Count > 0)
            {
                thisObject += "\n    [Databases]:";
                foreach (var database in Databases)
                {
                    thisObject += I18NHelper.FormatInvariant("\n      -> Name: '{0}', ConnectionString: '{1}'", database.Key, database.Value.ConnectionString);
                }
            }

            Console.WriteLine(thisObject);
            return thisObject;
        }

        #endregion Public functions
    }
}
