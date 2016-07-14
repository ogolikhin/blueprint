module Storyteller {
    export class SubArtifactDialogModel implements IDialogModel {
        public clonedUserTask: UserTask;
        public originalUserTask: UserTask;
        public originalSystemTask: SystemTask;
        public clonedSystemTask: SystemTask;
        public isUserTask: boolean;
        public isSystemTask: boolean;
        public subArtifactId: number;
        public nextNode: IProcessShape;
        public propertiesMw: any; //TODO correct interface required! 
        public isReadonly: boolean;
        public tabClick: Function;
        public isHistoricalVersion: boolean;
    }
}