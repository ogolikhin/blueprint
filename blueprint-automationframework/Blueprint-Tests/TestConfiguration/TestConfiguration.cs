using Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace TestConfig
{
    public class Database
    {
        // Example:
        // Data Source=cjust-test02;Initial Catalog=Blueprint;Integrated Security=False;MultipleActiveResultSets=True;User ID=sa;Password=Welcome1;
        public string ConnectionString { get; set; }

        public string Name { get; set; }
    }

    public class TestConfiguration
    {
        private static TestConfiguration _instance = null;

        private Dictionary<string, Database> _databases = new Dictionary<string, Database>();

        #region Properties

        public string BlueprintServerAddress { get; set; }

        public Dictionary<string, Database> Databases { get { return _databases; } }

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

        #region Public functions

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        /// <param name="path">(optional) The path to the TestConfiguration.xml file.</param>
        /// <returns>The TestConfiguration object.</returns>
        public static TestConfiguration GetInstance(string path = null)
        {
            if (path == null) { path = Path.Combine(Directory.GetCurrentDirectory(), "TestConfiguration.xml"); }

            if (_instance == null) { _instance = ReadTestConfigFile(path); }

            return _instance;
        }

        /// <summary>
        /// Reads the test configuration from the specified file path and returns a TestConfiguration object.
        /// </summary>
        /// <param name="path">The path to the TestConfiguration.xml file.</param>
        /// <returns>The TestConfiguration that was read.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]    // Ignore this warning.
        public static TestConfiguration ReadTestConfigFile(string path)
        {
            Logger.LogLevel = Logger.LogLevels.INFO;
            Logger.WriteInfo("Reading test configuration from: '{0}'.", path);

            XmlDocument doc = new XmlDocument();
            doc.Load(path);

            TestConfiguration testConfig = new TestConfiguration();
            XmlNode root = doc.SelectSingleNode("/TestConfiguration");
            testConfig.BlueprintServerAddress = root.SelectSingleNode("BlueprintServerAddress").InnerText;
            testConfig.Username = root.SelectSingleNode("Username").InnerText;
            testConfig.Password = root.SelectSingleNode("Password").InnerText;
            testConfig.LogLevel = (Logger.LogLevels)Enum.Parse(typeof(Logger.LogLevels), root.SelectSingleNode("LogLevel").InnerText);
            Logger.LogLevel = testConfig.LogLevel;
            XmlNode databasesNode = root.SelectSingleNode("Databases");

            foreach (XmlNode databaseNode in databasesNode.ChildNodes)
            {
                Database database = new Database();
                database.Name = databaseNode.Attributes["Name"].Value;
                database.ConnectionString = databaseNode.Attributes["ConnectionString"].Value;
                testConfig.Databases.Add(database.Name, database);
            }

            return testConfig;
        }

        /// <summary>
        /// Returns a string representation of this object.
        /// </summary>
        /// <returns>A string representation of this object.</returns>
        public override string ToString()
        {
            string thisObject = string.Format("[TestConfiguration]: BlueprintServerAddress='{0}', Username='{1}', Password='{2}', LogLevel='{3}'", BlueprintServerAddress, Username, Password, LogLevel.ToString());

            if (Databases.Count > 0)
            {
                thisObject += "\n    [Databases]:";
                foreach (var database in Databases)
                {
                    thisObject += string.Format("\n      -> Name: '{0}', ConnectionString: '{1}'", database.Key, database.Value.ConnectionString);
                }
            }

            Console.WriteLine(thisObject);
            return thisObject;
        }

        #endregion Public functions
    }
}
