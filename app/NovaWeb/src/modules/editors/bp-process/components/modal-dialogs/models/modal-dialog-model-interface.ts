import {UserTask} from "../../diagram/presentation/graph/shapes/";

export interface IModalDialogModel {
    subArtifactId: number;
    // clonedUserTask: UserTask;
    // originalUserTask: UserTask;
    //propertiesMw: any; //TODO correct interface required
    isHistoricalVersion: boolean;
    isReadonly: boolean;
}
