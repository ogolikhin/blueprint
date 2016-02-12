using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Model.Factories;
using Utilities;

namespace Model.Impl
{
    public class Property : IProperty
    {

    }

    public class OpenApiProperty : Property, IOpenApiProperty
    {
        public int PropertyTypeId { get; set; }
        public string Name { get; set; }
        public string BasePropertyType { get; set; }
        public string TextOrChoiceValue { get; set; }
        public bool IsRichText { get; set; }
        public bool IsReadOnly { get; set; }
        public List<object> UsersAndGroups { get; }
        public List<object> Choices { get; }
        public string DateValue { get; set; }

        /// TODO: need to be updated for future script update
        /// <summary>
        /// Create a property object based on the information from DB
        /// <param name="projectId">project Id</param>
        /// <param name="propertyName">property Name</param>
        /// <param name="propertyValue">(optional) property Name</param>
        public IOpenApiProperty CreatePropertyBasedonDB(IProject project, string propertyName, string propertyValue = null)
        {
            ThrowIf.ArgumentNull(project, nameof(project));
            string query = null;
            SqlDataReader reader;

            //variables
            int query_propertyTypeId;
            string query_name;
            string query_basePropertyType;
            string query_textOrChoiceValue;
            bool query_isRichText;
            bool query_isReadOnly;

            using (IDatabase database = DatabaseFactory.CreateDatabase())
            {
                query = "SELECT PropertyTypeId, RichText, Name FROM dbo.TipPropertyTypesView WHERE Project_ItemId = @Project_ItemId and Name = @Name;";
                using (SqlCommand cmd = database.CreateSqlCommand(query))
                {
                    database.Open();
                    cmd.Parameters.Add("@Project_ItemId", SqlDbType.Int).Value = project.Id;
                    cmd.Parameters.Add("@Name", SqlDbType.NChar).Value = propertyName;
                    cmd.CommandType = CommandType.Text;
                    using (reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();
                        }
                        query_basePropertyType = "Text";
                        if (propertyValue == null)
                        {
                            query_textOrChoiceValue = "DefaultValue";
                        }
                        else
                        {
                            query_textOrChoiceValue = propertyValue;
                        }
                        query_isReadOnly = false;
                        query_propertyTypeId = Int32.Parse(reader["PropertyTypeId"].ToString(), CultureInfo.InvariantCulture);
                        //query_isRichText = testbool.Equals(1) ? true : false;
                        query_isRichText = true;
                        query_name = reader["Name"].ToString();
                    }
                }
            }
            IOpenApiProperty property = new OpenApiProperty();
            property.PropertyTypeId = query_propertyTypeId;
            property.Name = query_name;
            property.BasePropertyType = query_basePropertyType;
            property.TextOrChoiceValue = query_textOrChoiceValue;
            property.IsRichText = query_isRichText;
            property.IsReadOnly = query_isReadOnly;
            return property;
        }
    }
}
