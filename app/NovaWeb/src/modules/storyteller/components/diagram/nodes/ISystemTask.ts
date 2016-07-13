module Storyteller {
    export interface ISystemTask extends ITask {
        associatedImageUrl: string;
        imageId: string;
        getUserTask(graph: ProcessGraph): IUserTask;
    }
}
