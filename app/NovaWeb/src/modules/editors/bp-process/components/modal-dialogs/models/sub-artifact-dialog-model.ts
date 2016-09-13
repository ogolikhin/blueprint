import {IProcessShape} from "../../diagram/presentation/graph/models/";
import {IModalDialogModel} from "./modal-dialog-model-interface";
import {UserTask, SystemTask} from "../../diagram/presentation/graph/shapes/";

export class SubArtifactDialogModel implements IModalDialogModel {
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
