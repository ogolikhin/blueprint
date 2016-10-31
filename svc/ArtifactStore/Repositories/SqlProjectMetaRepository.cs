using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ArtifactStore.Helpers;
using ArtifactStore.Models;
using ArtifactStore.Repositories.PropertyXml;
using ArtifactStore.Repositories.PropertyXml.Models;
using Dapper;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;

namespace ArtifactStore.Repositories
{
    public class SqlProjectMetaRepository : ISqlProjectMetaRepository
    {
        private readonly static ISet<ItemTypePredefined> HiddenSubartifactTypes = new HashSet<ItemTypePredefined> {
            ItemTypePredefined.Content,
            ItemTypePredefined.BaselinedArtifactSubscribe,
            ItemTypePredefined.Extension,
            ItemTypePredefined.Flow
        };

        internal readonly ISqlConnectionWrapper ConnectionWrapper;

        public SqlProjectMetaRepository()
            : this(new SqlConnectionWrapper(ServiceConstants.RaptorMain))
        {
        }

        internal SqlProjectMetaRepository(ISqlConnectionWrapper connectionWrapper)
        {
            ConnectionWrapper = connectionWrapper;
        }

        public async Task<ProjectTypes> GetCustomProjectTypesAsync(int projectId, int userId)
        {
            if (projectId < 1)
                throw new ArgumentOutOfRangeException(nameof(projectId));
            if (userId < 1)
                throw new ArgumentOutOfRangeException(nameof(userId));

            var prm = new DynamicParameters();
            prm.Add("@projectId", projectId);
            prm.Add("@userId", userId);

            var project = (await ConnectionWrapper.QueryAsync<ProjectVersion>("GetInstanceProjectById", prm, commandType: CommandType.StoredProcedure))?.FirstOrDefault();
            if (project == null)
                throw new ResourceNotFoundException(string.Format("Project (Id:{0}) is not found.", projectId), ErrorCodes.ResourceNotFound);
            if (!project.IsAccesible.GetValueOrDefault())
                throw new AuthorizationException(string.Format("The user does not have permissions for Project (Id:{0}).", projectId), ErrorCodes.UnauthorizedAccess);

            prm = new DynamicParameters();
            prm.Add("@projectId", projectId);
            prm.Add("@revisionId", ServiceConstants.VersionHead);

            var typeGrid =
                (await
                    ConnectionWrapper.QueryMultipleAsync<PropertyTypeVersion, ItemTypeVersion, ItemTypePropertyTypeMapRecord>("GetProjectCustomTypes", prm,
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
                var at = ConvertItemTypeVersion(itv, ptIds, projectId);

                if(itv.Predefined != null && itv.Predefined.Value.HasFlag(ItemTypePredefined.CustomArtifactGroup))
                    projectTypes.ArtifactTypes.Add(at);
                else if (itv.Predefined != null && itv.Predefined.Value.HasFlag(ItemTypePredefined.SubArtifactGroup))
                    projectTypes.SubArtifactTypes.Add(at);
            }

            return projectTypes;
        }

        private PropertyType ConvertPropertyTypeVersion(PropertyTypeVersion pv)
        {
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
                UserGroupDefaultValue = pv.PrimitiveType == PropertyPrimitiveType.User ? ParseUserGroups(pv.UserDefaultValue) : null,
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
                                          return new ValidValue {Id = vvId, Value = v.Value};
                                      }).ToList()
                                      : null,
                DefaultValidValueId = pv.PrimitiveType == PropertyPrimitiveType.Choice
                                      ? FindDefaultValidValueId(propertyFromXml.ValidValues)//TODO
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

            if(ptIds != null)
                it.CustomPropertyTypeIds.AddRange(ptIds);

            return it;
        }

        private static List<UserGroup> ParseUserGroups(string userGroups)
        {
            if (string.IsNullOrWhiteSpace(userGroups))
                return null;

            var result = new List<UserGroup>();
            var tokens = userGroups.Split('\n');
            foreach (var token in tokens)
            {
                var isGroup = token.StartsWith("g", StringComparison.Ordinal);
                int id;
                if (int.TryParse(isGroup ? token.TrimStart('g') : token, out id))
                    result.Add(new UserGroup {Id = id, IsGroup = isGroup});
            }

            return result;
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

        internal class ProjectVersion
        {
            internal bool? IsAccesible { get; set; }
        }

        internal class PropertyTypeVersion
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

        internal class ItemTypeVersion
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
        }

        internal class ItemTypePropertyTypeMapRecord
        {
            internal int ItemTypeId { get; set; }
            internal int PropertyTypeId { get; set; }
        }
    }
}