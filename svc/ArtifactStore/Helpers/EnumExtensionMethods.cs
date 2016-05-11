using System.Collections.Generic;
using ArtifactStore.Models;

namespace ArtifactStore.Helpers
{
    public static class EnumExtensionMethods
    {

        private static readonly Dictionary<int, PredefinedType> PredefinedTypeMap = new Dictionary<int, PredefinedType> 
        {
            { 0x1000 | 6, PredefinedType.Folder },
            { 0x1000 | 8, PredefinedType.Actor },
            { 0x1000 | 14, PredefinedType.Document },
            { 0x1000 | 16, PredefinedType.DomainDiagram },
            { 0x1000 | 12, PredefinedType.GenericDiagram },
            { 0x1000 | 3, PredefinedType.Glossary },
            { 0x1000 | 18, PredefinedType.Process },
            { 0x1000 | 15, PredefinedType.Storyboard },
            { 0x1000 | 5, PredefinedType.Requirement },
            { 0x1000 | 11, PredefinedType.UiMockup },
            { 0x1000 | 9, PredefinedType.UseCase },
            { 0x1000 | 17, PredefinedType.UseCaseDiagram },
            { 0x1000 | 0x100 | 1, PredefinedType.BaselineReviewFolder },
            { 0x1000 | 0x100 | 2, PredefinedType.Baleline },
            { 0x1000 | 0x100 | 3, PredefinedType.Review },
            { 0x1000 | 0x200 | 1, PredefinedType.CollectionFolder },
            { 0x1000 | 0x200 | 2, PredefinedType.Collection }
        };

        public static PredefinedType ToPredefinedType(this int type)
        {
            PredefinedType result;
            return PredefinedTypeMap.TryGetValue(type, out result) ? result : PredefinedType.Unknown;
        }
    }
}