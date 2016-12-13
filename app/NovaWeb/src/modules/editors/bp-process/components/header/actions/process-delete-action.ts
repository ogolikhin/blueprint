import {IProcessDiagramCommunication} from "./../../diagram/process-diagram-communication";
import {IDiagramNode} from "./../../diagram/presentation/graph/models/process-graph-interfaces";
import {NodeType} from "./../../diagram/presentation/graph/models/process-graph-constants";
import {INavigationService} from "./../../../../../core/navigation/navigation.svc";
import {IDialogService} from "./../../../../../shared/widgets/bp-dialog/bp-dialog";
import {ILoadingOverlayService} from "./../../../../../core/loading-overlay/loading-overlay.svc";
import {IProjectManager} from "./../../../../../managers/project-manager/project-manager";
import {IArtifactManager} from "./../../../../../managers/artifact-manager/artifact-manager";
import {IMessageService} from "./../../../../../core/messages/message.svc";
import {ILocalizationService} from "./../../../../../core/localization/localizationService";
import {IStatefulProcessArtifact} from "./../../../process-artifact";
import {StatefulProcessSubArtifact} from "./../../../process-subartifact";
import {DeleteAction} from "./../../../../../main/components/bp-artifact-info/actions/delete-action";
import {ProcessEvents} from "../../diagram/process-diagram-communication";
import {ItemTypePredefined, RolePermissions, ReuseSettings} from "../../../../../main/models/enums";

export class ProcessDeleteAction extends DeleteAction {
    private selectionChangedHandle: string;
    private selectedNodes: IDiagramNode[];
    
    constructor(
        private process: IStatefulProcessArtifact,
        localization: ILocalizationService,
        messageService: IMessageService,
        artifactManager: IArtifactManager,
        projectManager: IProjectManager,
        loadingOverlayService: ILoadingOverlayService,
        dialogService: IDialogService,
        navigationService: INavigationService,
        private communication: IProcessDiagramCommunication
    ) {
        super(process, localization, messageService, artifactManager, projectManager, loadingOverlayService, dialogService, navigationService);
    
        if (!communication) {
            throw new Error("Process diagram communication is not provided or is null");
        }

        this.selectionChangedHandle = this.communication.register(ProcessEvents.SelectionChanged, this.onSelectionChanged);
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

        if (this.selectedNodes.length > 1) {
            return false;
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
        
        if (validNodeTypes.indexOf(selectedNode.getNodeType()) < 0) {
            return false;
        }

        return true;
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
