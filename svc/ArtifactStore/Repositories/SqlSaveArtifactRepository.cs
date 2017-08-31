using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Models.PropertyType;
using ServiceLibrary.Models.VersionControl;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ProjectMeta.PropertyXml;
using ServiceLibrary.Repositories.ProjectMeta.PropertyXml.Models;
using ServiceLibrary.Models.Workflow;

namespace ArtifactStore.Repositories
{
    public class SqlSaveArtifactRepository : ISaveArtifactRepository
    {
        private readonly ISqlConnectionWrapper _connectionWrapper;

        public SqlSaveArtifactRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        internal SqlSaveArtifactRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        #region SavePropertyChangeActions

        public async Task SavePropertyChangeActions(
            int userId,
            IEnumerable<IPropertyChangeAction> actions,
            IEnumerable<WorkflowPropertyType> propertyTypes,
            VersionControlArtifactInfo artifact,
            IDbTransaction transaction = null)
        {
            var propertyValueVersionsTable = PopulateSavePropertyValueVersionsTable(actions, propertyTypes, artifact);

            var propertyValueImagesTable = PopulateImagePropertyValueVersionsTable();

            var param = new DynamicParameters();
            param.Add("@propertyValueVersions", propertyValueVersionsTable);
            param.Add("@propertyValueImages", propertyValueImagesTable);
            param.Add("@userId", userId);

            const string storedProcedure = "SavePropertyValueVersions";
            if (transaction == null)
            {
                await
                    _connectionWrapper.QueryAsync<dynamic>(storedProcedure, param, commandType: CommandType.StoredProcedure);
                
            }
            else
            {
                await
                    transaction.Connection.QueryAsync<dynamic>(storedProcedure, param, transaction,
                        commandType: CommandType.StoredProcedure);
            }
        }

        private DataTable PopulateSavePropertyValueVersionsTable(
            IEnumerable<IPropertyChangeAction> actions,
            IEnumerable<WorkflowPropertyType> propertyTypes,
            VersionControlArtifactInfo artifact)
        {
            DataTable propertyValueVersionsTable = new DataTable();
            propertyValueVersionsTable.Locale = CultureInfo.InvariantCulture;

            propertyValueVersionsTable.Columns.Add("NodeId", typeof (long));
            propertyValueVersionsTable.Columns.Add("NodeDeleted", typeof (bool));
            //
            propertyValueVersionsTable.Columns.Add("VersionProjectId", typeof (int));
            propertyValueVersionsTable.Columns.Add("VersionArtifactId", typeof (int));
            propertyValueVersionsTable.Columns.Add("VersionItemId", typeof (int));
            propertyValueVersionsTable.Columns.Add("PropertyTypePredefined", typeof (int));
            propertyValueVersionsTable.Columns.Add("PrimitiveType", typeof (int));
            propertyValueVersionsTable.Columns.Add("DecimalValue", typeof (byte[]));
            propertyValueVersionsTable.Columns.Add("DateValue", typeof (DateTime));
            propertyValueVersionsTable.Columns.Add("UserValue", typeof (string));
            propertyValueVersionsTable.Columns.Add("UserLabel", typeof (string));
            propertyValueVersionsTable.Columns.Add("StringValue", typeof (string));
            propertyValueVersionsTable.Columns.Add("ImageValue_ImageId", typeof (int));
            propertyValueVersionsTable.Columns.Add("CustomPropertyChar", typeof (string));
            propertyValueVersionsTable.Columns.Add("PropertyType_PropertyTypeId", typeof (int));
            //
            propertyValueVersionsTable.Columns.Add("SearchableValue", typeof (string));
            foreach (var action in actions)
            {
                var propertyType =
                    propertyTypes.FirstOrDefault(a => a.InstancePropertyTypeId == action.InstancePropertyTypeId);

                var customPropertyChar = GetCustomPropertyChar(action.PropertyLiteValue, propertyType);
                var searchableValue = GetSearchableValue(action.PropertyLiteValue, propertyType);

                if (propertyType is NumberPropertyType)
                {
                    propertyValueVersionsTable.Rows.Add(propertyType.PropertyTypeId, false,
                        artifact.ProjectId, artifact.Id, artifact.Id, (int) propertyType.Predefined,
                        //
                        (int) PropertyPrimitiveType.Number,
                        PropertyHelper.GetBytes(action.PropertyLiteValue.NumberValue),
                        null, null, null, null, null,
                        //
                        customPropertyChar, propertyType.PropertyTypeId, searchableValue);
                }
                else if (propertyType is DatePropertyType)
                {
                    propertyValueVersionsTable.Rows.Add(propertyType.PropertyTypeId, false,
                        artifact.ProjectId, artifact.Id, artifact.Id, (int)propertyType.Predefined,
                        //
                        (int)PropertyPrimitiveType.Date,
                        null,
                        action.PropertyLiteValue.DateValue,
                        null, null, null, null,
                        //
                        customPropertyChar, propertyType.PropertyTypeId, searchableValue);
                }
                else if (propertyType is UserPropertyType)
                {
                    propertyValueVersionsTable.Rows.Add(propertyType.PropertyTypeId, false,
                        artifact.ProjectId, artifact.Id, artifact.Id, (int) propertyType.Predefined,
                        //
                        (int) PropertyPrimitiveType.User,
                        null, null,
                        PropertyHelper.ParseUserGroupsToString(action.PropertyLiteValue.UsersAndGroups), null, null,
                        null,
                        //
                        customPropertyChar, propertyType.PropertyTypeId, searchableValue);
                }
                else if (propertyType is ChoicePropertyType)
                {
                    propertyValueVersionsTable.Rows.Add(propertyType.PropertyTypeId, false,
                        artifact.ProjectId, artifact.Id, artifact.Id, (int)propertyType.Predefined,
                        //
                        (int)PropertyPrimitiveType.Choice,
                        null, null, null, null, action.PropertyLiteValue.TextOrChoiceValue, null,
                        // 
                        customPropertyChar, propertyType.PropertyTypeId, searchableValue);
                }
            }
            propertyValueVersionsTable.SetTypeName("SavePropertyValueVersionsCollection");
            return propertyValueVersionsTable;
        }

