using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Dapper;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Repositories.ProjectMeta.PropertyXml;
using ServiceLibrary.Repositories.ProjectMeta.PropertyXml.Models;

namespace ServiceLibrary.Repositories.ProjectMeta
{
    public class SqlProjectMetaRepository : ISqlProjectMetaRepository
    {
        private readonly static ISet<ItemTypePredefined> HiddenSubartifactTypes = new HashSet<ItemTypePredefined> {
            ItemTypePredefined.Content,
            ItemTypePredefined.BaselinedArtifactSubscribe,
            ItemTypePredefined.Extension,
            ItemTypePredefined.Flow
        };

        private readonly ISqlConnectionWrapper _connectionWrapper;

        public SqlProjectMetaRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        internal SqlProjectMetaRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public async Task<ProjectTypes> GetCustomProjectTypesAsync(int projectId, int userId)
        {
            if (projectId < 0)
                throw new ArgumentOutOfRangeException(nameof(projectId));
            if (userId < 1)
                throw new ArgumentOutOfRangeException(nameof(userId));

            if (projectId > 0)
            {
                await CheckProjectIsAccessible(projectId, userId);
            }

            var prm = new DynamicParameters();
            prm.Add("@projectId", projectId);
            prm.Add("@revisionId", ServiceConstants.VersionHead);

            var typeGrid =
                (await
                    _connectionWrapper.QueryMultipleAsync<PropertyTypeVersion, ItemTypeVersion, ItemTypePropertyTypeMapRecord>("GetProjectCustomTypes", prm,
                    commandType: CommandType.StoredProcedure));

            var ptVersions = typeGrid.Item1.ToList();
            var ptIdMap = new HashSet<int>(ptVersions.Select(p => p.PropertyTypeId));
            var itVersions = typeGrid.Item2.Where(i => !HiddenSubartifactTypes.Contains(i.Predefined.GetValueOrDefault()));
            var itPtMap = typeGrid.Item3.Where(r => ptIdMap.Contains(r.PropertyTypeId))
                .GroupBy(r => r.ItemTypeId).ToDictionary(r => r.Key, r => r.ToList().Select(mr => mr.PropertyTypeId));

            var projectTypes = new ProjectTypes();

            // Process properties
            foreach (var pv in ptVersions)
            {
                var pt = ConvertPropertyTypeVersion(pv);
                projectTypes.PropertyTypes.Add(pt);
            }

            // Process items
            foreach (var itv in itVersions)
            {
                IEnumerable<int> ptIds;
                itPtMap.TryGetValue(itv.ItemTypeId, out ptIds);
                ptIds = OrderProperties(ptIds?.ToList(), projectTypes.PropertyTypes, itv.AdvancedSettings);
                var at = ConvertItemTypeVersion(itv, ptIds, projectId);

                if (itv.Predefined != null && itv.Predefined.Value.HasFlag(ItemTypePredefined.CustomArtifactGroup))
                    projectTypes.ArtifactTypes.Add(at);
                else if (itv.Predefined != null && itv.Predefined.Value.HasFlag(ItemTypePredefined.SubArtifactGroup))
                    projectTypes.SubArtifactTypes.Add(at);
            }

            return projectTypes;
        }

        public async Task<ProjectTypes> GetStandardProjectTypesAsync()
        {
            // userId does not matter here as it's used only for projects with id greater than zero.
            return await GetCustomProjectTypesAsync(0, 1);
        }

        public async Task<IEnumerable<PropertyType>> GetStandardProjectPropertyTypesAsync(IEnumerable<int> predefinedTypeIds)
        {
            return await GetProjectPropertyTypesAsync(0, predefinedTypeIds);
        }

        public async Task<IEnumerable<PropertyType>> GetProjectPropertyTypesAsync(int projectId, IEnumerable<int> predefinedTypeIds)
        {
            var prm = new DynamicParameters();
            prm.Add("@projectId", projectId);
            prm.Add("@predefinedPropertyTypes", SqlConnectionWrapper.ToDataTable(predefinedTypeIds, "Int32Collection", "Int32Value"));

            var ptVersions = await _connectionWrapper.QueryAsync<PropertyTypeVersion>(
                "GetProjectPropertyTypes", prm, commandType: CommandType.StoredProcedure);

            var propTypes = new List<PropertyType>();
            foreach (var pv in ptVersions)
            {
                propTypes.Add(ConvertPropertyTypeVersion(pv));
            }

            return propTypes;
        }

