using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArtifactStore.Models;
using ArtifactStore.Models.Reuse;
using ArtifactStore.Repositories.Reuse;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;

namespace ArtifactStore.Helpers
{
    public interface ISensitivityCommonHelper
    {
        Task<ISet<int>> FilterInsensitiveItems(ICollection<int> affectedItems,
            ReuseSensitivityCollector sensitivityCollector,
            IReuseRepository reuseRepository);
    }

    public class SensitivityCommonHelper : ISensitivityCommonHelper
    {
        public async Task<ISet<int>> FilterInsensitiveItems(ICollection<int> affectedItems,
            ReuseSensitivityCollector sensitivityCollector,
            IReuseRepository reuseRepository)
        {
            var modifiedArtifacts = GetModifiedArtifacts(affectedItems, sensitivityCollector);
            var result = new HashSet<int>();

            if (modifiedArtifacts.IsEmpty())
            {
                return result;
            }
            //TODO: we can improve the artifact type Id retrieval as the inforamtion is already loaded into memory
            var artifactId2StandardTypeId =
                await
                    reuseRepository.GetStandardTypeIdsForArtifactsIdsAsync(
                        modifiedArtifacts.ToHashSet());

            var reuseTemplatesDic = await reuseRepository.GetReuseItemTypeTemplatesAsyc(
                artifactId2StandardTypeId.Values.Where(v => v?.InstanceTypeId != null).
                    Select(v => v.InstanceTypeId.Value).ToHashSet());

            foreach (var itemId in modifiedArtifacts)
            {
                SqlItemTypeInfo standardTypeInfo;
                if (!artifactId2StandardTypeId.TryGetValue(itemId, out standardTypeInfo) || standardTypeInfo == null)
                {
                    continue;
                }

                ItemTypeReuseTemplate settings;
                if (reuseTemplatesDic.TryGetValue(standardTypeInfo.InstanceTypeId.GetValueOrDefault(0), out settings) && settings != null)
                {
                    var modification = sensitivityCollector.ArtifactModifications[itemId];

                    if ((modification.ArtifactAspects & (~settings.SensitivitySettings)) !=
                        ItemTypeReuseTemplateSetting.None
                        ||
                        await HasSensitiveModifiedProperty(modification, settings, standardTypeInfo.ItemTypePredefined, reuseRepository))
                    {
                        result.Add(itemId);
                    }
                }
                else
                {
                    //If no template settings - everything sensitive by default
                    result.Add(itemId);
                }
            }
            return result;
        }

        private async Task<bool> HasSensitiveModifiedProperty(
            ReuseSensitivityCollector.ArtifactModification modification,
            ItemTypeReuseTemplate itemTypeReuseTemplate,
            ItemTypePredefined artifacTypePredefined,
            IReuseRepository reuseRepository)
        {
            HashSet<int> modifiedPropertyTypes = new HashSet<int>();
            foreach (var modifiedPropertyType in modification.ModifiedPropertyTypes)
            {
                modifiedPropertyTypes.Add(modifiedPropertyType.Item1);
            }
            var propertyTypesToStandardPropertyTypeDict =
                await reuseRepository.GetStandardPropertyTypeIdsForPropertyIdsAsync(modifiedPropertyTypes);

            foreach (var modifiedPropertyType in modification.ModifiedPropertyTypes)
            {
                var propertyTypePredefined = modifiedPropertyType.Item2;
                if (propertyTypePredefined.IsCustom())
                {
                    SqlPropertyTypeInfo propInfo;
                    if (
                        !propertyTypesToStandardPropertyTypeDict.TryGetValue(modifiedPropertyType.Item1, out propInfo) ||
                        !propInfo.InstancePropertyTypeId.HasValue)
                    {
                        continue;
                    }

                    PropertyTypeReuseTemplate propertyTemplate;
                    //Get corresponding property type template for standard property
                    if (
                        !itemTypeReuseTemplate.PropertyTypeReuseTemplates.TryGetValue(
                            propInfo.InstancePropertyTypeId.Value,
                            out propertyTemplate))
                    {
                        continue;
                    }

                    if (!propertyTemplate.Settings.HasFlag(PropertyTypeReuseTemplateSettings.ChangesIgnored))
                    {
                        return true;
                    }
                }
                else if (!propertyTypePredefined.IsFake())
                {
                    ItemTypeReuseTemplateSetting correspondingReuseTemplateSetting;

                    if (propertyTypePredefined == PropertyTypePredefined.Description)
                    {
                        correspondingReuseTemplateSetting = ItemTypeReuseTemplateSetting.Description;
                    }
                    else
                    {
                        var reconcileProperty =
                            ReuseSystemPropertiesMap.Instance.GetCorrespondingReconcileProperty(propertyTypePredefined,
                                artifacTypePredefined);

                        correspondingReuseTemplateSetting =
                            ReuseTemplateSettingsMap.GetCorrespondingReuseTemplateSetting(reconcileProperty);
                    }

                    if ((correspondingReuseTemplateSetting & ~itemTypeReuseTemplate.SensitivitySettings) !=
                        ItemTypeReuseTemplateSetting.None)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private HashSet<int> GetModifiedArtifacts(ICollection<int> affectedItems,
            ReuseSensitivityCollector sensitivityCollector)
        {
            var modifiedArtifacts = new HashSet<int>();

            foreach (var affectedItem in affectedItems)
            {
                ReuseSensitivityCollector.ArtifactModification modifications;
                if (
                    !sensitivityCollector.ArtifactModifications.TryGetValue(affectedItem,
                        out modifications))
                {
                    continue;
                }

                if (modifications.ArtifactAspects == ItemTypeReuseTemplateSetting.None &&
                    modifications.ModifiedPropertyTypes.IsEmpty())
                {
                    continue;
                }

                modifiedArtifacts.Add(affectedItem);
            }
            return modifiedArtifacts;
        }
    }
}