        private DataTable PopulateImagePropertyValueVersionsTable()
        {
            var propertyValueImagesTable = new DataTable {Locale = CultureInfo.InvariantCulture};
            propertyValueImagesTable.Columns.Add("NodeId", typeof (long));
            propertyValueImagesTable.Columns.Add("Content", typeof (byte[]));
            propertyValueImagesTable.SetTypeName("SavePropertyValueImagesCollection");
            return propertyValueImagesTable;
        }

        private static string GetCustomPropertyChar(PropertyLite propertyValue, WorkflowPropertyType propertyType)
        {
            //BluePrintSys.RC.CrossCutting.Logging.Log.Assert(
            //    (propertyValue != null) && propertyValue.SaveState.HasFlag(NodeSaveState.MemoryNode));
            if ( /*propertyValue.NodeDeleted ||*/
                (((int) PropertyTypePredefined.GroupMask & (int) propertyType.Predefined) !=
                 (int) PropertyTypePredefined.CustomGroup))
            {
                return null;
            }
            PropertyPrimitiveType primitiveType;
            if (propertyType is NumberPropertyType)
            {
                primitiveType = PropertyPrimitiveType.Number;
            }
            else if (propertyType is DatePropertyType)
            {
                primitiveType = PropertyPrimitiveType.Date;
            }
            //else if (propertyValue is DTextPropertyValue)
            //{
            //    primitiveType = PropertyPrimitiveType.Text;
            //}
            else if (propertyType is UserPropertyType)
            {
                primitiveType = PropertyPrimitiveType.User;
            }
            else if (propertyType is ChoicePropertyType)
            {
                primitiveType = PropertyPrimitiveType.Choice;
            }
            //else if (propertyValue is DImagePropertyValue)
            //{
            //    primitiveType = PropertyPrimitiveType.Image;
            //}
            else
            {
                //BluePrintSys.RC.CrossCutting.Logging.Log.Assert(false);
                return null;
            }
            XmlCustomProperties customProperties = new XmlCustomProperties();
            XmlCustomProperty customProperty = XmlCustomProperty.CreateAsValue(propertyType.PropertyTypeId,
                (int)primitiveType);
            customProperties.CustomProperties.Add(customProperty);
            if (propertyType is ChoicePropertyType)
            {
                List<XmlCustomPropertyValidValue> validValues = customProperty.ValidValues;
                foreach (int choiceId in propertyValue.ChoiceIds)
                {
                    var customChoice = (propertyType as ChoicePropertyType).ValidValues
                                       .Where(v => v.Sid.Value == choiceId)
                                       .Select(v => v.Id).FirstOrDefault().Value;
                    XmlCustomPropertyValidValue validValue = XmlCustomPropertyValidValue.CreateAsValue(customChoice);
                    validValues.Add(validValue);
                }
            }
            return XmlModelSerializer.SerializeCustomProperties(customProperties);
        }

        private static string GetSearchableValue(PropertyLite propertyLite, WorkflowPropertyType propertyType)
        {
            //if (propertyValue is DTextPropertyValue)
            //{
            //    string value = ((DTextPropertyValue)propertyValue).Value;
            //    if (string.IsNullOrWhiteSpace(value))
            //    {
            //        return null;
            //    }
            //    switch (propertyValue.PropertyTypePredefined)
            //    {
            //        case PropertyTypePredefined.Name:
            //            return value;
            //        case PropertyTypePredefined.Description:
            //        case PropertyTypePredefined.Label:
            //        case PropertyTypePredefined.CustomGroup:
            //            return RichTextHtmlHelper.Instance.HtmlToText(value);
            //        default:
            //            return null;
            //    }
            //}
            if (propertyType is NumberPropertyType)
            {
                return null;
            }
            if (propertyType is DatePropertyType)
            {
                return null;
            }
            //if (propertyValue is DDatePropertyValue)
            //{
            //    return null;
            //}
            //if (propertyValue is DUserPropertyValue)
            //{
            //    return null;
            //}
            if (propertyType is ChoicePropertyType)
            {
                return null;
            }
            //if (propertyValue is DImagePropertyValue)
            //{
            //    return null;
            //}
            //BluePrintSys.RC.CrossCutting.Logging.Log.Assert(false);
            return null;
        }

        #endregion
    }
}