import {BaseModalDialogController, IModalScope} from "./base-modal-dialog-controller";
import {SubArtifactDialogModel} from "./models/sub-artifact-dialog-model";
import {IArtifactReference} from "../../models/process-models";
import {IModalProcessViewModel} from "./models/modal-process-view-model";
import {ICommunicationManager} from "../../services/communication-manager";

export class SubArtifactEditorSystemTaskModalController extends BaseModalDialogController<SubArtifactDialogModel> {
    public getLinkableProcesses: (viewValue: string) => ng.IPromise<IArtifactReference[]>;
    public getLinkableArtifacts: (viewValue: string) => ng.IPromise<IArtifactReference[]>;
    public isProjectOnlySearch: boolean = true;
    public isLoadingIncludes: boolean = false;

    private isIncludeNoResults: boolean = false;
    private isIncludeBadRequest: boolean = false;
    private isIncludeResultsVisible: boolean;
    private isReadonly: boolean = false;
    private isSMB: boolean = false;
    private actionPlaceHolderText: string;
    private systemNamePlaceHolderText: string;
    private searchIncludesDelay: ng.IPromise<any>;
    private modalProcessViewModel: IModalProcessViewModel;

    public static $inject = [
        "$scope",
        "$uibModalInstance",
        "dialogModel",
        "communicationManager",
        //"artifactSearchService",
        "$rootScope",
        "$q",
        "$timeout",
        "$sce"
    ];

    constructor($scope: IModalScope,
                $uibModalInstance: angular.ui.bootstrap.IModalServiceInstance,
                dialogModel: SubArtifactDialogModel,
                private communicationManager: ICommunicationManager,
                // TODO look at this later
                //private artifactSearchService: Shell.IArtifactSearchService,
                $rootScope: ng.IRootScopeService,
                private $q: ng.IQService,
                private $timeout: ng.ITimeoutService,
                private $sce: ng.ISCEService) {

        super($rootScope, $scope, $uibModalInstance, dialogModel);

        this.isReadonly = this.dialogModel.isReadonly;

        let isSMBVal = $rootScope["config"].settings.StorytellerIsSMB;

        if (isSMBVal.toLowerCase() === "true") {
            this.isSMB = true;
        }

        this.systemNameOnBlur();

        this.communicationManager.modalDialogManager.setModalProcessViewModel(this.setModalProcessViewModel);
        this.setNextNode(this.modalProcessViewModel);
    }

    private setModalProcessViewModel = (modalProcessViewModel) => {
        this.modalProcessViewModel = modalProcessViewModel;
    }

    public prepIncludeField(): void {
        this.isIncludeResultsVisible = true;
        this.clearFields();
    }

    public cleanIncludeField(): void {
        this.isIncludeResultsVisible = false;
        this.clearFields();
    }

    public changeIncludeField(): void {
        this.clearFields();
    }

    private cancelIncludeSearchTimer(): void {
        this.$timeout.cancel(this.searchIncludesDelay);
        this.isLoadingIncludes = false;
    }

    private clearFields() {
        this.cancelIncludeSearchTimer();
        this.isIncludeBadRequest = false;
        this.isIncludeNoResults = false;
    }

    public formatIncludeLabel(model) {
        if (!model) {
            return "";
        }

        let msg: string;
        if (model.typePrefix === "<Inaccessible>") {
            msg = this.$rootScope["config"].labels["HttpError_Forbidden"];
        } else {
            msg = model.typePrefix + model.id + ": " + model.name;
        }

        return msg;
    }

    public setNextNode(modalProcessViewModel: IModalProcessViewModel) {
        this.dialogModel.nextNode = modalProcessViewModel.getNextNode(this.dialogModel.clonedSystemTask.model);
    }

    public sortById(p1: IArtifactReference, p2: IArtifactReference) {
        return p1.id - p2.id;
    }

    public filterByDisplayLabel(process: IArtifactReference, viewValue: string): boolean {
        //exlude current process
        if (process.id === this.modalProcessViewModel.processViewModel.id) {
            return false;
        }

        //show all if viewValue is null/'underfined' or empty string
        if (!viewValue) {
            return true;
        }

        if ((`${process.typePrefix}${process.id}: ${process.name}`).toLowerCase().indexOf(viewValue.toLowerCase()) > -1) {
            return true;
        }

        return false;
    }

    private systemNameOnFocus = () => {
        this.systemNamePlaceHolderText = (<any>this.$rootScope).config.labels["ST_System_Task_Name_Label"];
    }

    private systemNameOnBlur = () => {
        if (this.dialogModel.clonedSystemTask) {
            if (this.dialogModel.clonedSystemTask.action) {
                this.systemNameOnFocus();
            } else {
                this.systemNamePlaceHolderText =
                    (<any>this.$rootScope).config.labels["ST_System_Task_Name_Label"] + " " + this.dialogModel.clonedSystemTask.label;
            }
        }
    }

    public saveData() {

        if (this.dialogModel.clonedSystemTask.associatedArtifact === undefined) {
            this.dialogModel.clonedSystemTask.associatedArtifact = null;
        }

        this.populateSystemTaskChanges();
    }

    private populateSystemTaskChanges() {

        if (this.dialogModel.originalSystemTask && this.dialogModel.clonedSystemTask) {
            this.dialogModel.originalSystemTask.persona = this.dialogModel.clonedSystemTask.persona;
            this.dialogModel.originalSystemTask.action = this.dialogModel.clonedSystemTask.action;
            this.dialogModel.originalSystemTask.imageId = this.dialogModel.clonedSystemTask.imageId;
            this.dialogModel.originalSystemTask.associatedImageUrl = this.dialogModel.clonedSystemTask.associatedImageUrl;
            this.dialogModel.originalSystemTask.associatedArtifact = this.dialogModel.clonedSystemTask.associatedArtifact;
        }
    }

    private refreshView() {
        let element: HTMLElement = document.getElementsByClassName("modal-dialog")[0].parentElement;

        // temporary solution from: http://stackoverflow.com/questions/8840580/force-dom-redraw-refresh-on-chrome-mac
        if (!element) {
            return;
        }

        let node = document.createTextNode(" ");
        element.appendChild(node);
//fixme: use the $timeout services not setTimeout
        setTimeout(function () {
            node.parentNode.removeChild(node);
        }, 20);
    }

    public getActiveHeader(): string {
        if (this.dialogModel.isSystemTask) {
            return this.dialogModel.clonedSystemTask.label;
        }

        return null;
    }
}
