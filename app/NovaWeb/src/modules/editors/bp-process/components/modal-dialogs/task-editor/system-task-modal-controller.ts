import {IDialogService} from "../../../../../shared";
import {IModalScope} from "../base-modal-dialog-controller";
import {SystemTaskDialogModel} from "./sub-artifact-dialog-model";
import {IArtifactReference} from "../../../models/process-models";
import {TaskModalController} from "./task-modal-controller";
import {ILocalizationService} from "../../../../../core/localization/localizationService";
import {Models} from "../../../../../main/models";
import {IdGenerator} from "../../diagram/presentation/graph/shapes/id-generator";

export class SystemTaskModalController extends TaskModalController<SystemTaskDialogModel> {
    private systemNamePlaceHolderText: string;
    private _idGenerator = new IdGenerator();

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

    protected getPersonaReference(): IArtifactReference {
        return this.dialogModel.personaReference;
    }

    protected setPersonaReference(value: IArtifactReference) {
        if (value) {
            this.dialogModel.personaReference = value;
            this.dialogModel.persona = value.name;
        } else {
            const defaultSystemPersonaReference = {
                id: this._idGenerator.getUserPeronaId(),
                projectId: null,
                name: this.localization.get("ST_New_System_Task_Persona"),
                typePrefix: null,
                baseItemTypePredefined: Models.ItemTypePredefined.Actor,
                projectName: null,
                link: null,
                version: null
            }
            this.dialogModel.personaReference = defaultSystemPersonaReference;
            this.dialogModel.persona = this.localization.get("ST_New_System_Task_Persona");
        }
    }

    protected populateTaskChanges() {
        if (this.dialogModel.originalItem && this.dialogModel) {
            this.dialogModel.originalItem.persona = this.dialogModel.persona;
            this.dialogModel.originalItem.action = this.dialogModel.action;
            this.dialogModel.originalItem.imageId = this.dialogModel.imageId;
            this.dialogModel.originalItem.associatedImageUrl = this.dialogModel.associatedImageUrl;
            this.dialogModel.originalItem.associatedArtifact = this.dialogModel.associatedArtifact;
            this.dialogModel.originalItem.personaReference = this.dialogModel.personaReference;
        }
    }
}
