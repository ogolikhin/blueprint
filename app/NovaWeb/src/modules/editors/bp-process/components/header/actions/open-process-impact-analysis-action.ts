import {OpenImpactAnalysisAction} from "../../../../../main/components/bp-artifact-info/actions/open-impact-analysis-action";
import {IStatefulProcessArtifact} from "../../../process-artifact";
import {ILocalizationService} from "../../../../../core/localization/localizationService";
import {IProcessDiagramCommunication, ProcessEvents} from "../../diagram/process-diagram-communication";
import {IDiagramNode} from "./../../diagram/presentation/graph/models/process-graph-interfaces";

export class OpenProcessImpactAnalysisAction extends OpenImpactAnalysisAction {
    private selectionChangedHandle: string;
    private selectedNodes: IDiagramNode[];

    constructor(
        process: IStatefulProcessArtifact,
        localization: ILocalizationService,
        private communication: IProcessDiagramCommunication
    ) {
        super(process, localization);

        if (!communication) {
            throw new Error("Process diagram communication is not provided or is null");
        }

        this.selectionChangedHandle = communication.register(ProcessEvents.SelectionChanged, this.onSelectionChanged);
    }

    public dispose(): void {
        this.communication.unregister(ProcessEvents.SelectionChanged, this.selectionChangedHandle);
    }

    protected canOpenImpactAnalysis(): boolean {
        if (!super.canOpenImpactAnalysis()) {
            return false;
        }

        return !this.selectedNodes || this.selectedNodes.length <= 1;
    }

    protected openImpactAnalysis(): void {
        if (!this.selectedNodes || !this.selectedNodes.length) {
            super.openImpactAnalysis();
        } else {
            const subArtifactId: number = this.selectedNodes[0].model.id;
            this.openImpactAnalysisInternal(subArtifactId);
        }
    }

    private onSelectionChanged = (nodes: IDiagramNode[]): void => {
        this.selectedNodes = nodes;

        if (this.selectedNodes && this.selectedNodes.length === 1) {
            this._tooltip = this.localization.get("App_Toolbar_Open_Shape_Impact_Analysis");
        } else {
            this._tooltip = this.localization.get("App_Toolbar_Open_Impact_Analysis");
        }
    };
}