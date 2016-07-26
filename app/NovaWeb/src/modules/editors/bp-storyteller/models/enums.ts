
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