module Storyteller {
    export interface IUserStoryProperties {
        nfr: IArtifactProperty;
        businessRules: IArtifactProperty;
    }

    export interface IUserTask extends ITask {        
        objective: string;
        userStoryProperties: IUserStoryProperties;
        getNextSystemTasks(graph: ProcessGraph): SystemTask[];
    }
}