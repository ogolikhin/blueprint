﻿using System;
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

namespace ServiceLibrary.Repositories.ProjectMeta
{
    public class SqlProjectMetaRepository : IProjectMetaRepository
    {
        private static readonly ISet<ItemTypePredefined> HiddenSubartifactTypes = new HashSet<ItemTypePredefined>
        {
            ItemTypePredefined.Content,
            ItemTypePredefined.BaselinedArtifactSubscribe,
            ItemTypePredefined.Extension,
            ItemTypePredefined.Flow
        };

        private readonly ISqlConnectionWrapper _connectionWrapper;

        public SqlProjectMetaRepository() : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        public SqlProjectMetaRepository(ISqlConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public async Task<ProjectTypes> GetCustomProjectTypesAsync(int projectId, int userId)
        {
            if (projectId < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(projectId));
            }

            if (userId < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(userId));
            }

            if (projectId > 0)
            {
                await CheckProjectIsAccessible(projectId, userId);
            }

            var parameters = new DynamicParameters();
            parameters.Add("@projectId", projectId);
            parameters.Add("@revisionId", ServiceConstants.VersionHead);

            var typeGrid = await _connectionWrapper
                .QueryMultipleAsync<PropertyTypeVersion, ItemTypeVersion, ItemTypePropertyTypeMapRecord>(
                    "GetProjectCustomTypes", parameters, commandType: CommandType.StoredProcedure);

            var ptVersions = typeGrid.Item1.ToList();
            var ptIdMap = new HashSet<int>(ptVersions.Select(p => p.PropertyTypeId));
            var itVersions =
                typeGrid.Item2.Where(i => !HiddenSubartifactTypes.Contains(i.Predefined.GetValueOrDefault()));
            var itPtMap = typeGrid.Item3.Where(r => ptIdMap.Contains(r.PropertyTypeId))
                .GroupBy(r => r.ItemTypeId).ToDictionary(r => r.Key, r => r.ToList().Select(mr => mr.PropertyTypeId));

            var projectTypes = new ProjectTypes();

            // Process properties
            foreach (var pv in ptVersions)
            {
                var pt = pv.ConvertToPropertyType();
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
                {
                    projectTypes.ArtifactTypes.Add(at);
                }
                else if (itv.Predefined != null && itv.Predefined.Value.HasFlag(ItemTypePredefined.SubArtifactGroup))
                {
                    projectTypes.SubArtifactTypes.Add(at);
                }
            }

            return projectTypes;
        }

        public async Task<ProjectTypes> GetStandardProjectTypesAsync()
        {
            // userId does not matter here as it's used only for projects with id greater than zero.
            return await GetCustomProjectTypesAsync(0, 1);
        }

        public async Task<IEnumerable<PropertyType>> GetStandardProjectPropertyTypesAsync(
            IEnumerable<int> predefinedTypeIds)
        {
            return await GetProjectPropertyTypesAsync(0, predefinedTypeIds);
        }

        public async Task<IEnumerable<PropertyType>> GetProjectPropertyTypesAsync(
            int projectId, IEnumerable<int> predefinedTypeIds)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@projectId", projectId);
            parameters.Add("@predefinedPropertyTypes", SqlConnectionWrapper.ToDataTable(predefinedTypeIds));

            var ptVersions = await _connectionWrapper.QueryAsync<PropertyTypeVersion>(
                "GetProjectPropertyTypes", parameters, commandType: CommandType.StoredProcedure);

            return ptVersions.Select(pv => pv.ConvertToPropertyType()).ToList();
        }

        private async Task CheckProjectIsAccessible(int projectId, int userId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@projectId", projectId);
            parameters.Add("@userId", userId);

            var project = (await _connectionWrapper.QueryAsync<ProjectVersion>("GetInstanceProjectById", parameters,
                commandType: CommandType.StoredProcedure))?.FirstOrDefault();

            if (project == null)
            {
                var errorMessage =
                    $"The project (Id:{projectId}) can no longer be accessed. It may have been deleted, or is no longer accessible by you.";
                throw new ResourceNotFoundException(errorMessage, ErrorCodes.ResourceNotFound);
            }

            if (!project.IsAccesible.GetValueOrDefault())
            {
                var errorMessage = $"The user does not have permissions for Project (Id:{projectId}).";
                throw new AuthorizationException(errorMessage, ErrorCodes.UnauthorizedAccess);
            }
        }

        #region Ordering properties

        private static IEnumerable<int> OrderProperties(
            List<int> itPtIds, IEnumerable<PropertyType> propertyTypes, string xmlAdvancedSettings)
        {
            if (itPtIds == null || !itPtIds.Any())
            {
                return itPtIds;
            }

            var advancedSettings = SerializationHelper.FromXml<AdvancedSettings>(xmlAdvancedSettings);

            return OrderProperties(itPtIds, propertyTypes, advancedSettings);
        }

        internal static List<int> OrderProperties(
            IEnumerable<int> propertyValueIds, IEnumerable<PropertyType> propertyTypes,
            AdvancedSettings advancedSettings)
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

        private static ItemType ConvertItemTypeVersion(ItemTypeVersion itv, IEnumerable<int> ptIds, int projectId)
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
            {
                it.CustomPropertyTypeIds.AddRange(ptIds);
            }

            return it;
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

        private Task<IEnumerable<ProjectSetting>> GetProjectSettingsAsync(
            int projectId, PropertyTypePredefined? propertyType = null, bool includeDeleted = false)
        {
            var parameters = new DynamicParameters();

            parameters.Add("@projectId", projectId);
            parameters.Add("@propertyType", propertyType);
            parameters.Add("@includeDeleted", includeDeleted);

            return _connectionWrapper.QueryAsync<ProjectSetting>(
                "GetProjectSettings", parameters, commandType: CommandType.StoredProcedure);
        }

        private static ProjectApprovalStatus MapApprovalStatus(ProjectSetting projectSetting)
        {
            var values = projectSetting.Setting.Split(';');

            if (values.Length != 2)
            {
                throw new ArgumentException("Unexpected Approval Status setting format: " + projectSetting.Setting);
            }

            var approvalTypeString = values[0];
            var statusText = values[1];

            ApprovalType approvalType;

            if (string.IsNullOrEmpty(approvalTypeString))
            {
                approvalType = ApprovalType.NotSpecified;
            }
            else
            {
                bool isApproved;

                var parsed = bool.TryParse(approvalTypeString, out isApproved);

                if (!parsed)
                {
                    throw new ArgumentException("Unexpected Approval Status setting format: " + projectSetting.Setting);
                }

                approvalType = isApproved ? ApprovalType.Approved : ApprovalType.Disapproved;
            }

            // For the default not specified approval status, we want to display Pending to be consistent with SilverLight
            if (approvalType == ApprovalType.NotSpecified && projectSetting.ReadOnly
                                                          && statusText.Equals("Not Specified",
                                                              StringComparison.OrdinalIgnoreCase))
            {
                statusText = "Pending";
            }

            return new ProjectApprovalStatus
            {
                ApprovalType = approvalType,
                StatusText = statusText,
                IsPreset = projectSetting.ReadOnly,
                Id = projectSetting.ProjectSettingId
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
            General,
            Details,
            Custom
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
