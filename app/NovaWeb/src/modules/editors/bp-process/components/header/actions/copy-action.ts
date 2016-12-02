import {BPButtonAction} from "../../../../../shared";
import {ILocalizationService} from "../../../../../core/localization/localizationService";
import {StatefulProcessArtifact} from "../../../process-artifact";
import {ICommunicationManager} from "../../../services/communication-manager";
import {ProcessType} from "../../../models/enums";
import {ProcessEvents} from "../../diagram/process-diagram-communication";
import {IDiagramNode} from "../../diagram/presentation/graph/models/process-graph-interfaces";
import {NodeType} from "../../diagram/presentation/graph/models/process-graph-constants";

export class CopyAction extends BPButtonAction {
    private subscribers: Rx.IDisposable[];
    private canCopy: boolean;
    private selectionChangedHandle: string;

    constructor(
        private process: StatefulProcessArtifact,
        private communicationManager: ICommunicationManager,
        private localization: ILocalizationService
    ) {
        super();

        if (!process) {
            throw new Error("Process is not provided or is null");
        }

        if (!communicationManager) {
            throw new Error("Communication manager is not provided or is null");
        }

        if (!localization) {
            throw new Error("Localization service is not provided or is null");
        }

        this.canCopy = false;
        this.selectionChangedHandle = communicationManager.processDiagramCommunication.register(ProcessEvents.SelectionChanged, this.onSelectionChanged);
    }

    public get icon(): string {
        return "fonticon2-copy-shapes";
    }

    public get tooltip(): string {
        return this.localization.get("App_Toolbar_Copy_Shapes");
    }

    public get disabled(): boolean {
        return !this.canCopy;
    }

    public get execute(): () => void {
        return this.copy;
    }

    public dispose(): void {
        if (this.communicationManager) {
            if (this.communicationManager.processDiagramCommunication) {
                this.communicationManager.processDiagramCommunication.unregister(ProcessEvents.SelectionChanged, this.selectionChangedHandle);
            }
        }
    }

    private copy(): void {
        if (this.disabled) {
            return;
        }

        this.communicationManager.toolbarCommunicationManager.copySelection();
    }

    private onSelectionChanged = (elements: IDiagramNode[]) => {
        this.canCopy = elements && elements.length > 0 && _.every(elements, (element: IDiagramNode) => element.getNodeType() === NodeType.UserTask);
    };
}
