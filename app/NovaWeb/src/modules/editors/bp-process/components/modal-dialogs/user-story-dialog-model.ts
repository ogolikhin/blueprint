import {IModalDialogModel} from "./modal-dialog-model";

export class UserStoryDialogModel implements IModalDialogModel {
    // TODO: replace definitions:
    // public clonedUserTask: UserTask;
    // public originalUserTask: UserTask;
    // public previousSytemTasks: SystemTask[];
    // public nextSystemTasks: SystemTask[];
    public clonedUserTask: any;
    public originalUserTask: any;
    public previousSytemTasks: any[];
    public nextSystemTasks: any[];
    public subArtifactId: number;
    public isUserSystemProcess: boolean;
    public propertiesMw: any; //TODO correct interface required! 
    public isReadonly: boolean;
    public isHistoricalVersion: boolean;
}