        private async Task CheckProjectIsAccessible(int projectId, int userId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@projectId", projectId);
            parameters.Add("@userId", userId);

            var project = (await _connectionWrapper.QueryAsync<ProjectVersion>("GetInstanceProjectById", parameters, commandType: CommandType.StoredProcedure))?.FirstOrDefault();

            if (project == null)
            {
                throw new ResourceNotFoundException(string.Format("The project (Id:{0}) can no longer be accessed. It may have been deleted, or is no longer accessible by you.", projectId), ErrorCodes.ResourceNotFound);
            }

            if (!project.IsAccesible.GetValueOrDefault())
            {
                throw new AuthorizationException(string.Format("The user does not have permissions for Project (Id:{0}).", projectId), ErrorCodes.UnauthorizedAccess);
            }  
        }


        #region Ordering properties

        private static List<int> OrderProperties(List<int> itPtIds, List<PropertyType> propertyTypes, string xmlAdvancedSettings)
        {
            if (itPtIds == null || !itPtIds.Any())
                return itPtIds;

            var advancedSettings = SerializationHelper.FromXml<AdvancedSettings>(xmlAdvancedSettings);
            return OrderProperties(itPtIds, propertyTypes, advancedSettings);
        }

        internal static List<int> OrderProperties(IEnumerable<int> propertyValueIds,
            IEnumerable<PropertyType> propertyTypes, AdvancedSettings advancedSettings)
        {
            var mapPropertyTypes = propertyTypes.ToDictionary(t => t.Id);
            var mapGeneralGroup = GetLayoutGroupMap(advancedSettings, GroupType.General);
            var mapDetailsGroup = GetLayoutGroupMap(advancedSettings, GroupType.Details);

            // Tuple Key for sorting properties:
            // isMultiLineRichText - bool, multi line rich text properties go to the end
            // isNotInGneralGroup - bool, properties in General group come first
            // isNotInDetailsGroup - bool, then properties in Details group
            // orderIndex - int, int.MaxValue for properties not from layout groups
            // isStandard - bool, standard properties come after project ones
            // propertyTypeId - finally sorted by the property type Id
            var sortedDic = new SortedDictionary<Tuple<bool, bool, bool, int, bool, int>, int>();
            foreach (var pvId in propertyValueIds)
            {
                PropertyType pt;
                if (!mapPropertyTypes.TryGetValue(pvId, out pt))
                {
                    Debug.Assert(false);
                    continue;
                }

                int orderIndex;
                var isNotInGneralGroup = !mapGeneralGroup.TryGetValue(pvId, out orderIndex);

                var isNotInDetailsGroup = true;
                if (isNotInGneralGroup)
                    isNotInDetailsGroup = !mapDetailsGroup.TryGetValue(pvId, out orderIndex);

                var isMultiLineRichText = pt.IsRichText.GetValueOrDefault() && pt.IsMultipleAllowed.GetValueOrDefault();
                var key = Tuple.Create(
                    isMultiLineRichText,
                    isNotInDetailsGroup,
                    isNotInGneralGroup,
                    isNotInGneralGroup && isNotInDetailsGroup ? int.MaxValue : orderIndex,
                    pt.InstancePropertyTypeId.HasValue,
                    pvId);
                sortedDic.Add(key, pvId);
            }

            return sortedDic.OrderBy(p => p.Key).Select(r => r.Value).ToList();
        }

        // key = PropertyTypeId, value = OrderIndex
        private static IDictionary<int, int> GetLayoutGroupMap(AdvancedSettings advancedSettings, GroupType groupType)
        {
            return advancedSettings?.LayoutGroups?.FirstOrDefault(g => g.Type == groupType)?
                .Properties?.ToDictionary(pl => pl.PropertyTypeId, pl => pl.OrderIndex)
                ?? new Dictionary<int, int>();
        }

        #endregion Ordering properties

