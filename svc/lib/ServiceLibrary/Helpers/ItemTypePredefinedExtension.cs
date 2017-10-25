using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;

namespace ServiceLibrary.Helpers
{
    // The content is copied from the Raptor solution.
    // KEEP IN SYNC!

    public static class ItemTypePredefinedExtension
    {
        public static bool IsPrimitiveArtifactType(this ItemTypePredefined artifactType)
        {
            // not artifact
            if (((int)ItemTypePredefined.GroupMask & (int)artifactType) != (int)ItemTypePredefined.PrimitiveArtifactGroup)
            {
                return false;
            }

            if ((artifactType == ItemTypePredefined.PrimitiveArtifactGroup) || (artifactType == ItemTypePredefined.Project) || (artifactType == ItemTypePredefined.Baseline))
            {
                return false;
            }

            return true;
        }

        public static bool IsRegularArtifactType(this ItemTypePredefined artifactType)
        {
            return artifactType.IsPrimitiveArtifactType() && artifactType < ItemTypePredefined.BaselineFolder;
        }

        public static bool IsCustomArtifactType(this ItemTypePredefined artifactType)
        {
            return (((int)ItemTypePredefined.GroupMask & (int)artifactType) == (int)ItemTypePredefined.CustomArtifactGroup);
        }

        public static bool IsBaselinesAndReviewsGroupType(this ItemTypePredefined artifactType)
        {
            return ((int)artifactType & (int)ItemTypePredefined.BaselineArtifactGroup) != 0;
        }

        public static bool IsCollectionsGroupType(this ItemTypePredefined artifactType)
        {
            return ((int)artifactType & (int)ItemTypePredefined.CollectionArtifactGroup) != 0;
        }

        public static bool IsObsoleteGroupType(this ItemTypePredefined artifactType)
        {
            return ((int)artifactType & (int)ItemTypePredefined.ObsoleteArtifactGroup) != 0;
        }

        public static bool IsSubArtifactType(this ItemTypePredefined predefinedType)
        {
            return ((int)ItemTypePredefined.GroupMask & (int)predefinedType) == (int)ItemTypePredefined.SubArtifactGroup;
        }

        public static bool IsProjectOrFolderArtifactType(this ItemTypePredefined predefinedType)
        {
            return predefinedType == ItemTypePredefined.BaselineFolder
                || predefinedType == ItemTypePredefined.PrimitiveFolder
                || predefinedType == ItemTypePredefined.Project
                || predefinedType == ItemTypePredefined.CollectionFolder;
        }

        public static bool CanContainSubartifacts(this ItemTypePredefined itemTypePredefined)
        {
            return itemTypePredefined == ItemTypePredefined.BusinessProcess ||
                   itemTypePredefined == ItemTypePredefined.DomainDiagram ||
                   itemTypePredefined == ItemTypePredefined.GenericDiagram ||
                   itemTypePredefined == ItemTypePredefined.Glossary ||
                   itemTypePredefined == ItemTypePredefined.Process ||
                   itemTypePredefined == ItemTypePredefined.Storyboard ||
                   itemTypePredefined == ItemTypePredefined.UIMockup ||
                   itemTypePredefined == ItemTypePredefined.UseCase ||
                   itemTypePredefined == ItemTypePredefined.UseCaseDiagram;
        }

        public static bool IsDiagram(this ItemTypePredefined itemTypePredefined)
        {
            return itemTypePredefined == ItemTypePredefined.BusinessProcess ||
                   itemTypePredefined == ItemTypePredefined.DomainDiagram ||
                   itemTypePredefined == ItemTypePredefined.GenericDiagram ||
                   itemTypePredefined == ItemTypePredefined.Storyboard ||
                   itemTypePredefined == ItemTypePredefined.UIMockup ||
                   itemTypePredefined == ItemTypePredefined.UseCaseDiagram;
        }

        public static bool IsDiagramShape(this ItemTypePredefined itemTypePredefined)
        {
            return itemTypePredefined == ItemTypePredefined.BPShape ||
                   itemTypePredefined == ItemTypePredefined.DDShape ||
                   itemTypePredefined == ItemTypePredefined.GDShape ||
                   itemTypePredefined == ItemTypePredefined.SBShape ||
                   itemTypePredefined == ItemTypePredefined.UIShape ||
                   itemTypePredefined == ItemTypePredefined.UCDShape;
        }

