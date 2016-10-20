import {BaseModalDialogController, IModalScope} from "./base-modal-dialog-controller";
import {SubArtifactDialogModel} from "./models/sub-artifact-dialog-model";
import {IArtifactReference} from "../../models/process-models";
import {IModalProcessViewModel} from "./models/modal-process-view-model";
import {ICommunicationManager} from "../../services/communication-manager";
import {IDiagramService} from "../../../../editors/bp-diagram/diagram.svc";
import {IDialogSettings, IDialogService} from "../../../../shared";
import {ArtifactPickerDialogController, IArtifactPickerOptions} from "../../../../main/components/bp-artifact-picker";
import {Models} from "../../../../main/models";

export class SubArtifactEditorUserTaskModalController extends BaseModalDialogController<SubArtifactDialogModel> {
    public getLinkableProcesses: (viewValue: string) => ng.IPromise<IArtifactReference[]>;
    public getLinkableArtifacts: (viewValue: string) => ng.IPromise<IArtifactReference[]>;
    public isProjectOnlySearch: boolean = true;
    public isLoadingIncludes: boolean = false;

    private isIncludeResultsVisible: boolean;
    private includeArtifactName: string;
    private isReadonly: boolean = false;
    private isSMB: boolean = false;
    private actionPlaceHolderText: string;
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
        "$sce", 
        "dialogService"
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
                private $sce: ng.ISCEService,
                private dialogService: IDialogService  ) {

        super($rootScope, $scope, $uibModalInstance, dialogModel);

        this.isReadonly = this.dialogModel.isReadonly;

        let isSMBVal = $rootScope["config"].settings.StorytellerIsSMB;

        if (isSMBVal.toLowerCase() === "true") {
            this.isSMB = true;
        }

        this.actionOnBlur();

        if (dialogModel.originalUserTask.associatedArtifact) {
            this.prepIncludeField();
        }

        this.communicationManager.modalDialogManager.setModalProcessViewModel(this.setModalProcessViewModel);
    }

    private setModalProcessViewModel = (modalProcessViewModel) => {
        this.modalProcessViewModel = modalProcessViewModel;
    }

    public prepIncludeField(): void {
        this.isIncludeResultsVisible = true;
        this.includeArtifactName = this.formatIncludeLabel(this.dialogModel.clonedUserTask.associatedArtifact);
    }

    public cleanIncludeField(): void {
        this.isIncludeResultsVisible = false;
        this.dialogModel.clonedUserTask.associatedArtifact = null;
    }

    public formatIncludeLabel(model) {
        if (!model) {
            return "";
        }

        let msg: string;
        if (model.typePrefix === "<Inaccessible>") {
            msg = this.$rootScope["config"].labels["HttpError_Forbidden"];
        } else {
            msg = model.prefix + model.id + " - " + model.name;
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
        if (this.dialogModel.clonedUserTask) {
            if (this.dialogModel.clonedUserTask.action) {
                this.actionOnFocus();
            } else {
                this.actionPlaceHolderText = (<any>this.$rootScope).config.labels["ST_User_Task_Name_Label"] + " " + this.dialogModel.clonedUserTask.label;
            }
        }
    }

    public saveData() {
        if (this.dialogModel.clonedUserTask.associatedArtifact === undefined) {
            this.dialogModel.clonedUserTask.associatedArtifact = null;
        }
        this.populateUserTaskChanges();
    }

    private populateUserTaskChanges() {

        if (this.dialogModel.originalUserTask && this.dialogModel.clonedUserTask) {
            this.dialogModel.originalUserTask.persona = this.dialogModel.clonedUserTask.persona;
            this.dialogModel.originalUserTask.action = this.dialogModel.clonedUserTask.action;
            this.dialogModel.originalUserTask.objective = this.dialogModel.clonedUserTask.objective;
            this.dialogModel.originalUserTask.associatedArtifact = this.dialogModel.clonedUserTask.associatedArtifact;
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
            return this.dialogModel.clonedUserTask.label;
        }

        return null;
    }

    public openArtifactPicker() {
        const dialogSettings = <IDialogSettings>{
            okButton: this.$rootScope["config"].labels["App_Button_Open"],
            template: require("../../../../main/components/bp-artifact-picker/bp-artifact-picker-dialog.html"),
            controller: ArtifactPickerDialogController,
            css: "nova-open-project",
            header: this.$rootScope["config"].labels["ST_Select_Include_Artifact_Label"]
        };

        const dialogData: IArtifactPickerOptions = {
            showSubArtifacts: false
        };

        this.dialogService.open(dialogSettings, dialogData).then((items: Models.IItem[]) => {
            if (items.length === 1) {
                this.dialogModel.clonedUserTask.associatedArtifact = items[0];
                console.log(this.dialogModel.originalUserTask.associatedArtifact);
                this.prepIncludeField();
            }
        });
    }
}
