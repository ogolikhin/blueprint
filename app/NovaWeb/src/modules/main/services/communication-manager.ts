import {IModalDialogManager, ModalDialogManager} from "../../editors/bp-process/components/modal-dialogs/modal-dialog-manager";
import {IToolbarCommunicationManager, ToolbarCommunicationManager} from "../components/bp-toolbar/toolbar-communication-manager";
import {IProcessDiagramCommunication, ProcessDiagramCommunication} from "../../editors/bp-process/components/diagram/process-diagram-communication";

export class ICommunicationManager {
    modalDialogManager: IModalDialogManager;
    toolbarCommunicationManager: IToolbarCommunicationManager;
    processDiagramCommunication: IProcessDiagramCommunication;
}

export class CommunicationManager implements ICommunicationManager {
    public modalDialogManager: IModalDialogManager;
    public toolbarCommunicationManager: IToolbarCommunicationManager;
    public processDiagramCommunication: IProcessDiagramCommunication;

    constructor () {
        this.modalDialogManager = new ModalDialogManager();
        this.toolbarCommunicationManager = new ToolbarCommunicationManager();
        this.processDiagramCommunication = new ProcessDiagramCommunication();
    }
}