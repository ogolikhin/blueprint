import {IModalDialogCommunication, ModalDialogCommunication} from "../../editors/bp-process/components/modal-dialogs/modal-dialog-communication";
import {IToolbarCommunication, ToolbarCommunication} from "../components/bp-toolbar/toolbar-communication";
import {IProcessDiagramCommunication, ProcessDiagramCommunication} from "../../editors/bp-process/components/diagram/process-diagram-communication";

export class ICommunicationManager {
    modalDialogManager: IModalDialogCommunication;
    toolbarCommunicationManager: IToolbarCommunication;
    processDiagramCommunication: IProcessDiagramCommunication;
}

export class CommunicationManager implements ICommunicationManager {
    public modalDialogManager: IModalDialogCommunication;
    public toolbarCommunicationManager: IToolbarCommunication;
    public processDiagramCommunication: IProcessDiagramCommunication;

    constructor () {
        this.modalDialogManager = new ModalDialogCommunication();
        this.toolbarCommunicationManager = new ToolbarCommunication();
        this.processDiagramCommunication = new ProcessDiagramCommunication();
    }
}