        public static bool IsDiagramConnector(this ItemTypePredefined itemTypePredefined)
        {
            return itemTypePredefined == ItemTypePredefined.BPConnector ||
                   itemTypePredefined == ItemTypePredefined.DDConnector ||
                   itemTypePredefined == ItemTypePredefined.GDConnector ||
                   itemTypePredefined == ItemTypePredefined.SBConnector ||
                   itemTypePredefined == ItemTypePredefined.UIConnector ||
                   itemTypePredefined == ItemTypePredefined.UCDConnector;
        }

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
                   itemTypePredefined == ItemTypePredefined.Process ||
                   itemTypePredefined == ItemTypePredefined.Storyboard ||
                   itemTypePredefined == ItemTypePredefined.TextualRequirement ||
                   itemTypePredefined == ItemTypePredefined.UIMockup ||
                   itemTypePredefined == ItemTypePredefined.UseCase ||
                   itemTypePredefined == ItemTypePredefined.UseCaseDiagram;
        }

        public static bool CanHaveDocumentReferences(this ItemTypePredefined itemTypePredefined)
        {
            return itemTypePredefined == ItemTypePredefined.Actor ||
                   itemTypePredefined == ItemTypePredefined.BaselineFolder ||
                   itemTypePredefined == ItemTypePredefined.BusinessProcess ||
                   itemTypePredefined == ItemTypePredefined.DomainDiagram ||
                   itemTypePredefined == ItemTypePredefined.PrimitiveFolder ||
                   itemTypePredefined == ItemTypePredefined.GenericDiagram ||
                   itemTypePredefined == ItemTypePredefined.Glossary ||
                   itemTypePredefined == ItemTypePredefined.Process ||
                   itemTypePredefined == ItemTypePredefined.Storyboard ||
                   itemTypePredefined == ItemTypePredefined.TextualRequirement ||
                   itemTypePredefined == ItemTypePredefined.UIMockup ||
                   itemTypePredefined == ItemTypePredefined.UseCase ||
                   itemTypePredefined == ItemTypePredefined.UseCaseDiagram;
        }

        public static bool IsAvailableForSensitivityCalculations(this ItemTypePredefined itemTypePredefined)
        {
            return itemTypePredefined == ItemTypePredefined.Actor ||
                   itemTypePredefined == ItemTypePredefined.BusinessProcess ||
                   itemTypePredefined == ItemTypePredefined.DomainDiagram ||
                   itemTypePredefined == ItemTypePredefined.Document ||
                   itemTypePredefined == ItemTypePredefined.PrimitiveFolder ||
                   itemTypePredefined == ItemTypePredefined.GenericDiagram ||
                   itemTypePredefined == ItemTypePredefined.Glossary ||
                   itemTypePredefined == ItemTypePredefined.Process ||
                   itemTypePredefined == ItemTypePredefined.Storyboard ||
                   itemTypePredefined == ItemTypePredefined.TextualRequirement ||
                   itemTypePredefined == ItemTypePredefined.UIMockup ||
                   itemTypePredefined == ItemTypePredefined.UseCase ||
                   itemTypePredefined == ItemTypePredefined.UseCaseDiagram;
        }

        public static bool IsAvailableForImpactAnalysis(this ItemTypePredefined itemTypePredefined)
        {
            return itemTypePredefined == ItemTypePredefined.Actor ||
                   itemTypePredefined == ItemTypePredefined.BusinessProcess ||
                   itemTypePredefined == ItemTypePredefined.DomainDiagram ||
                   itemTypePredefined == ItemTypePredefined.Document ||
                   itemTypePredefined == ItemTypePredefined.PrimitiveFolder ||
                   itemTypePredefined == ItemTypePredefined.GenericDiagram ||
                   itemTypePredefined == ItemTypePredefined.Glossary ||
                   itemTypePredefined == ItemTypePredefined.Process ||
                   itemTypePredefined == ItemTypePredefined.Storyboard ||
                   itemTypePredefined == ItemTypePredefined.TextualRequirement ||
                   itemTypePredefined == ItemTypePredefined.UIMockup ||
                   itemTypePredefined == ItemTypePredefined.UseCase ||
                   itemTypePredefined == ItemTypePredefined.UseCaseDiagram ||
                   IsSubArtifactType(itemTypePredefined);
        }

        public static ProjectSection GetProjectSection(this ItemTypePredefined itemTypePredefined)
        {
            if (itemTypePredefined.IsRegularArtifactType())
                return ProjectSection.Artifacts;
            if (itemTypePredefined.IsBaselinesAndReviewsGroupType())
                return ProjectSection.BaselinesAndReviews;
            if (itemTypePredefined.IsCollectionsGroupType())
                return ProjectSection.Collections;

            return ProjectSection.Unknown;
        }
    }
}
