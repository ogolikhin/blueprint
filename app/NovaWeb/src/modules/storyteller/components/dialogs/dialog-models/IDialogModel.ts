module Storyteller {
    export interface IDialogModel {
        subArtifactId: number;
        clonedUserTask: UserTask;
        originalUserTask: UserTask;
        propertiesMw: any; //TODO correct interface required
        isHistoricalVersion: boolean;
    }
}