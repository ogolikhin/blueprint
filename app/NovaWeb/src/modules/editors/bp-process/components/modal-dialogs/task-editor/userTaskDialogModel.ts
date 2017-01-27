import {UserTask} from "../../diagram/presentation/graph/shapes/user-task";
import {IPersonaOption, TaskDialogModel} from "./taskDialogModel";

export class UserTaskDialogModel extends TaskDialogModel {
    originalItem: UserTask;
    objective: string;
    userTaskPersonaReferenceOptions: IPersonaOption[];
}
