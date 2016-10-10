using System.Collections.Generic;

namespace Model.ArtifactModel.Enums
{
    public static class BaseArtifactTypeExtensions
    {
        /// <summary>
        /// This is a map of BaseArtifactType enum values to ItemTypePredefined enum values.
        /// </summary>
        internal static Dictionary<BaseArtifactType, ItemTypePredefined> BaseArtifactTypeToItemTypePredefinedMap { get; } =
            new Dictionary<BaseArtifactType, ItemTypePredefined>
        {
            {BaseArtifactType.Undefined,                ItemTypePredefined.None},
            {BaseArtifactType.PrimitiveFolder,          ItemTypePredefined.PrimitiveFolder},
            {BaseArtifactType.Glossary,                 ItemTypePredefined.Glossary},
            {BaseArtifactType.TextualRequirement,       ItemTypePredefined.TextualRequirement},
            {BaseArtifactType.BusinessProcess,          ItemTypePredefined.BusinessProcess},
            {BaseArtifactType.Actor,                    ItemTypePredefined.Actor},
            {BaseArtifactType.UseCase,                  ItemTypePredefined.UseCase},
            {BaseArtifactType.DataElement,              ItemTypePredefined.DataElement},
            {BaseArtifactType.UIMockup,                 ItemTypePredefined.UIMockup},
            {BaseArtifactType.GenericDiagram,           ItemTypePredefined.GenericDiagram},
            {BaseArtifactType.Document,                 ItemTypePredefined.Document},
            {BaseArtifactType.Storyboard,               ItemTypePredefined.Storyboard},
            {BaseArtifactType.DomainDiagram,            ItemTypePredefined.DomainDiagram},
            {BaseArtifactType.UseCaseDiagram,           ItemTypePredefined.UseCaseDiagram},
            {BaseArtifactType.Baseline,                 ItemTypePredefined.Baseline},
            {BaseArtifactType.BaselineFolder,           ItemTypePredefined.BaselineFolder},
            {BaseArtifactType.ArtifactBaseline,         ItemTypePredefined.ArtifactBaseline},
            {BaseArtifactType.ArtifactReviewPackage,    ItemTypePredefined.ArtifactReviewPackage},
            {BaseArtifactType.Process,                  ItemTypePredefined.Process}
        };

        /// <summary>
        /// Converts this BaseArtifactType enum value to its ItemTypePredefined equivalent.
        /// </summary>
        /// <param name="baseArtifactType">The BaseArtifactType to convert.</param>
        /// <returns>The ItemTypePredefined version of this BaseArtifactType.</returns>
        public static ItemTypePredefined ToItemTypePredefined(this BaseArtifactType baseArtifactType)
        {
            return BaseArtifactTypeToItemTypePredefinedMap[baseArtifactType];
        }
    }
}
