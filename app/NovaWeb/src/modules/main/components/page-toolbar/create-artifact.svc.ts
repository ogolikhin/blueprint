import {ItemTypePredefined} from "../../models/enums";
import {IArtifact, IItem} from "../../models/models";
import {ILocalizationService} from "../../../core/localization/localizationService";
import {IArtifactService, IStatefulArtifact, IStatefulArtifactFactory} from "../../../managers/artifact-manager/artifact";
import {ILoadingOverlayService} from "../../../core/loading-overlay/loading-overlay.svc";
import {IMessageService} from "../../../core/messages/message.svc";
import {IDialogService, IDialogSettings} from "../../../shared/widgets/bp-dialog/bp-dialog";
import {CreateNewArtifactController, ICreateNewArtifactDialogData, ICreateNewArtifactReturn} from "../dialogs/new-artifact/new-artifact";
import {error} from "util";
import {IApplicationError} from "../../../core/error/applicationError";

export interface ICreateArtifactService {
    createNewArtifact(
        artifactId: number,
        useModal: boolean,
        artifactName?: string,
        artifactTypeId?: number,
        postCreationAction?: (artifact: IItem) => void): void;
}

export class CreateArtifactService implements ICreateArtifactService {
    public static $inject = [
        "dialogService",
        "localization",
        "$q",
        "statefulArtifactFactory",
        "loadingOverlayService",
        "messageService",
        "artifactService"
    ];

    constructor(
        private dialogService: IDialogService,
        private localization: ILocalizationService,
        private $q: ng.IQService,
        private statefulArtifactFactory: IStatefulArtifactFactory,
        private loadingOverlayService: ILoadingOverlayService,
        private messageService: IMessageService,
        private artifactService: IArtifactService,
    ) {
        // empty
    }

    private getArtifactById(artifactId: number): ng.IPromise<IStatefulArtifact> {
        return this.statefulArtifactFactory.createStatefulArtifactFromId(artifactId);
    }

    public createNewArtifact = (
        artifactId: number,
        useModal: boolean = true,
        artifactName?: string,
        artifactTypeId?: number,
        postCreationAction?: (artifact: IItem) => void): void => {

        let createNewArtifactLoadingId: number;
        let newArtifact: IStatefulArtifact;
        let projectId: number;
        let parentId: number;

        this.getArtifactById(artifactId)
        .then((parentArtifact: IStatefulArtifact) => {
            projectId = parentArtifact.projectId;
            parentId = parentArtifact.predefinedType !== ItemTypePredefined.ArtifactCollection ? parentArtifact.id : parentArtifact.parentId;
            if (useModal) {
                return this.dialogService.open(<IDialogSettings>{
                        okButton: this.localization.get("App_Button_Create"),
                        cancelButton: this.localization.get("App_Button_Cancel"),
                        template: require("../dialogs/new-artifact/new-artifact.html"),
                        controller: CreateNewArtifactController,
                        css: "nova-new-artifact"
                    },
                    <ICreateNewArtifactDialogData>{
                        projectId: projectId,
                        parentId: parentId,
                        parentPredefinedType: parentArtifact.predefinedType
                    });
                } else {
                    const result: ICreateNewArtifactReturn = {
                        artifactTypeId: artifactTypeId,
                        artifactName: artifactName
                    };
                    return this.$q.resolve(result);
                }
            })
            .then((result: ICreateNewArtifactReturn) => {
                createNewArtifactLoadingId = this.loadingOverlayService.beginLoading();
                // if canceled:
                if (!result.artifactTypeId) {
                    return this.$q.reject();
                }
                const itemTypeId = result.artifactTypeId;
                const name = result.artifactName;
                return this.artifactService.create(name, projectId, parentId, itemTypeId);
            })
            .then((data: IArtifact) => {
                return this.getArtifactById(data.id);
            })
            .then((artifact: IStatefulArtifact) => {
                newArtifact = artifact;
                // save and publish
                return newArtifact.publish();
            })
            .then (() => {
                postCreationAction(newArtifact);
            })
            .catch((error: IApplicationError) => {
                if (error.statusCode === 404 && error.errorCode === 102) {
                    this.messageService.addError("Create_New_Artifact_Error_404_102", true);
                } else if (error.statusCode === 404 && error.errorCode === 101) {
                    // parent not found, we refresh the single project and move to the root
                    this.messageService.addError("Create_New_Artifact_Error_404_101", true);
                } else if (error.statusCode === 404 && error.errorCode === 109) {
                    // artifact type not found, we refresh the single project
                    this.messageService.addError("Create_New_Artifact_Error_404_109", true);
                } else if (!error.handled) {
                    this.messageService.addError("Create_New_Artifact_Error_Generic");
                }
            }).finally(() => {
                    if (!!createNewArtifactLoadingId) {
                        this.loadingOverlayService.endLoading(createNewArtifactLoadingId);
                    }
            });
    };
}
