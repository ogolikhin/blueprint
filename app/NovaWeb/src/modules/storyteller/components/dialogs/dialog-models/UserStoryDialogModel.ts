module Storyteller {

    export class UserStoryDialogModel implements IDialogModel {
        public clonedUserTask: UserTask;
        public originalUserTask: UserTask;
        public previousSytemTasks: SystemTask[];
        public nextSystemTasks: SystemTask[];
        public subArtifactId: number;
        public isUserSystemProcess: boolean;
        public propertiesMw: any; //TODO correct interface required! 
        public isReadonly: boolean;
        public isHistoricalVersion: boolean;
    }
}