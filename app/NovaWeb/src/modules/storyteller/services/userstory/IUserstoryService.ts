module Storyteller {
    export interface IUserstoryService {
        /**
        *  Generates user story artifacts for specific project and process
        */
        generateUserstories(projectId: number, processId: number, taskId?: number): ng.IPromise<IUserStory[]>;

        /**
        *  Returns user story id by specified user task id.
        */
        getUserStoryId(userTaskId: number): number;
    }
}
