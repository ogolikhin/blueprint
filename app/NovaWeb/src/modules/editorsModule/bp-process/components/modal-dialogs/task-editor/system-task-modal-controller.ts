import {ILoadingOverlayService} from "../../../../../commonModule/loadingOverlay/loadingOverlay.service";
import {ILocalizationService} from "../../../../../commonModule/localization/localization.service";
import {IMessageService} from "../../../../../main/components/messages/message.svc";
import {ICreateArtifactService} from "../../../../../main/components/projectControls/create-artifact.svc";
import {Models} from "../../../../../main/models";
import {ItemTypePredefined} from "../../../../../main/models/item-type-predefined";
import {IArtifactService, IStatefulArtifactFactory} from "../../../../../managers/artifact-manager/artifact";
import {ISelectionManager} from "../../../../../managers/selection-manager/selection-manager";
import {IDialogService} from "../../../../../shared";
import {ISession} from "../../../../../shell/login/session.svc";
import {IArtifactReference} from "../../../models/process-models";
import {IdGenerator} from "../../diagram/presentation/graph/shapes/id-generator";
import {IModalScope} from "../base-modal-dialog-controller";
import {SystemTaskDialogModel} from "./systemTaskDialogModel";
import {TaskModalController} from "./task-modal-controller";

export class SystemTaskModalController extends TaskModalController<SystemTaskDialogModel> {
    private systemNamePlaceHolderText: string;
    private _idGenerator = new IdGenerator();

    constructor(
        $scope: IModalScope,
        $rootScope: ng.IRootScopeService,
        $timeout: ng.ITimeoutService,
        dialogService: IDialogService,
        $q: ng.IQService,
        localization: ILocalizationService,
        createArtifactService: ICreateArtifactService,
        statefulArtifactFactory: IStatefulArtifactFactory,
        messageService: IMessageService,
        artifactService: IArtifactService,
        loadingOverlayService: ILoadingOverlayService,
        session: ISession,
        selectionManager: ISelectionManager,
        $uibModalInstance?: ng.ui.bootstrap.IModalServiceInstance,
        dialogModel?: SystemTaskDialogModel
    ) {
        super($scope,
        $rootScope,
        $timeout,
        dialogService,
        $q,
        localization,
        createArtifactService,
        statefulArtifactFactory,
        messageService,
        artifactService,
        loadingOverlayService,
        session,
        selectionManager,
        $uibModalInstance,
        dialogModel);
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

    public getPersonaLabel(): string {
        if (this.dialogModel.personaReference.id < 0) {
            return this.dialogModel.personaReference.name;
        }

        return this.dialogModel.personaReference.typePrefix +
            this.dialogModel.personaReference.id +
            ": " +
            this.dialogModel.personaReference.name;
    }

    protected getAssociatedArtifact(): IArtifactReference {
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

    protected getNewArtifactName(): string {
        return this.dialogModel.label;
    }

    protected getItemTypeId(): number {
        return this.dialogModel.itemTypeId;
    }

    protected populateTaskChanges() {
        if (this.dialogModel.originalItem && this.dialogModel) {
            this.dialogModel.originalItem.action = this.dialogModel.action;
            this.dialogModel.originalItem.imageId = this.dialogModel.imageId;
            this.dialogModel.originalItem.associatedImageUrl = this.dialogModel.associatedImageUrl;
            this.dialogModel.originalItem.associatedArtifact = this.dialogModel.associatedArtifact;
            this.dialogModel.originalItem.personaReference = this.dialogModel.personaReference;
        }
    }

    protected getDefaultPersonaReference(): IArtifactReference {
        const defaultSystemPersonaReference = {
            id: this._idGenerator.getSystemPeronaId(),
            projectId: null,
            name: this.localization.get("ST_New_System_Task_Persona"),
            typePrefix: null,
            baseItemTypePredefined: ItemTypePredefined.Actor,
            projectName: null,
            link: null,
            version: null
        };

        return defaultSystemPersonaReference;
    }

    protected getModel(): Models.IArtifact {
        return this.dialogModel.originalItem.model as Models.IArtifact;
    }
}
