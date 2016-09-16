import {BaseModalDialogController, IModalScope} from "./base-modal-dialog-controller";
import {SubArtifactDialogModel} from "./models/sub-artifact-dialog-model";
import {IArtifactReference} from "../../models/process-models";
import {IModalProcessViewModel} from "./models/modal-process-view-model";
import {ICommunicationManager} from "../../services/communication-manager";

export class SubArtifactEditorModalController extends BaseModalDialogController<SubArtifactDialogModel> {
    public getLinkableProcesses: (viewValue: string) => ng.IPromise<IArtifactReference[]>;
    public getLinkableArtifacts: (viewValue: string) => ng.IPromise<IArtifactReference[]>;
    private isShowMore: boolean = false;
    private showMoreActiveTabIndex: number = 0;
    private isIncludeNoResults: boolean = false;
    private isIncludeBadRequest: boolean = false;
    private isIncludeResultsVisible: boolean;
    private isReadonly: boolean = false;
    private isSMB: boolean = false;
    private actionPlaceHolderText: string;
    private systemNamePlaceHolderText: string;
    private isProjectOnlySearch: boolean = true;
    private searchIncludesDelay: ng.IPromise<any>;
    public isLoadingIncludes: boolean = false;
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

        this.actionOnBlur();
        this.systemNameOnBlur();

        this.communicationManager.modalDialogManager.setModalProcessViewModel(this.setModalProcessViewModel);
        //dialogModel.clonedUserTask.description = this.$sce.trustAsHtml(dialogModel.clonedUserTask.description);

        // Only get processes once per controller
        //let processesPromise: ng.IPromise<IArtifactReference[]>;
        //const getProcesses = () => processesPromise || (processesPromise = processModelService.getProcesses(processModelService.processViewModel.projectId));

        // this.getLinkableProcesses = (viewValue: string) => getProcesses()
        //     .then((processes: IArtifactReference[]) => {
        //         const filtered = processes.filter(p => this.filterByDisplayLabel(p, viewValue));
        //         return filtered.slice(0, 10).sort(this.sortById);
        //     });

        // TODO look at this later 
        // this.getLinkableArtifacts = (viewValue: string) => {
        //     this.clearFileds();

        //     const searchIncludesDefered = this.$q.defer<IArtifactSearchResultItem[]>();
        //     const projectId: string = this.isProjectOnlySearch ? this.processModelService.processModel.projectId.toString() : null;
        //     this.isLoadingIncludes = true;

        //     this.searchIncludesDelay = this.$timeout(() => {
        //         this.artifactSearchService.search(viewValue, projectId).then(
        //             (artifacts: IArtifactReference[]) => {
        //                 this.isLoadingIncludes = false;
        //                 if (artifacts instanceof Array) {
        //                     let artifactsWithoutCurrentProcess: IArtifactReference[] = artifacts.filter(
        //                         (artifact: IArtifactReference) => artifact.id !== this.processModelService.processModel.id
        //                     );

        //                     if (artifactsWithoutCurrentProcess.length === 0) {
        //                         this.isIncludeNoResults = true;
        //                     }
        //                     searchIncludesDefered.resolve(artifactsWithoutCurrentProcess);

        //                 } else {
        //                     this.isIncludeBadRequest = true;
        //                     searchIncludesDefered.resolve([]);
        //                 }
        //             },
        //             () => {
        //                 this.isLoadingIncludes = false;
        //                 searchIncludesDefered.resolve([]);
        //             }
        //         );
        //     }, 1000);
        //     return searchIncludesDefered.promise;
        // };

