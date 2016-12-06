import {IDialogService} from "../../../../../shared";
import {IModalScope} from "../base-modal-dialog-controller";
import {UserTaskDialogModel} from "./sub-artifact-dialog-model";
import {IArtifactReference} from "../../../models/process-models";
import {TaskModalController} from "./task-modal-controller";
import {ILocalizationService} from "../../../../../core/localization/localizationService";
import {Models} from "../../../../../main/models";
import {IdGenerator} from "../../diagram/presentation/graph/shapes/id-generator";

export class UserTaskModalController extends TaskModalController<UserTaskDialogModel> {
    public actionPlaceHolderText: string;
    private _idGenerator = new IdGenerator();

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
        $timeout: ng.ITimeoutService,
        dialogService: IDialogService,
        localization: ILocalizationService,
        $uibModalInstance?: ng.ui.bootstrap.IModalServiceInstance,
        dialogModel?: UserTaskDialogModel
    ) {
        super($scope, $rootScope, $timeout, dialogService, localization, $uibModalInstance, dialogModel);
    }

    public nameOnFocus() {
        this.actionPlaceHolderText = this.localization.get("ST_User_Task_Name_Label");
    }

    public nameOnBlur() {
        if (this.dialogModel.action) {
            this.nameOnFocus();
        } else {
            this.actionPlaceHolderText = this.localization.get("ST_User_Task_Name_Label") + " " + this.dialogModel.label;
        }
    }

    public getActiveHeader(): string {
        return this.dialogModel.label;
    }

    public getPersonaLabel(): string {
        if (this.dialogModel.personaReference.id < 0) {
            return this.dialogModel.personaReference.name;
        }

        return this.dialogModel.personaReference.typePrefix +
            this.dialogModel.personaReference.id +
            ": " +
            this.dialogModel.personaReference.name;
    }


    protected  getAssociatedArtifact(): IArtifactReference {
        return this.dialogModel.associatedArtifact;
    }

    protected setAssociatedArtifact(value: IArtifactReference) {
        this.dialogModel.associatedArtifact = value;
    }

    protected getPersonaReference(): IArtifactReference {
        return this.dialogModel.personaReference;
    }

    protected setPersonaReference(value: IArtifactReference) {
        if (value) {
            this.dialogModel.personaReference = value;
        } else {
            this.dialogModel.personaReference = this.getDefaultPersonaReference();
        }
    }

    protected populateTaskChanges() {
        if (this.dialogModel && this.dialogModel.originalItem) {
            this.dialogModel.originalItem.action = this.dialogModel.action;
            this.dialogModel.originalItem.objective = this.dialogModel.objective;
            this.dialogModel.originalItem.associatedArtifact = this.dialogModel.associatedArtifact;
            this.dialogModel.originalItem.personaReference = this.dialogModel.personaReference;
        }
    }

    protected getDefaultPersonaReference(): IArtifactReference {
        const defaultUserPersonaReference = {
            id: this._idGenerator.getUserPeronaId(),
            projectId: null,
            name: this.localization.get("ST_New_User_Task_Persona"),
            typePrefix: null,
            baseItemTypePredefined: Models.ItemTypePredefined.Actor,
            projectName: null,
            link: null,
            version: null
        };

        return defaultUserPersonaReference;
    }

    public getModel(): Models.IArtifact {
        return <Models.IArtifact>this.dialogModel.originalItem.model;
    }
}
