using System.Collections.Generic;

namespace Model.ArtifactModel.Enums
{
    /// <summary>
    /// Contains extention functions for the ItemTypePredefined enum.
    /// Taken from:  blueprint-current/Source/BluePrintSys.RC.CrossCutting.Portable/Enums/ItemTypePredefinedExtension.cs
    /// </summary>
    public static class ItemTypePredefinedExtension
    {
        /// <summary>
        /// This is a map of ItemTypePredefined enum values to BaseArtifactType enum values.
        /// </summary>
        internal static Dictionary<ItemTypePredefined, BaseArtifactType> ItemTypePredefinedToBaseArtifactTypeMap { get; } =
            new Dictionary<ItemTypePredefined, BaseArtifactType>
        {
            {ItemTypePredefined.None,                     BaseArtifactType.Undefined},
            {ItemTypePredefined.PrimitiveFolder,          BaseArtifactType.PrimitiveFolder},
            {ItemTypePredefined.Glossary,                 BaseArtifactType.Glossary},
            {ItemTypePredefined.TextualRequirement,       BaseArtifactType.TextualRequirement},
            {ItemTypePredefined.BusinessProcess,          BaseArtifactType.BusinessProcess},
            {ItemTypePredefined.Actor,                    BaseArtifactType.Actor},
            {ItemTypePredefined.UseCase,                  BaseArtifactType.UseCase},
            {ItemTypePredefined.DataElement,              BaseArtifactType.DataElement},
            {ItemTypePredefined.UIMockup,                 BaseArtifactType.UIMockup},
            {ItemTypePredefined.GenericDiagram,           BaseArtifactType.GenericDiagram},
            {ItemTypePredefined.Document,                 BaseArtifactType.Document},
            {ItemTypePredefined.Storyboard,               BaseArtifactType.Storyboard},
            {ItemTypePredefined.DomainDiagram,            BaseArtifactType.DomainDiagram},
            {ItemTypePredefined.UseCaseDiagram,           BaseArtifactType.UseCaseDiagram},
            {ItemTypePredefined.Baseline,                 BaseArtifactType.Baseline},
            {ItemTypePredefined.BaselineFolder,           BaseArtifactType.BaselineFolder},
            {ItemTypePredefined.ArtifactBaseline,         BaseArtifactType.ArtifactBaseline},
            {ItemTypePredefined.ArtifactReviewPackage,    BaseArtifactType.ArtifactReviewPackage},
            {ItemTypePredefined.Process,                  BaseArtifactType.Process}
        };

        /// <summary>
        /// Converts this ItemTypePredefined enum value to its BaseArtifactType equivalent.
        /// </summary>
        /// <param name="itemType">The ItemTypePredefined to convert.</param>
        /// <returns>The BaseArtifactType version of this ItemTypePredefined.</returns>
        public static BaseArtifactType ToBaseArtifactType(this ItemTypePredefined itemType)
        {
            return ItemTypePredefinedToBaseArtifactTypeMap[itemType];
        }

