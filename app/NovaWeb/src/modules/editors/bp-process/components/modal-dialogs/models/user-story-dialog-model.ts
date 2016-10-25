import { UserTask, SystemTask } from "../../diagram/presentation/graph/shapes/";
import { UserStoryProperties } from "../../diagram/presentation/graph/shapes/user-task";
import { IModalDialogModel } from "./modal-dialog-model-interface";

export class UserStoryDialogModel implements IModalDialogModel {
    public originalUserTask: UserTask;
    public previousSystemTasks: SystemTask[];
    public nextSystemTasks: SystemTask[];
    public artifactId: number;
    public subArtifactId: number;
    public isUserSystemProcess: boolean;
    public propertiesMw: any; //TODO correct interface required! 
    public isReadonly: boolean;
    public isHistoricalVersion: boolean;

    public get userStoryId(): number {
        return this.originalUserTask.userStoryId;
    }
    public get userTaskLabel(): string {
        return this.originalUserTask.label;
    }
    public get userTaskAction(): string {
        return this.originalUserTask.action;
    }
}
