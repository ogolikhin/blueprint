import {IProcessDiagramCommunication} from "../../diagram/process-diagram-communication";
import {IDiagramNode} from "../../diagram/presentation/graph/models/process-graph-interfaces";
import {NodeType} from "../../diagram/presentation/graph/models/process-graph-constants";
import {INavigationService} from "../../../../../commonModule/navigation/navigation.service";
import {IDialogService} from "../../../../../shared/widgets/bp-dialog/bp-dialog";
import {ILoadingOverlayService} from "../../../../../commonModule/loadingOverlay/loadingOverlay.service";
import {ILocalizationService} from "../../../../../commonModule/localization/localization.service";
import {IStatefulProcessArtifact} from "../../../process-artifact";
import {DeleteAction} from "../../../../../main/components/bp-artifact-info/actions/delete-action";
import {ProcessEvents} from "../../diagram/process-diagram-communication";
import {RolePermissions, ReuseSettings} from "../../../../../main/models/enums";
import {IMessageService} from "../../../../../main/components/messages/message.svc";
import {ISelectionManager} from "../../../../../managers/selection-manager/selection-manager";
import {IProjectExplorerService} from "../../../../../main/components/bp-explorer/project-explorer.service";

export class ProcessDeleteAction extends DeleteAction {
    private selectionChangedHandle: string;
    private selectedNodes: IDiagramNode[];
    private _tooltip: string;

    constructor(
        private process: IStatefulProcessArtifact,
        localization: ILocalizationService,
        messageService: IMessageService,
        selectionManager: ISelectionManager,
        projectExplorerService: IProjectExplorerService,
        loadingOverlayService: ILoadingOverlayService,
        dialogService: IDialogService,
        navigationService: INavigationService,
        private communication: IProcessDiagramCommunication
    ) {
        super(process, localization, messageService, projectExplorerService, loadingOverlayService, dialogService, navigationService);

        if (!this.communication) {
            throw new Error("Process diagram communication is not provided or is null");
        }

        this._tooltip = this.localization.get("App_Toolbar_Delete");
        this.selectionChangedHandle = this.communication.register(ProcessEvents.SelectionChanged, this.onSelectionChanged);
    }

    public get tooltip(): string {
        return this._tooltip;
    }

    public dispose(): void {
        this.communication.unregister(ProcessEvents.SelectionChanged, this.selectionChangedHandle);
    }

    protected canDelete(): boolean {
        if (!super.canDelete()) {
            return false;
        }

        //Is artifact and has Delete permissions
        if (this.isArtifactSelected()) {
            return this.hasDesiredPermissions(RolePermissions.Delete);
        }

        const selectedNode: IDiagramNode = this.selectedNodes[0];

        //Subartifact is selected and selective readonly is set
        if (this.process.isReuseSettingSRO && this.process.isReuseSettingSRO(ReuseSettings.Subartifacts)) {
            return false;
        }

        const validNodeTypes: NodeType[] = [
            NodeType.UserTask,
            NodeType.UserDecision,
            NodeType.SystemDecision
        ];

        return validNodeTypes.indexOf(selectedNode.getNodeType()) >= 0;


    }

    protected hasRequiredPermissions(): boolean {
        return this.hasDesiredPermissions(RolePermissions.Edit);
    }

    protected delete(): void {
        if (!this.canDelete()) {
            return;
        }

        if (this.isArtifactSelected()) {
            super.delete();
        } else {
            this.communication.action(ProcessEvents.DeleteShape, this.selectedNodes[0]);
        }
    }

    private isArtifactSelected(): boolean {
        return !this.selectedNodes || !this.selectedNodes.length;
    }

    private onSelectionChanged = (nodes: IDiagramNode[]): void => {
        this.selectedNodes = nodes;

        if (this.selectedNodes && this.selectedNodes.length === 1) {
            this._tooltip = this.localization.get("ST_Shapes_Delete_Tooltip");
        } else {
            this._tooltip = this.localization.get("App_Toolbar_Delete");
        }
    };
}