        private PropertyType ConvertPropertyTypeVersion(PropertyTypeVersion pv)
        {
            // Property XmlInfo is not supposed to be null, see bug 4819
            var propertyFromXml = pv.PrimitiveType == PropertyPrimitiveType.Choice
                ? XmlModelSerializer.DeserializeCustomProperties(pv.XmlInfo).CustomProperties[0]
                : null;

            return new PropertyType
            {
                Id = pv.PropertyTypeId,
                Name = pv.Name,
                VersionId = pv.VersionId,
                InstancePropertyTypeId = pv.InstancePropertyTypeId,
                PrimitiveType = pv.PrimitiveType,
                IsRichText = pv.PrimitiveType == PropertyPrimitiveType.Text ? pv.RichText : null,
                IsRequired = pv.Required,
                IsValidated = pv.PrimitiveType == PropertyPrimitiveType.Number
                                    || pv.PrimitiveType == PropertyPrimitiveType.Date
                                    || pv.PrimitiveType == PropertyPrimitiveType.Choice
                                    ? pv.Validate : null,
                IsMultipleAllowed = pv.PrimitiveType == PropertyPrimitiveType.Text
                                    || pv.PrimitiveType == PropertyPrimitiveType.Choice 
                                    ? pv.AllowMultiple : null,
                StringDefaultValue = pv.PrimitiveType == PropertyPrimitiveType.Text ? pv.StringDefaultValue : null,
                DateDefaultValue = pv.PrimitiveType == PropertyPrimitiveType.Date ? pv.DateDefaultValue : null,
                DecimalDefaultValue = pv.PrimitiveType == PropertyPrimitiveType.Number
                                      ? PropertyHelper.ToDecimal(pv.DecimalDefaultValue) : null,
                UserGroupDefaultValue = pv.PrimitiveType == PropertyPrimitiveType.User 
                                      ? PropertyHelper.ParseUserGroups(pv.UserDefaultValue) : null,
                MinDate = pv.PrimitiveType == PropertyPrimitiveType.Date && pv.Validate.GetValueOrDefault() ? pv.MinDate : null,
                MaxDate = pv.PrimitiveType == PropertyPrimitiveType.Date && pv.Validate.GetValueOrDefault() ? pv.MaxDate : null,
                MinNumber = pv.PrimitiveType == PropertyPrimitiveType.Number && pv.Validate.GetValueOrDefault()
                                      ? PropertyHelper.ToDecimal(pv.MinNumber) : null,
                MaxNumber = pv.PrimitiveType == PropertyPrimitiveType.Number && pv.Validate.GetValueOrDefault()
                                      ? PropertyHelper.ToDecimal(pv.MaxNumber) : null,
                DecimalPlaces = pv.PrimitiveType == PropertyPrimitiveType.Number ? pv.DecimalPlaces : null,
                ValidValues = pv.PrimitiveType == PropertyPrimitiveType.Choice
                                      ? propertyFromXml?.ValidValues.OrderBy(v => I18NHelper.Int32ParseInvariant(v.OrderIndex))
                                      .Select(v =>
                                      {
                                          int? vvId = null;
                                          if (!string.IsNullOrWhiteSpace(v.LookupListItemId))
                                          {
                                              int intValue;
                                              if (int.TryParse(v.LookupListItemId, out intValue))
                                                  vvId = intValue;
                                          }
                                          return new ValidValue { Id = vvId, Value = v.Value };
                                      }).ToList()
                                      : null,
                DefaultValidValueId = pv.PrimitiveType == PropertyPrimitiveType.Choice
                                      ? FindDefaultValidValueId(propertyFromXml.ValidValues) // TODO
                                      : null
            };
        }

        private ItemType ConvertItemTypeVersion(ItemTypeVersion itv, IEnumerable<int> ptIds, int projectId)
        {
            var it = new ItemType
            {
                Id = itv.ItemTypeId,
                Name = itv.Name,
                ProjectId = projectId,
                VersionId = itv.VersionId,
                InstanceItemTypeId = itv.InstanceItemTypeId,
                PredefinedType = itv.BasePredefined ?? itv.Predefined,
                Prefix = itv.Prefix,
                IconImageId = itv.IconImageId,
                UsedInThisProject = itv.UsedInThisProject
            };

            if (ptIds != null)
                it.CustomPropertyTypeIds.AddRange(ptIds);

            return it;
        }

        private static int? FindDefaultValidValueId(List<XmlCustomPropertyValidValue> validValues)
        {
            if (validValues == null)
                return null;

            var orderedValidValues = validValues.OrderBy(v => I18NHelper.Int32ParseInvariant(v.OrderIndex)).ToList();
            for (var i = 0; i < orderedValidValues.Count; i++)
            {
                var validValue = orderedValidValues.ElementAt(i);
                if (validValue?.Selected == "1")
                {
                    int? vvId = null;
                    if (!string.IsNullOrWhiteSpace(validValue.LookupListItemId))
                    {
                        int intValue;
                        if (int.TryParse(validValue.LookupListItemId, out intValue))
                            vvId = intValue;
                    }
                    return vvId;
                }
            }

            return null;
        }

        public async Task<IEnumerable<ProjectApprovalStatus>> GetApprovalStatusesAsync(int projectId, int userId)
        {
            if (projectId < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(projectId));
            }

            await CheckProjectIsAccessible(projectId, userId);

            var projectSettings = await GetProjectSettingsAsync(projectId, PropertyTypePredefined.ApprovalStatus);

            return projectSettings.Select(MapApprovalStatus);
        }

