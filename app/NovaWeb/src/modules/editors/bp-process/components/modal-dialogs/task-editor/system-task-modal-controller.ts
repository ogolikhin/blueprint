import {IDialogSettings, IDialogService} from "../../../../../shared";
import {ILocalizationService} from "../../../../../core";
import {BaseModalDialogController, IModalScope} from "../base-modal-dialog-controller";
import {SystemTaskDialogModel} from "./sub-artifact-dialog-model";
import {IArtifactReference, ArtifactReference} from "../../../models/process-models";
import {ICommunicationManager} from "../../../services/communication-manager";
import {TaskModalController} from "./task-modal-controller";

export class SystemTaskModalController extends TaskModalController<SystemTaskDialogModel> {
    private systemNamePlaceHolderText: string;
    
    constructor(
        $scope: IModalScope,
        $rootScope: ng.IRootScopeService,
        $timeout: ng.ITimeoutService,
        dialogService: IDialogService,
        localization: ILocalizationService,
        $uibModalInstance?: ng.ui.bootstrap.IModalServiceInstance,
        dialogModel?: SystemTaskDialogModel
    ) {
        super($scope, $rootScope, $timeout, dialogService, localization, $uibModalInstance, dialogModel);
    }

    public nameOnFocus() {
        this.systemNamePlaceHolderText = this.localization.get("ST_System_Task_Name_Label");
    }

    public nameOnBlur() {
        if (this.dialogModel) {
            if (this.dialogModel.action) {
                this.nameOnFocus(); 
            } else {
                this.systemNamePlaceHolderText = `${this.localization.get("ST_System_Task_Name_Label")} ${this.dialogModel.label}`;
            }
        }
    }

    public getActiveHeader(): string {
        return this.dialogModel.label;
    }

    protected  getAssociatedArtifact(): IArtifactReference {
        return this.dialogModel.associatedArtifact;
    }

    protected setAssociatedArtifact(value: IArtifactReference) {
        this.dialogModel.associatedArtifact = value;
    }

    protected populateTaskChanges() {
        if (this.dialogModel.originalItem && this.dialogModel) {
            this.dialogModel.originalItem.persona = this.dialogModel.persona;
            this.dialogModel.originalItem.action = this.dialogModel.action;
            this.dialogModel.originalItem.imageId = this.dialogModel.imageId;
            this.dialogModel.originalItem.associatedImageUrl = this.dialogModel.associatedImageUrl;
            this.dialogModel.originalItem.associatedArtifact = this.dialogModel.associatedArtifact;
        }
    }
}