        /// <summary>
        /// Returns true if this is a primitive artifact type.
        /// </summary>
        /// <param name="artifactType">The item type to check.</param>
        /// <returns>True if this is a primitive artifact type.</returns>
        public static bool IsPrimitiveArtifactType(this ItemTypePredefined artifactType)
        {
            //not artifact
            if (((int)ItemTypeEnumGroups.GroupMask & (int)artifactType) != (int)ItemTypeEnumGroups.PrimitiveArtifactGroup)
            {
                return false;
            }

            if (((int)artifactType == (int)ItemTypeEnumGroups.PrimitiveArtifactGroup) || (artifactType == ItemTypePredefined.Project) || (artifactType == ItemTypePredefined.Baseline))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns true if this is a custom artifact type.
        /// </summary>
        /// <param name="artifactType">The item type to check.</param>
        /// <returns>True if this is a custom artifact type.</returns>
        public static bool IsCustomArtifactType(this ItemTypePredefined artifactType)
        {
            return (((int)ItemTypeEnumGroups.GroupMask & (int)artifactType) == (int)ItemTypeEnumGroups.CustomArtifactGroup);
        }

        /// <summary>
        /// Returns true if this is a baseline or review.
        /// </summary>
        /// <param name="artifactType">The item type to check.</param>
        /// <returns>True if this is a baseline or review.</returns>
        public static bool IsBaselinesAndReviewsGroupType(this ItemTypePredefined artifactType)
        {
            return ((int)artifactType & (int)ItemTypeEnumGroups.BaselineArtifactGroup) != 0;
        }

        /// <summary>
        /// Returns true if this is a collection artifact type.
        /// </summary>
        /// <param name="artifactType">The item type to check.</param>
        /// <returns>True if this is a collection artifact type.</returns>
        public static bool IsCollectionsGroupType(this ItemTypePredefined artifactType)
        {
            return ((int)artifactType & (int)ItemTypeEnumGroups.CollectionArtifactGroup) != 0;
        }

        /// <summary>
        /// Returns true if this is an obsolete artifact type.
        /// </summary>
        /// <param name="artifactType">The item type to check.</param>
        /// <returns>True if this is an obsolete artifact type.</returns>
        public static bool IsObsoleteGroupType(this ItemTypePredefined artifactType)
        {
            return ((int)artifactType & (int)ItemTypeEnumGroups.ObsoleteArtifactGroup) != 0;
        }

        /// <summary>
        /// Returns true if this is a sub-artifact type.
        /// </summary>
        /// <param name="predefinedType">The item type to check.</param>
        /// <returns>True if this is a sub-artifact type.</returns>
        public static bool IsSubArtifactType(this ItemTypePredefined predefinedType)
        {
            return ((int)ItemTypeEnumGroups.GroupMask & (int)predefinedType) == (int)ItemTypeEnumGroups.SubArtifactGroup;
        }

        /// <summary>
        /// Returns true if this is a project or folder artifact type.
        /// </summary>
        /// <param name="predefinedType">The item type to check.</param>
        /// <returns>True if this is a project or folder artifact type.</returns>
        public static bool IsProjectOrFolderArtifactType(this ItemTypePredefined predefinedType)
        {
            return predefinedType == ItemTypePredefined.BaselineFolder
                || predefinedType == ItemTypePredefined.PrimitiveFolder
                || predefinedType == ItemTypePredefined.Project
                || predefinedType == ItemTypePredefined.CollectionFolder;
        }

        /// <summary>
        /// Returns true if this type can contain sub-artifacts.
        /// </summary>
        /// <param name="itemTypePredefined">The item type to check.</param>
        /// <returns>True if this type can contain sub-artifacts.</returns>
        public static bool CanContainSubartifacts(this ItemTypePredefined itemTypePredefined)
        {
            return itemTypePredefined == ItemTypePredefined.BusinessProcess ||
                   itemTypePredefined == ItemTypePredefined.DomainDiagram ||
                   itemTypePredefined == ItemTypePredefined.GenericDiagram ||
                   itemTypePredefined == ItemTypePredefined.Glossary ||
                   itemTypePredefined == ItemTypePredefined.Storyboard ||
                   itemTypePredefined == ItemTypePredefined.UIMockup ||
                   itemTypePredefined == ItemTypePredefined.UseCase ||
                   itemTypePredefined == ItemTypePredefined.UseCaseDiagram;
        }

        /// <summary>
        /// Returns true if this is a diagram type.
        /// </summary>
        /// <param name="itemTypePredefined">The item type to check.</param>
        /// <returns>True if this is a diagram type.</returns>
        public static bool IsDiagram(this ItemTypePredefined itemTypePredefined)
        {
            return itemTypePredefined == ItemTypePredefined.BusinessProcess ||
                   itemTypePredefined == ItemTypePredefined.DomainDiagram ||
                   itemTypePredefined == ItemTypePredefined.GenericDiagram ||
                   itemTypePredefined == ItemTypePredefined.Storyboard ||
                   itemTypePredefined == ItemTypePredefined.UIMockup ||
                   itemTypePredefined == ItemTypePredefined.UseCaseDiagram;
        }

        /// <summary>
        /// Returns true if this is a diagram shape type.
        /// </summary>
        /// <param name="itemTypePredefined">The item type to check.</param>
        /// <returns>True if this is a diagram shape type.</returns>
        public static bool IsDiagramShape(this ItemTypePredefined itemTypePredefined)
        {
            return itemTypePredefined == ItemTypePredefined.BPShape ||
                   itemTypePredefined == ItemTypePredefined.DDShape ||
                   itemTypePredefined == ItemTypePredefined.GDShape ||
                   itemTypePredefined == ItemTypePredefined.SBShape ||
                   itemTypePredefined == ItemTypePredefined.UIShape ||
                   itemTypePredefined == ItemTypePredefined.UCDShape;
        }

        /// <summary>
        /// Returns true if this is a diagram connector type.
        /// </summary>
        /// <param name="itemTypePredefined">The item type to check.</param>
        /// <returns>True if this is a diagram connector type.</returns>
        public static bool IsDiagramConnector(this ItemTypePredefined itemTypePredefined)
        {
            return itemTypePredefined == ItemTypePredefined.BPConnector ||
                   itemTypePredefined == ItemTypePredefined.DDConnector ||
                   itemTypePredefined == ItemTypePredefined.GDConnector ||
                   itemTypePredefined == ItemTypePredefined.SBConnector ||
                   itemTypePredefined == ItemTypePredefined.UIConnector ||
                   itemTypePredefined == ItemTypePredefined.UCDConnector;
        }

        /// <summary>
        /// Returns true if this type can have attachments.
        /// </summary>
        /// <param name="itemTypePredefined">The item type to check.</param>
        /// <returns>True if this type can have attachments.</returns>
        public static bool CanHaveAttachments(this ItemTypePredefined itemTypePredefined)
        {
            return itemTypePredefined == ItemTypePredefined.Actor ||
                   itemTypePredefined == ItemTypePredefined.Baseline ||
                   itemTypePredefined == ItemTypePredefined.BaselineFolder ||
                   itemTypePredefined == ItemTypePredefined.BusinessProcess ||
                   itemTypePredefined == ItemTypePredefined.DomainDiagram ||
                   itemTypePredefined == ItemTypePredefined.PrimitiveFolder ||
                   itemTypePredefined == ItemTypePredefined.GenericDiagram ||
                   itemTypePredefined == ItemTypePredefined.Glossary ||
                   itemTypePredefined == ItemTypePredefined.ArtifactReviewPackage ||
                   itemTypePredefined == ItemTypePredefined.Storyboard ||
                   itemTypePredefined == ItemTypePredefined.TextualRequirement ||
                   itemTypePredefined == ItemTypePredefined.UIMockup ||
                   itemTypePredefined == ItemTypePredefined.UseCase ||
                   itemTypePredefined == ItemTypePredefined.UseCaseDiagram;
        }

        /// <summary>
        /// Returns true if this type can have document references.
        /// </summary>
        /// <param name="itemTypePredefined">The item type to check.</param>
        /// <returns>True if this type can have document references.</returns>
        public static bool CanHaveDocumentReferences(this ItemTypePredefined itemTypePredefined)
        {
            return itemTypePredefined == ItemTypePredefined.Actor ||
                   itemTypePredefined == ItemTypePredefined.BaselineFolder ||
                   itemTypePredefined == ItemTypePredefined.BusinessProcess ||
                   itemTypePredefined == ItemTypePredefined.DomainDiagram ||
                   itemTypePredefined == ItemTypePredefined.PrimitiveFolder ||
                   itemTypePredefined == ItemTypePredefined.GenericDiagram ||
                   itemTypePredefined == ItemTypePredefined.Glossary ||
                   itemTypePredefined == ItemTypePredefined.Storyboard ||
                   itemTypePredefined == ItemTypePredefined.TextualRequirement ||
                   itemTypePredefined == ItemTypePredefined.UIMockup ||
                   itemTypePredefined == ItemTypePredefined.UseCase ||
                   itemTypePredefined == ItemTypePredefined.UseCaseDiagram;
        }

        /// <summary>
        /// Returns true if this type is available for Sensitivity Calculations (i.e. Mark Suspect in Reuse).
        /// </summary>
        /// <param name="itemTypePredefined">The item type to check.</param>
        /// <returns>True if this type is available for Sensitivity Calculations.</returns>
        public static bool IsAvailableForSensitivityCalculations(this ItemTypePredefined itemTypePredefined)
        {
            return itemTypePredefined == ItemTypePredefined.Actor ||
                   itemTypePredefined == ItemTypePredefined.BusinessProcess ||
                   itemTypePredefined == ItemTypePredefined.DomainDiagram ||
                   itemTypePredefined == ItemTypePredefined.Document ||
                   itemTypePredefined == ItemTypePredefined.PrimitiveFolder ||
                   itemTypePredefined == ItemTypePredefined.GenericDiagram ||
                   itemTypePredefined == ItemTypePredefined.Glossary ||
                   itemTypePredefined == ItemTypePredefined.Storyboard ||
                   itemTypePredefined == ItemTypePredefined.TextualRequirement ||
                   itemTypePredefined == ItemTypePredefined.UIMockup ||
                   itemTypePredefined == ItemTypePredefined.UseCase ||
                   itemTypePredefined == ItemTypePredefined.UseCaseDiagram;
        }

        /// <summary>
        /// Returns true if this type is available for Impact Analysis.
        /// </summary>
        /// <param name="itemTypePredefined">The item type to check.</param>
        /// <returns>True if this type is available for Impact Analysis.</returns>
        public static bool IsAvailableForImpactAnalysis(this ItemTypePredefined itemTypePredefined)
        {
            return itemTypePredefined == ItemTypePredefined.Actor ||
                   itemTypePredefined == ItemTypePredefined.BusinessProcess ||
                   itemTypePredefined == ItemTypePredefined.DomainDiagram ||
                   itemTypePredefined == ItemTypePredefined.Document ||
                   itemTypePredefined == ItemTypePredefined.PrimitiveFolder ||
                   itemTypePredefined == ItemTypePredefined.GenericDiagram ||
                   itemTypePredefined == ItemTypePredefined.Glossary ||
                   itemTypePredefined == ItemTypePredefined.Storyboard ||
                   itemTypePredefined == ItemTypePredefined.TextualRequirement ||
                   itemTypePredefined == ItemTypePredefined.UIMockup ||
                   itemTypePredefined == ItemTypePredefined.UseCase ||
                   itemTypePredefined == ItemTypePredefined.UseCaseDiagram ||
                   IsSubArtifactType(itemTypePredefined);
        }
    }
}
