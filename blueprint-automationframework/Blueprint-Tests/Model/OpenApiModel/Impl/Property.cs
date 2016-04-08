using Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Model.Factories;
using Utilities;

namespace Model.OpenApiModel.Impl
{
    /// <summary>
    /// An Enumeration of possible UsersAndGroups Types
    /// </summary>
    public enum UsersAndGroupsType
    {
        User,
        Group
    }

    public class OpenApiProperty : IOpenApiProperty
    {
        public int PropertyTypeId { get; set; }
        public string Name { get; set; }
        public string BasePropertyType { get; set; }
        public string TextOrChoiceValue { get; set; }
        public bool IsRichText { get; set; }
        public bool IsReadOnly { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public List<UsersAndGroups> UsersAndGroups { get; set; }
        public List<object> Choices { get; }
        public string DateValue { get; set; }

        /// TODO: need to be updated for future script update
        /// <summary>
        /// Create a property object based on the information from DB</summary>
        /// <param name="project">project Id</param>
        /// <param name="propertyName">property Name</param>
        /// <param name="propertyValue">(optional) property Name</param>
        /// <exception cref="System.Data.SqlClient.SqlException">The exception that is thrown when SQL Server returns a warning or error.</exception>
        /// <exception cref="System.InvalidOperationException">If no data is present with the requested sql</exception>
        public OpenApiProperty GetProperty(IProject project, string propertyName, string propertyValue = null)
        {
            ThrowIf.ArgumentNull(project, nameof(project));

            OpenApiProperty property = null;

            using (IDatabase database = DatabaseFactory.CreateDatabase())
            {
                string query = "SELECT PropertyTypeId, RichText, Name FROM dbo.TipPropertyTypesView WHERE Project_ItemId = @Project_ItemId and Name = @Name;";
                Logger.WriteDebug("Running: {0}", query);
                using (SqlCommand cmd = database.CreateSqlCommand(query))
                {
                    database.Open();
                    cmd.Parameters.Add("@Project_ItemId", SqlDbType.Int).Value = project.Id;
                    cmd.Parameters.Add("@Name", SqlDbType.NChar).Value = propertyName;
                    cmd.CommandType = CommandType.Text;

                    try
                    {
                        SqlDataReader reader;
                        using (reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                reader.Read();
                            }
                            string querybasePropertyType = "Text";
                            string querytextOrChoiceValue = propertyValue ?? "DefaultValue";
                            int querypropertyTypeId = int.Parse(reader["PropertyTypeId"].ToString(), CultureInfo.InvariantCulture);
                            string queryname = reader["Name"].ToString();

                            property = new OpenApiProperty
                            {
                                PropertyTypeId = querypropertyTypeId,
                                Name = queryname,
                                BasePropertyType = querybasePropertyType,
                                TextOrChoiceValue = querytextOrChoiceValue,
                                IsRichText = true,
                                IsReadOnly = false
                            };
                        }
                    }
                    catch (InvalidOperationException ex)
                    {
                        Logger.WriteError("No Property is available which matches with condition. Exception details - {0}", ex);
                    }
                }
            }
            return property;
        }
        public OpenApiProperty SetPropertyValue(IProject project, BaseArtifactType artifactType,
    string propertyValue = null)
        {
            OpenApiProperty property = new OpenApiProperty
            {
                PropertyTypeId = 1,
                Name = "",
                BasePropertyType = "",
                TextOrChoiceValue = "",
                IsReadOnly = false,
                IsRichText = false
            };

            return property;
        }
    }

    public class UsersAndGroups
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
        public UsersAndGroupsType Type { get; set; }

        public int Id { get; set; }

        public string DisplayName { get; set; }
    }
}
