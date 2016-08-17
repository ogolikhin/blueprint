import {IModalDialogModel} from "./modal-dialog-model";

export class SubArtifactDialogModel implements IModalDialogModel {
    // TODO: replace definitions:
    // public clonedUserTask: UserTask;
    // public originalUserTask: UserTask;
    // public originalSystemTask: SystemTask;
    // public clonedSystemTask: SystemTask;
    public clonedUserTask: any;
    public originalUserTask: any;
    public originalSystemTask: any;
    public clonedSystemTask: any;
    public isUserTask: boolean;
    public isSystemTask: boolean;
    public subArtifactId: number;
    // TODO: replace definition:
    //public nextNode: IProcessShape;
    public nextNode: any;
    public propertiesMw: any; //TODO correct interface required! 
    public isReadonly: boolean;
    public tabClick: Function;
    public isHistoricalVersion: boolean;
}