        private Task<IEnumerable<ProjectSetting>> GetProjectSettingsAsync(int projectId, PropertyTypePredefined? propertyType = null, bool includeDeleted = false)
        {
            var parameters = new DynamicParameters();

            parameters.Add("@projectId", projectId);
            parameters.Add("@propertyType", propertyType);
            parameters.Add("@includeDeleted", includeDeleted);

            return _connectionWrapper.QueryAsync<ProjectSetting>("GetProjectSettings", parameters, commandType: CommandType.StoredProcedure);
        }

        private ProjectApprovalStatus MapApprovalStatus(ProjectSetting projectSetting)
        {
            var values = projectSetting.Setting.Split(';');

            if (values.Length != 2)
            {
                throw new ArgumentException("Unexpected Approval Status setting format: " + projectSetting.Setting);
            }

            var approvalTypeString = values[0];
            var statusText = values[1];

            ApprovalType approvalType;

            if (String.IsNullOrEmpty(approvalTypeString))
            {
                approvalType = ApprovalType.NotSpecified;
            }
            else
            {
                bool isApproved;
                
                bool parsed = Boolean.TryParse(approvalTypeString, out isApproved);

                if (!parsed)
                {
                    throw new ArgumentException("Unexpected Approval Status setting format: " + projectSetting.Setting);
                }

                if (isApproved)
                {
                    approvalType = ApprovalType.Approved;
                }
                else
                {
                    approvalType = ApprovalType.Disapproved;
                }
            }

            // For the default not specified approval status, we want to display Pending to be consistent with SilverLight
            if (approvalType == ApprovalType.NotSpecified && projectSetting.ReadOnly
               && statusText.Equals("Not Specified", StringComparison.OrdinalIgnoreCase))
            {
                statusText = "Pending";
            }

            return new ProjectApprovalStatus()
            {
                ApprovalType = approvalType,
                StatusText = statusText,
                IsPreset = projectSetting.ReadOnly
            };
        }

        internal class ProjectVersion
        {
            internal bool? IsAccesible { get; set; }
        }

        public class PropertyTypeVersion
        {
            internal int PropertyTypeId { get; set; }
            internal int VersionId { get; set; }
            internal int? InstancePropertyTypeId { get; set; }
            internal string Name { get; set; }
            internal PropertyPrimitiveType PrimitiveType { get; set; }
            internal bool? RichText { get; set; }
            internal byte[] DecimalDefaultValue { get; set; }
            internal DateTime? DateDefaultValue { get; set; }
            internal string UserDefaultValue { get; set; }
            internal string StringDefaultValue { get; set; }
            internal int? DecimalPlaces { get; set; }
            internal byte[] MaxNumber { get; set; }
            internal byte[] MinNumber { get; set; }
            internal DateTime? MaxDate { get; set; }
            internal DateTime? MinDate { get; set; }
            internal bool? AllowMultiple { get; set; }
            internal bool? Required { get; set; }
            internal bool? Validate { get; set; }
            internal string XmlInfo { get; set; }


        }

        public class ItemTypeVersion
        {
            internal int ItemTypeId { get; set; }
            internal int VersionId { get; set; }
            internal ItemTypePredefined? Predefined { get; set; }
            internal ItemTypePredefined? BasePredefined { get; set; }
            internal int? InstanceItemTypeId { get; set; }
            internal string Name { get; set; }
            internal string Prefix { get; set; }
            internal int? IconImageId { get; set; }
            internal int? ItemTypeGroupId { get; set; }
            internal bool UsedInThisProject { get; set; }
            internal string AdvancedSettings { get; set; }
        }

        public class ItemTypePropertyTypeMapRecord
        {
            internal int ItemTypeId { get; set; }
            internal int PropertyTypeId { get; set; }
        }


        // Lightweight Advanced Settings object model for ordering properties in Nova
        [XmlRoot("AdvancedSettings", Namespace = "http://www.blueprintsys.com/RC2011", IsNullable = false)]
        public class AdvancedSettings
        {
            [XmlArray]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:For unit tests.")]
            public List<PropertyLayoutGroup> LayoutGroups { get; set; }
        }

        public class PropertyLayoutGroup
        {
            [XmlAttribute]
            public GroupType Type { get; set; }
            [XmlArray]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:For unit tests.")]
            public List<PropertyLayout> Properties { get; set; }
        }

        public enum GroupType
        {
            General, Details, Custom
        }

        public class PropertyLayout
        {
            [XmlAttribute]
            public int PropertyTypeId { get; set; }
            [XmlAttribute]
            public int OrderIndex { get; set; }
        }
    }
}