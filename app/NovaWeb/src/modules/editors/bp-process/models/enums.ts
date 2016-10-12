
export const enum ProcessType {
    None = 0,
    BusinessProcess = 1,
    UserToSystemProcess = 2,
    SystemToSystemProcess = 3
}

export const enum ProcessShapeType {
    None = 0,
    Start = 1,
    UserTask = 2,
    End = 3,
    SystemTask = 4,
    PreconditionSystemTask = 5,
    UserDecision = 6,
    SystemDecision = 7
}

export const enum PropertyType {
    PlainText = 0,
    RichText = 1,
    Number = 2,
    Date = 3,
    Choice = 4,
    User = 5
}
export const enum PropertyValueFormat {
    Text = 0,
    Html = 1,
    Date = 2,
    DateTimeUtc = 3
}
export const enum ItemIndicatorFlags {
    None = 0,
    HasComments = 1,
    HasAttachmentsOrDocumentRefs = 2,
    HasManualReuseOrOtherTraces = 4,
    HasLast24HoursChanges = 8
}

export const enum ArtifactUpdateType {
    SubArtifact = 0,
    LinkLabel = 1
}