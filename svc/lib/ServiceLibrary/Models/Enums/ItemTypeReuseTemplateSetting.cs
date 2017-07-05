using System;

namespace ServiceLibrary.Models.Enums
{
    [Flags]
    public enum ItemTypeReuseTemplateSetting
    {
        None = 0x0,
        Name = 0x1,
        Description = 0x2,
        ActorImage = 0x4,
        BaseActor = 0x8,
        DocumentFile = 0x10,
        DiagramHeight = 0x20,
        DiagramWidth = 0x40,
        UseCaseLevel = 0x80,
        UIMockupTheme = 0x100,
        UseCaseDiagramShowConditions = 0x200,
        Attachments = 0x400,
        DocumentReferences = 0x800,
        Relationships = 0x1000,
        Subartifacts = 0x2000,
        All = Name | Description | ActorImage | BaseActor | DocumentFile | DiagramHeight | DiagramWidth | UseCaseLevel | UIMockupTheme 
            | UseCaseDiagramShowConditions | Attachments | DocumentReferences | Relationships | Subartifacts
    }
}
