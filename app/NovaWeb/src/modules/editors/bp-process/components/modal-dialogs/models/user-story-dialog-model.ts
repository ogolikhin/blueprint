import {UserTask, SystemTask} from "../../diagram/presentation/graph/shapes/";
import {IModalDialogModel} from "./modal-dialog-model-interface";

export class UserStoryDialogModel implements IModalDialogModel {
    public clonedUserTask: UserTask;
    public originalUserTask: UserTask;
    public previousSytemTasks: SystemTask[];
    public nextSystemTasks: SystemTask[];
    public artifactId: number;
    public subArtifactId: number;
    public isUserSystemProcess: boolean;
    public propertiesMw: any; //TODO correct interface required! 
    public isReadonly: boolean;
    public isHistoricalVersion: boolean;
}
