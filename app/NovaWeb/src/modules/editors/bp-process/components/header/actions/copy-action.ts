import {BPButtonAction} from "../../../../../shared";
import {ILocalizationService} from "../../../../../commonModule/localization/localization.service";
import {StatefulProcessArtifact} from "../../../process-artifact";
import {ICommunicationManager} from "../../../services/communication-manager";
import {ProcessEvents} from "../../diagram/process-diagram-communication";
import {IDiagramNode} from "../../diagram/presentation/graph/models/process-graph-interfaces";
import {NodeType} from "../../diagram/presentation/graph/models/process-graph-constants";

export class CopyAction extends BPButtonAction {
    private subscribers: Rx.IDisposable[];
    private hasValidSelection: boolean;
    private selectionChangedHandle: string;

    constructor(
        private process: StatefulProcessArtifact,
        private communicationManager: ICommunicationManager,
        private localization: ILocalizationService
    ) {
        super();

        if (!this.process) {
            throw new Error("Process is not provided or is null");
        }

        if (!this.communicationManager) {
            throw new Error("Communication manager is not provided or is null");
        }

        if (!this.localization) {
            throw new Error("Localization service is not provided or is null");
        }

        this.hasValidSelection = false;
        this.selectionChangedHandle = communicationManager.processDiagramCommunication.register(ProcessEvents.SelectionChanged, this.onSelectionChanged);
    }

    public get icon(): string {
        return "fonticon2-copy-shapes";
    }

    public get tooltip(): string {
        return this.localization.get("App_Toolbar_Copy_Shapes");
    }

    public get disabled(): boolean {
        return !this.process
            || this.process.artifactState.historical
            || !this.hasValidSelection;
    }

    public execute(): void {
        if (this.disabled) {
            return;
        }

        this.communicationManager.toolbarCommunicationManager.copySelection();
    }

    public dispose(): void {
        this.communicationManager.processDiagramCommunication.unregister(ProcessEvents.SelectionChanged, this.selectionChangedHandle);
    }

    private onSelectionChanged = (elements: IDiagramNode[]) => {
        this.hasValidSelection = elements && elements.length > 0 && _.every(elements, (element: IDiagramNode) => element.getNodeType() === NodeType.UserTask);
    };
}
