import {IDiagramService} from "../../../../../editors/bp-diagram/diagram.svc";
import {IDialogSettings, IDialogService} from "../../../../../shared";
import {ArtifactPickerDialogController, IArtifactPickerOptions} from "../../../../../main/components/bp-artifact-picker";
import {Models} from "../../../../../main/models";
import {ILocalizationService} from "../../../../../core";
import {BaseModalDialogController, IModalScope} from "../base-modal-dialog-controller";
import {IArtifactReference, ArtifactReference} from "../../../models/process-models";
import {IModalProcessViewModel} from "../models/modal-process-view-model";
import {ICommunicationManager} from "../../../services/communication-manager";
import {IModalDialogModel} from "../models/modal-dialog-model-interface";

export abstract class TaskModalController<T extends IModalDialogModel> extends BaseModalDialogController<T> {
    
    //public members
    includeArtifactName: string;
    isReadonly: boolean = false;
    isIncludeResultsVisible: boolean;

    //abstract members --> public
    abstract nameOnFocus();
    abstract nameOnBlur();
    abstract getActiveHeader(): string;

    //protected members --> protected
    protected abstract getAssociatedArtifact(): IArtifactReference;
    protected abstract setAssociatedArtifact(value: IArtifactReference);
    protected abstract populateTaskChanges();

    public static $inject = [
        "$scope",
        "$rootScope",
        "$timeout",
        "dialogService",
        "localization"
    ];

    constructor(
                $scope: IModalScope,
                $rootScope: ng.IRootScopeService,
                private $timeout: ng.ITimeoutService,
                private dialogService: IDialogService,
                protected localization: ILocalizationService,
                $uibModalInstance?: ng.ui.bootstrap.IModalServiceInstance,
                dialogModel?: T) {

        super($rootScope, $scope, $uibModalInstance, dialogModel);
        
        this.isReadonly = this.dialogModel.isReadonly || this.dialogModel.isHistoricalVersion;
        
        this.nameOnBlur();

        if (this.getAssociatedArtifact()) {
            this.prepIncludeField();
        }
    }    

    //public methods
    prepIncludeField(): void {
        this.isIncludeResultsVisible = true;
        this.includeArtifactName = this.formatIncludeLabel(this.getAssociatedArtifact());
    }

    cleanIncludeField(): void {
        this.isIncludeResultsVisible = false;
        this.setAssociatedArtifact(null);
    }

    formatIncludeLabel(model: IArtifactReference) {
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

    saveData() {
        if (this.dialogModel.isReadonly) {
            throw new Error("Changes cannot be made or saved as this is a read-only item");
        }

        if (this.getAssociatedArtifact() === undefined) {
            this.setAssociatedArtifact(null);
        }
        this.populateTaskChanges();
    }

    openArtifactPicker() {
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

    //private methods
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
}
