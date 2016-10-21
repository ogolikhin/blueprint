import {IDialogSettings, IDialogService} from "../../../../../shared";
import {ILocalizationService} from "../../../../../core";
import {BaseModalDialogController, IModalScope} from "../base-modal-dialog-controller";
import {SubArtifactSystemTaskDialogModel} from "../models/sub-artifact-dialog-model";
import {IArtifactReference, ArtifactReference} from "../../../models/process-models";
import {ICommunicationManager} from "../../../services/communication-manager";
import {TaskModalController} from "./task-modal-controller";

export class SystemTaskModalController extends TaskModalController<SubArtifactSystemTaskDialogModel> {
    
    private systemNamePlaceHolderText: string;
    
    constructor(
                $scope: IModalScope,
                communicationManager: ICommunicationManager,
                $rootScope: ng.IRootScopeService,
                $q: ng.IQService,
                $timeout: ng.ITimeoutService,
                $sce: ng.ISCEService,
                dialogService: IDialogService,
                localization: ILocalizationService) {

        super($scope, communicationManager, $rootScope, $q, $timeout, $sce, dialogService, localization);

        this.systemNameOnBlur();
    }

    public systemNameOnFocus = () => {
        this.systemNamePlaceHolderText = this.localization.get("ST_System_Task_Name_Label");
    }

    public systemNameOnBlur = () => {
        if (this.dialogModel.clonedItem) {
            if (this.dialogModel.clonedItem.action) {
                this.systemNameOnFocus();
            } else {
                this.systemNamePlaceHolderText =
                    this.localization.get("ST_System_Task_Name_Label") + " " + this.dialogModel.clonedItem.label;
            }
        }
    }

    public getActiveHeader(): string {
        if (this.dialogModel.isSystemTask) {
            return this.dialogModel.clonedItem.label;
        }

        return null;
    }

    protected  getAssociatedArtifact(): IArtifactReference {
        return this.dialogModel.clonedItem.associatedArtifact;
    }

    protected setAssociatedArtifact(value: IArtifactReference) {
        this.dialogModel.clonedItem.associatedArtifact = value;
    }

    protected populateTaskChanges() {
        
        if (this.dialogModel.originalItem && this.dialogModel.clonedItem) {
            this.dialogModel.originalItem.persona = this.dialogModel.clonedItem.persona;
            this.dialogModel.originalItem.action = this.dialogModel.clonedItem.action;
            this.dialogModel.originalItem.imageId = this.dialogModel.clonedItem.imageId;        
            this.dialogModel.originalItem.associatedImageUrl = this.dialogModel.clonedItem.associatedImageUrl;        
            this.dialogModel.originalItem.associatedArtifact = this.dialogModel.clonedItem.associatedArtifact;
        }        
    }   
}
