import {IMessageService} from "../messages/message.svc";
import {ILoadingOverlayService} from "../../../commonModule/loadingOverlay/loadingOverlay.service";
import {ILocalizationService} from "../../../commonModule/localization/localization.service";
import {IApplicationError} from "../../../shell/error/applicationError";
import {ItemTypePredefined} from "../../models/enums";
import {IArtifact, IItem} from "../../models/models";
import {IArtifactService, IStatefulArtifact, IStatefulArtifactFactory} from "../../../managers/artifact-manager/artifact";
import {IDialogService, IDialogSettings} from "../../../shared/widgets/bp-dialog/bp-dialog";
import {CreateNewArtifactController, ICreateNewArtifactDialogData, ICreateNewArtifactReturn} from "../dialogs/new-artifact/new-artifact";
import {error} from "util";

export interface ICreateArtifactService {
    createNewArtifact(
        artifactId: number,
        artifact: IStatefulArtifact,
        useModal: boolean,
        artifactName?: string,
        artifactTypeId?: number,
        postCreationAction?: (artifactId: number) => void,
        ErrorHandler?: (error: IApplicationError) => void): void;
    getArtifactById(artifactId: number, artifact?: IStatefulArtifact): ng.IPromise<IStatefulArtifact>;
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

    public getArtifactById(artifactId: number, artifact?: IStatefulArtifact): ng.IPromise<IStatefulArtifact> {
        if (!!artifact) {
            return this.$q.resolve(artifact);
        }
        return this.statefulArtifactFactory.createStatefulArtifactFromId(artifactId);
    }

    public createNewArtifact = (
        artifactId: number,
        artifact?: IStatefulArtifact,
        useModal: boolean = true,
        artifactName?: string,
        artifactTypeId?: number,
        postCreationAction?: (artifactId: number) => void,
        ErrorHandler?: (error: IApplicationError) => void): void => {

        let createNewArtifactLoadingId: number;
        let projectId: number;
        let parentId: number;

        this.getArtifactById(artifactId, artifact)
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
                const itemTypeId = result.artifactTypeId;
                const name = result.artifactName;
                createNewArtifactLoadingId = this.loadingOverlayService.beginLoading();
                return this.artifactService.create(name, projectId, parentId, itemTypeId);
            })
            .then((data: IArtifact) => {
                if (!!postCreationAction) {
                    return postCreationAction(data.id);
                } else {
                    return this.$q.resolve();
                }
            })
            .catch((error: IApplicationError) => {
                if (!!ErrorHandler) {
                    return ErrorHandler(error);
                } else {
                    this.messageService.addError("Create_New_Artifact_Error_Generic");
                    return this.$q.reject();
                }
            }).finally(() => {
                    if (!!createNewArtifactLoadingId) {
                        this.loadingOverlayService.endLoading(createNewArtifactLoadingId);
                    }
            });
    };
}
