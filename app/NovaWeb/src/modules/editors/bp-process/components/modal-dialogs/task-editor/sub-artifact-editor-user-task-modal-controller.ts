import {BaseModalDialogController, IModalScope} from "../base-modal-dialog-controller";
import {SubArtifactUserTaskDialogModel} from "../models/sub-artifact-dialog-model";
import {IArtifactReference} from "../../../models/process-models";
import {IModalProcessViewModel} from "../models/modal-process-view-model";
import {ICommunicationManager} from "../../../services/communication-manager";

export class SubArtifactEditorUserTaskModalController extends BaseModalDialogController<SubArtifactUserTaskDialogModel> {
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
    
    private searchIncludesDelay: ng.IPromise<any>;
    private modalProcessViewModel: IModalProcessViewModel;

    public static $inject = [
        "$scope",
        
        
        "communicationManager",
        //"artifactSearchService",
        "$rootScope",
        "$q",
        "$timeout",
        "$sce"
    ];

    constructor($scope: IModalScope,
                
                private communicationManager: ICommunicationManager,
                // TODO look at this later
                //private artifactSearchService: Shell.IArtifactSearchService,
                $rootScope: ng.IRootScopeService,
                private $q: ng.IQService,
                private $timeout: ng.ITimeoutService,
                private $sce: ng.ISCEService) {

        super($rootScope, $scope);

        this.isReadonly = this.dialogModel.isReadonly || this.dialogModel.isHistoricalVersion;

        let isSMBVal = $rootScope["config"].settings.StorytellerIsSMB;

        if (isSMBVal.toLowerCase() === "true") {
            this.isSMB = true;
        }

        this.actionOnBlur();
        

        this.communicationManager.modalDialogManager.setModalProcessViewModel(this.setModalProcessViewModel);        
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

    private actionOnFocus = () => {
        this.actionPlaceHolderText = (<any>this.$rootScope).config.labels["ST_User_Task_Name_Label"];
    }

    private actionOnBlur = () => {
        if (this.dialogModel.clonedItem) {
            if (this.dialogModel.clonedItem.action) {
                this.actionOnFocus();
            } else {
                this.actionPlaceHolderText = (<any>this.$rootScope).config.labels["ST_User_Task_Name_Label"] + " " + this.dialogModel.clonedItem.label;
            }
        }
    }

    public saveData() {
        if (this.dialogModel.clonedItem.associatedArtifact === undefined) {
            this.dialogModel.clonedItem.associatedArtifact = null;
        }
        this.populateUserTaskChanges();                
    }

    private populateUserTaskChanges() {

        if (this.dialogModel.originalItem && this.dialogModel.clonedItem) {
            this.dialogModel.originalItem.persona = this.dialogModel.clonedItem.persona;
            this.dialogModel.originalItem.action = this.dialogModel.clonedItem.action;            
            this.dialogModel.originalItem.objective = this.dialogModel.clonedItem.objective;            
            this.dialogModel.originalItem.associatedArtifact = this.dialogModel.clonedItem.associatedArtifact;
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

        if (this.dialogModel.isUserTask) {
            return this.dialogModel.clonedItem.label;
        }

        return null;
    }
}