        this.setNextNode(this.modalProcessViewModel);
    }
    
    private getDescription() {
        return this.$sce.trustAsHtml(this.dialogModel.clonedUserTask.description);
    }

    private setModalProcessViewModel = (modalProcessViewModel) => {
        this.modalProcessViewModel = modalProcessViewModel;
    }

    private prepIncludeField(): void {
        this.isIncludeResultsVisible = true;
        this.clearFileds();
    }

    private cleanIncludeField(): void {
        this.isIncludeResultsVisible = false;
        this.clearFileds();
    }

    private changeIncludeField(): void {
        this.clearFileds();
    }

    private cancelIncludeSearchTimer(): void {
        this.$timeout.cancel(this.searchIncludesDelay);
        this.isLoadingIncludes = false;
    }

    private clearFileds() {
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
            msg = this.$rootScope["config"].labels["ST_Artifact_Inaccessible"];
        } else {
            msg = model.typePrefix + model.id + ": " + model.name;
        }

        return msg;
    }

    public setNextNode(modalProcessViewModel: IModalProcessViewModel)  {
        this.dialogModel.nextNode = modalProcessViewModel.getNextNode(this.dialogModel.clonedSystemTask.model);
    }

    private sortById(p1: IArtifactReference, p2: IArtifactReference) {
        return p1.id - p2.id;
    }

    private filterByDisplayLabel(process: IArtifactReference, viewValue: string): boolean {
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
        if (this.dialogModel.clonedUserTask) {
            if (this.dialogModel.clonedUserTask.action) {
                this.actionOnFocus();
            } else {
                this.actionPlaceHolderText = (<any>this.$rootScope).config.labels["ST_User_Task_Name_Label"] + " " + this.dialogModel.clonedUserTask.label;
            }
        }
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
        if (this.dialogModel.clonedUserTask.associatedArtifact === undefined) {
            this.dialogModel.clonedUserTask.associatedArtifact = null;
        }
        if (this.dialogModel.clonedSystemTask.associatedArtifact === undefined) {
            this.dialogModel.clonedSystemTask.associatedArtifact = null;
        }
        this.populateUserTaskChanges();
        this.populateSystemTaskChanges();
        // TODO Not for MDP
        //this.processModelService.setNextNode(this.dialogModel.clonedSystemTask.model, this.dialogModel.nextNode);
    }

    private populateUserTaskChanges() {

        if (this.dialogModel.originalUserTask && this.dialogModel.clonedUserTask) {
            this.dialogModel.originalUserTask.action = this.dialogModel.clonedUserTask.action;
            this.dialogModel.originalUserTask.persona = this.dialogModel.clonedUserTask.persona;
            this.dialogModel.originalUserTask.objective = this.dialogModel.clonedUserTask.objective;
            this.dialogModel.originalUserTask.description = this.dialogModel.clonedUserTask.description;
            this.dialogModel.originalUserTask.associatedArtifact = this.dialogModel.clonedUserTask.associatedArtifact;
        }
    }

    private populateSystemTaskChanges() {
        this.dialogModel.originalSystemTask.action = this.dialogModel.clonedSystemTask.action;
        this.dialogModel.originalSystemTask.associatedImageUrl = this.dialogModel.clonedSystemTask.associatedImageUrl;
        this.dialogModel.originalSystemTask.description = this.dialogModel.clonedSystemTask.description;
        this.dialogModel.originalSystemTask.associatedArtifact = this.dialogModel.clonedSystemTask.associatedArtifact;
        this.dialogModel.originalSystemTask.imageId = this.dialogModel.clonedSystemTask.imageId;
    }

    private showMore(type: string, event: any) {
        // select tab
        if (type === "label") {
            this.isShowMore = !this.isShowMore;
        } else if (type === "info") {
            this.isShowMore = true;
            this.showMoreActiveTabIndex = 0;
        } else if (type === "include") {
            this.isShowMore = true;
            this.showMoreActiveTabIndex = 1;
        }
        this.refreshView();
        event.stopPropagation();
    }

    private refreshView() {
        setTimeout(() => {
            var elem: any = document.getElementsByClassName("modal-dialog")[0].parentElement;
            elem.style.width = "" + (elem.clientWidth - 1) + "px"; 
            elem.style.width = "" + (elem.clientWidth + 1) + "px"; 
        }, 100);
    }

    public getActiveHeader(): string {
        if (this.dialogModel.isSystemTask) {
            return this.dialogModel.clonedSystemTask.label;
        } 
        
        if (this.dialogModel.isUserTask) {
            return this.dialogModel.clonedUserTask.label;
        }

        return null;
    }
}
