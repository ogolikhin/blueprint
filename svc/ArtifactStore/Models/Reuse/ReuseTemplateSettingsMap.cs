using System.Collections.Generic;
using System.Collections.ObjectModel;
using ServiceLibrary.Models.Enums;

namespace ArtifactStore.Models.Reuse
{
    public static class ReuseTemplateSettingsMap
    {
        public static IDictionary<ReconcileProperty, ItemTypeReuseTemplateSetting> ReconcilePropertyToItemTypeReuseTemplateSettingsMap
            = new ReadOnlyDictionary<ReconcileProperty, ItemTypeReuseTemplateSetting>(
                new Dictionary<ReconcileProperty, ItemTypeReuseTemplateSetting>
                {
                    { ReconcileProperty.None, ItemTypeReuseTemplateSetting.None },
                    { ReconcileProperty.Name, ItemTypeReuseTemplateSetting.Name },
                    { ReconcileProperty.ActorBase, ItemTypeReuseTemplateSetting.BaseActor },
                    { ReconcileProperty.ActorImageName, ItemTypeReuseTemplateSetting.ActorImage },
                    { ReconcileProperty.DiagramHeight, ItemTypeReuseTemplateSetting.DiagramHeight },
                    { ReconcileProperty.DiagramWidth, ItemTypeReuseTemplateSetting.DiagramWidth },
                    { ReconcileProperty.DocumentFileName, ItemTypeReuseTemplateSetting.DocumentFile },
                    { ReconcileProperty.UseCaseDiagramShowConditions, ItemTypeReuseTemplateSetting.UseCaseDiagramShowConditions },
                    { ReconcileProperty.UIMockupTheme, ItemTypeReuseTemplateSetting.UIMockupTheme },
                    { ReconcileProperty.UseCaseLevel, ItemTypeReuseTemplateSetting.UseCaseLevel }
                });

        public static ItemTypeReuseTemplateSetting GetCorrespondingReuseTemplateSetting(ReconcileProperty property)
        {
            ItemTypeReuseTemplateSetting setting;
            if (!ReconcilePropertyToItemTypeReuseTemplateSettingsMap.TryGetValue(property, out setting))
                setting = ItemTypeReuseTemplateSetting.None;

            return setting;
        }
    }
}