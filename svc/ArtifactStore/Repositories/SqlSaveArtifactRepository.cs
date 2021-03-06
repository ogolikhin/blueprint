﻿using System;
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

            var parameters = new DynamicParameters();
            parameters.Add("@propertyValueVersions", propertyValueVersionsTable);
            parameters.Add("@propertyValueImages", propertyValueImagesTable);
            parameters.Add("@userId", userId);

            const string storedProcedure = "SavePropertyValueVersions";

            if (transaction == null)
            {
                await _connectionWrapper.QueryAsync<dynamic>
                (
                    storedProcedure,
                    parameters,
                    commandType: CommandType.StoredProcedure);

            }
            else
            {
                await transaction.Connection.QueryAsync<dynamic>
                (
                    storedProcedure,
                    parameters,
                    transaction,
                    commandType: CommandType.StoredProcedure);
            }
        }

        public async Task UpdateArtifactName(
            int userId,
            int artifactId,
            string artifactName,
            IDbTransaction transaction = null)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@userId", userId);
            parameters.Add("@artifactId", artifactId);
            parameters.Add("@artifactName", artifactName);

            object result;
            const string storedProcedure = "UpdateArtifactName";

            if (transaction == null)
            {
                result = await _connectionWrapper.QueryAsync<dynamic>
                (
                    storedProcedure,
                    parameters,
                    commandType: CommandType.StoredProcedure);
            }
            else
            {
                result = await transaction.Connection.QueryAsync<dynamic>
                (
                    storedProcedure,
                    parameters,
                    transaction,
                    commandType: CommandType.StoredProcedure);
            }

            int? errorCode = (result as IEnumerable<dynamic>)?.FirstOrDefault()?.Error;
            if (errorCode != null && errorCode.GetValueOrDefault(0) > 0)
            {
                throw new Exception("UpdateArtifactName failed with Error Code " + errorCode.Value);
            }
        }

        private DataTable PopulateSavePropertyValueVersionsTable(
            IEnumerable<IPropertyChangeAction> actions,
            IEnumerable<WorkflowPropertyType> propertyTypes,
            VersionControlArtifactInfo artifact)
        {
            DataTable propertyValueVersionsTable = new DataTable();
            propertyValueVersionsTable.Locale = CultureInfo.InvariantCulture;

            propertyValueVersionsTable.Columns.Add("NodeId", typeof(long));
            propertyValueVersionsTable.Columns.Add("NodeDeleted", typeof(bool));
            //
            propertyValueVersionsTable.Columns.Add("VersionProjectId", typeof(int));
            propertyValueVersionsTable.Columns.Add("VersionArtifactId", typeof(int));
            propertyValueVersionsTable.Columns.Add("VersionItemId", typeof(int));
            propertyValueVersionsTable.Columns.Add("PropertyTypePredefined", typeof(int));
            propertyValueVersionsTable.Columns.Add("PrimitiveType", typeof(int));
            propertyValueVersionsTable.Columns.Add("DecimalValue", typeof(byte[]));
            propertyValueVersionsTable.Columns.Add("DateValue", typeof(DateTime));
            propertyValueVersionsTable.Columns.Add("UserValue", typeof(string));
            propertyValueVersionsTable.Columns.Add("UserLabel", typeof(string));
            propertyValueVersionsTable.Columns.Add("StringValue", typeof(string));
            propertyValueVersionsTable.Columns.Add("ImageValue_ImageId", typeof(int));
            propertyValueVersionsTable.Columns.Add("CustomPropertyChar", typeof(string));
            propertyValueVersionsTable.Columns.Add("PropertyType_PropertyTypeId", typeof(int));
            //
            propertyValueVersionsTable.Columns.Add("SearchableValue", typeof(string));
            foreach (var action in actions)
            {
                var propertyType =
                    propertyTypes.FirstOrDefault(a => a.InstancePropertyTypeId == action.InstancePropertyTypeId);

                var customPropertyChar = GetCustomPropertyChar(action.PropertyLiteValue, propertyType);
                var searchableValue = GetSearchableValue(action.PropertyLiteValue, propertyType);

                if (propertyType is TextPropertyType)
                {
                    propertyValueVersionsTable.Rows.Add(propertyType.PropertyTypeId, false,
                        artifact.ProjectId, artifact.Id, artifact.Id, (int)propertyType.Predefined,
                        //
                        (int)PropertyPrimitiveType.Text,
                        null, null, null, null,
                        GetTextPropertyValue(propertyType, action.PropertyLiteValue),
                        null,
                        //
                        customPropertyChar, propertyType.PropertyTypeId, searchableValue);
                }
                else if (propertyType is NumberPropertyType)
                {
                    propertyValueVersionsTable.Rows.Add(propertyType.PropertyTypeId, false,
                        artifact.ProjectId, artifact.Id, artifact.Id, (int)propertyType.Predefined,
                        //
                        (int)PropertyPrimitiveType.Number,
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
                        artifact.ProjectId, artifact.Id, artifact.Id, (int)propertyType.Predefined,
                        //
                        (int)PropertyPrimitiveType.User,
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

        private string GetTextPropertyValue(WorkflowPropertyType propertyType, PropertyLite property)
        {
            if (property.TextOrChoiceValue == null)
            {
                return null;
            }

            if (propertyType.Predefined == PropertyTypePredefined.Name)
            {
                return property.TextOrChoiceValue;
            }
            else
            {
                return "<html><head/><p>" + property.TextOrChoiceValue + "</p></html>";
            }
        }

        private DataTable PopulateImagePropertyValueVersionsTable()
        {
            var propertyValueImagesTable = new DataTable { Locale = CultureInfo.InvariantCulture };
            propertyValueImagesTable.Columns.Add("NodeId", typeof(long));
            propertyValueImagesTable.Columns.Add("Content", typeof(byte[]));
            propertyValueImagesTable.SetTypeName("SavePropertyValueImagesCollection");
            return propertyValueImagesTable;
        }

        private static string GetCustomPropertyChar(PropertyLite propertyValue, WorkflowPropertyType propertyType)
        {
            // BluePrintSys.RC.CrossCutting.Logging.Log.Assert(
            //    (propertyValue != null) && propertyValue.SaveState.HasFlag(NodeSaveState.MemoryNode));
            if ( /*propertyValue.NodeDeleted ||*/
                (((int)PropertyTypePredefined.GroupMask & (int)propertyType.Predefined) !=
                 (int)PropertyTypePredefined.CustomGroup))
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
            else if (propertyType is TextPropertyType)
            {
                primitiveType = PropertyPrimitiveType.Text;
            }
            else if (propertyType is UserPropertyType)
            {
                primitiveType = PropertyPrimitiveType.User;
            }
            else if (propertyType is ChoicePropertyType)
            {
                primitiveType = PropertyPrimitiveType.Choice;
            }
            // else if (propertyValue is DImagePropertyValue)
            // {
            //    primitiveType = PropertyPrimitiveType.Image;
            // }
            else
            {
                // BluePrintSys.RC.CrossCutting.Logging.Log.Assert(false);
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
            if (propertyType is TextPropertyType)
            {
                return propertyLite.TextOrChoiceValue;
            }
            return null;
        }

        #endregion
    }
}