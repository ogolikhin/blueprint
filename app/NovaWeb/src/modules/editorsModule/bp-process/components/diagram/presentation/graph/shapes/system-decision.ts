import {NodeType} from "../models/";
import {IDialogParams} from "../../../../messages/message-dialog";
import {Decision} from "./decision";

export class SystemDecision extends Decision {
    protected get DECISION_SHIFT(): number {
        return 0;
    }

    protected get DEFAULT_FILL_COLOR(): string {
        return "#E2F3FF";
    }

    protected get DEFAULT_BORDER_COLOR(): string {
        return "#53BBED";
    }

    protected get HIGHLIGHT_BORDER_COLOR(): string {
        return "#53BBED";
    }

    public showMenu(mxGraph: MxGraph) {
        super.showMenu(mxGraph);

        this.detailsButton.setVisible(true);
    }

    public getDeleteDialogParameters(): IDialogParams {
        let dialogParams: IDialogParams = {};
        dialogParams.message = this.rootScope["config"].labels["ST_Confirm_Delete_System_Decision"];
        return dialogParams;
    }

    public getNodeType() {
        return NodeType.SystemDecision;
    }

    protected get textLabelLeft(): number {
        return this.DECISION_WIDTH / 2 - 15;
    }

    protected get textLabelWidth(): number {
        return this.DECISION_WIDTH - 30;
    }
}