using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using ArtifactStore.Models;
using ArtifactStore.Models.Workflow.Actions;
using Dapper;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Models.PropertyType;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ProjectMeta.PropertyXml;
using ServiceLibrary.Repositories.ProjectMeta.PropertyXml.Models;

namespace ArtifactStore.Repositories
{
    public interface ISaveArtifactRepository
    {
        Task SavePropertyChangeActions(
            IEnumerable<PropertyChangeAction> actions,
            IEnumerable<DPropertyType> propertyTypes,
            VersionControlArtifactInfo artifact,
            IDbTransaction transaction = null);
    }

    public class SqlSaveArtifactRepository: ISaveArtifactRepository
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
            IEnumerable<PropertyChangeAction> actions,
            IEnumerable<DPropertyType> propertyTypes,
            VersionControlArtifactInfo artifact,
            IDbTransaction transaction = null)
        {
            var propertyValueVersionsTable = PopulateSavePropertyValueVersionsTable(actions, propertyTypes, artifact);

            var propertyValueImagesTable = PopulateImagePropertyValueVersionsTable();

            var param = new DynamicParameters();
            param.Add("@propertyValueVersions", propertyValueVersionsTable);
            param.Add("@propertyValueImages", propertyValueImagesTable);
            param.Add("@userId", 1);

            const string storedProcedure = "SavePropertyValueVersions";
            if (transaction == null)
            {
                await
                    _connectionWrapper.ExecuteAsync(storedProcedure, param, commandType: CommandType.StoredProcedure);
            }
            else
            {
                await
                    transaction.Connection.ExecuteAsync(storedProcedure, param, transaction,
                        commandType: CommandType.StoredProcedure);
            }
        }

        private DataTable PopulateSavePropertyValueVersionsTable(
            IEnumerable<PropertyChangeAction> actions,
            IEnumerable<DPropertyType> propertyTypes,
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
                var customPropertyChar = GetCustomPropertyChar(propertyType);
                if (propertyType is DNumberPropertyType)
                {
                    propertyValueVersionsTable.Rows.Add(propertyType.PropertyTypeId, false,
                        artifact.ProjectId, artifact.Id, artifact.Id, (int)propertyType.Predefined,
                        //
                        (int)PropertyPrimitiveType.Number,
                        PropertyHelper.GetBytes(action.PropertyLiteValue.NumberValue.GetValueOrDefault(0)),
                        null, null, null, null, null,
                        //
                        customPropertyChar, propertyType.PropertyTypeId, "");
                    // propertyValues.Add(propertyValue);
                }
            }
            propertyValueVersionsTable.SetTypeName("SavePropertyValueVersionsCollection");
            return propertyValueVersionsTable;
        }

        private DataTable PopulateImagePropertyValueVersionsTable()
        {
            var propertyValueImagesTable = new DataTable();
            propertyValueImagesTable.Locale = CultureInfo.InvariantCulture;
            propertyValueImagesTable.Columns.Add("NodeId", typeof(long));
            propertyValueImagesTable.Columns.Add("Content", typeof(byte[]));
            propertyValueImagesTable.SetTypeName("SavePropertyValueImagesCollection");
            return propertyValueImagesTable;
        }

        private static string GetCustomPropertyChar(/*DPropertyValue propertyValue,*/ DPropertyType propertyType)
        {
            //BluePrintSys.RC.CrossCutting.Logging.Log.Assert(
            //    (propertyValue != null) && propertyValue.SaveState.HasFlag(NodeSaveState.MemoryNode));
            if (/*propertyValue.NodeDeleted ||*/
                (((int)PropertyTypePredefined.GroupMask & (int)propertyType.Predefined) != (int)PropertyTypePredefined.CustomGroup))
            {
                return null;
            }
            PropertyPrimitiveType primitiveType;
            if (propertyType is DNumberPropertyType)
            {
                primitiveType = PropertyPrimitiveType.Number;
            }
            //else if (propertyValue is DTextPropertyValue)
            //{
            //    primitiveType = PropertyPrimitiveType.Text;
            //}
            //else if (propertyValue is DDatePropertyValue)
            //{
            //    primitiveType = PropertyPrimitiveType.Date;
            //}
            //else if (propertyValue is DUserPropertyValue)
            //{
            //    primitiveType = PropertyPrimitiveType.User;
            //}
            //else if (propertyValue is DChoicePropertyValue)
            //{
            //    primitiveType = PropertyPrimitiveType.Choice;
            //}
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
            XmlCustomProperty customProperty = XmlCustomProperty.CreateAsValue(propertyType.PropertyTypeId, (int)primitiveType);
            customProperties.CustomProperties.Add(customProperty);
            //if (propertyValue is DChoicePropertyValue)
            //{
            //    List<XmlCustomPropertyValidValue> validValues = customProperty.ValidValues;
            //    foreach (DLookupListItem lookupListItem in propertyValue.NodeChildren.OfType<DLookupListItem>())
            //    {
            //        BluePrintSys.RC.CrossCutting.Logging.Log.Assert(lookupListItem.SaveState.HasFlag(NodeSaveState.Node));
            //        XmlCustomPropertyValidValue validValue = XmlCustomPropertyValidValue.CreateAsValue(lookupListItem.Id);
            //        validValues.Add(validValue);
            //    }
            //}
            return XmlModelSerializer.SerializeCustomProperties(customProperties);
        }

        #endregion
    }
}