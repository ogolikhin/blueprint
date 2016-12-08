import {IDecision, NodeType} from "../models/";
import {IDialogParams} from "../../../../messages/message-dialog";
import {Decision} from "./decision";


export class UserDecision extends Decision implements IDecision {

    protected get DECISION_SHIFT(): number {
        return 33;
    }

    protected get DEFAULT_FILL_COLOR(): string {
        return "#FFFFFF";
    }

    protected get DEFAULT_BORDER_COLOR(): string {
        return "#D4D5DA";
    }

    protected get HIGHLIGHT_BORDER_COLOR(): string {
        return "#FF0000";
    }

    public getX(): number {
        return this.getCenter().x + this.DECISION_SHIFT;
    }

    protected get textLabelLeft(): number {
        return this.DECISION_WIDTH / 2 + 24;
    }

    protected get textLabelWidth(): number {
        return this.DECISION_WIDTH - 20;
    }

    public getDeleteDialogParameters(): IDialogParams {
        let dialogParams: IDialogParams = {};
        dialogParams.message = this.rootScope["config"].labels["ST_Confirm_Delete_User_Decision"];
        return dialogParams;
    }

    public getNodeType() {
        return NodeType.UserDecision;
    }
}
