import {IDiagramService} from "../../../../../editors/bp-diagram/diagram.svc";
import {IDialogSettings, IDialogService} from "../../../../../shared";
import {ArtifactPickerDialogController, IArtifactPickerOptions} from "../../../../../main/components/bp-artifact-picker";
import {Models} from "../../../../../main/models";
import {ILocalizationService} from "../../../../../core";
import {BaseModalDialogController, IModalScope} from "../base-modal-dialog-controller";
import {SubArtifactUserTaskDialogModel} from "../models/sub-artifact-dialog-model";
import {IArtifactReference, ArtifactReference} from "../../../models/process-models";
import {IModalProcessViewModel} from "../models/modal-process-view-model";
import {ICommunicationManager} from "../../../services/communication-manager";
import {IModalDialogModel} from "../models/modal-dialog-model-interface";

export abstract class TaskModalController<T extends IModalDialogModel> extends BaseModalDialogController<T> {
    public getLinkableProcesses: (viewValue: string) => ng.IPromise<IArtifactReference[]>;
    public getLinkableArtifacts: (viewValue: string) => ng.IPromise<IArtifactReference[]>;
    public isProjectOnlySearch: boolean = true;
    public isLoadingIncludes: boolean = false;
    public includeArtifactName: string;
    public isReadonly: boolean = false;

    protected actionPlaceHolderText: string;
    protected modalProcessViewModel: IModalProcessViewModel;

    private isIncludeResultsVisible: boolean;    
    private isSMB: boolean = false;    
    private searchIncludesDelay: ng.IPromise<any>;

    public static $inject = [
        "$scope",
        
        
        "communicationManager",
        //"artifactSearchService",
        "$rootScope",
        "$q",
        "$timeout",
        "$sce", 
        "dialogService",
        "localization"
    ];

    constructor($scope: IModalScope,
                
                private communicationManager: ICommunicationManager,
                // TODO look at this later
                //private artifactSearchService: Shell.IArtifactSearchService,
                $rootScope: ng.IRootScopeService,
                private $q: ng.IQService,
                private $timeout: ng.ITimeoutService,
                private $sce: ng.ISCEService,
                private dialogService: IDialogService,
                protected localization: ILocalizationService  ) {

        super($rootScope, $scope);

        this.modalProcessViewModel = <IModalProcessViewModel>this.resolve["modalProcessViewModel"];
        this.isReadonly = this.dialogModel.isReadonly || this.dialogModel.isHistoricalVersion;

        let isSMBVal = $rootScope["config"].settings.StorytellerIsSMB;

        if (isSMBVal.toLowerCase() === "true") {
            this.isSMB = true;
        }

        if (this.getAssociatedArtifact()) {
            this.prepIncludeField();
        }

        this.communicationManager.modalDialogManager.setModalProcessViewModel(this.setModalProcessViewModel);
    }

    public abstract getActiveHeader(): string;

    protected abstract getAssociatedArtifact(): IArtifactReference;
    protected abstract setAssociatedArtifact(value: IArtifactReference);

    protected setModalProcessViewModel = (modalProcessViewModel) => {
        this.modalProcessViewModel = modalProcessViewModel;
    }

    public prepIncludeField(): void {
        this.isIncludeResultsVisible = true;
        this.includeArtifactName = this.formatIncludeLabel(this.getAssociatedArtifact());
    }

    public cleanIncludeField(): void {
        this.isIncludeResultsVisible = false;
        this.setAssociatedArtifact(null);
    }

    public formatIncludeLabel(model) {
        if (!model) {
            return "";
        }

        let msg: string;
        if (model.typePrefix === "<Inaccessible>") {
            msg = this.localization.get("HttpError_Forbidden");
        } else {
            msg = model.typePrefix + model.id + " - " + model.name;
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

    public saveData() {
        if (this.getAssociatedArtifact() === undefined) {
            this.setAssociatedArtifact(null);
        }
        this.populateTaskChanges();
    }

    protected abstract populateTaskChanges();

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

    public openArtifactPicker() {
        const dialogSettings = <IDialogSettings>{
            okButton: this.localization.get("App_Button_Open"),
            template: require("../../../../../main/components/bp-artifact-picker/bp-artifact-picker-dialog.html"),
            controller: ArtifactPickerDialogController,
            css: "nova-open-project",
            header: this.localization.get("ST_Select_Include_Artifact_Label")
        };

        const dialogData: IArtifactPickerOptions = {
        };

        this.dialogService.open(dialogSettings, dialogData).then((items: Models.IItem[]) => {
            if (items.length === 1) {
                const associatedArtifact = new ArtifactReference();
                associatedArtifact.baseItemTypePredefined = items[0].predefinedType;
                associatedArtifact.id = items[0].id;
                associatedArtifact.name = items[0].name;
                associatedArtifact.typePrefix = items[0].prefix;
                this.setAssociatedArtifact(associatedArtifact);                  
                this.prepIncludeField();
                this.prepIncludeField();
            }
        });
    }
}
