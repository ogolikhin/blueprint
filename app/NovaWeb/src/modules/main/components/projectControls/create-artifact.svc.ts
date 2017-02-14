import {ILoadingOverlayService} from "../../../commonModule/loadingOverlay/loadingOverlay.service";
import {ILocalizationService} from "../../../commonModule/localization/localization.service";
import {IArtifactService, IStatefulArtifact, IStatefulArtifactFactory} from "../../../managers/artifact-manager/artifact";
import {IDialogService, IDialogSettings} from "../../../shared/widgets/bp-dialog/bp-dialog";
import {ItemTypePredefined} from "../../models/itemTypePredefined.enum";
import {IArtifact} from "../../models/models";
import {CreateNewArtifactController, ICreateNewArtifactDialogData, ICreateNewArtifactReturn} from "../dialogs/new-artifact";
import {IMessageService} from "../messages/message.svc";

export interface ICreateArtifactService {
    createNewArtifact(
        artifactId: number,
        artifact: IStatefulArtifact,
        useModal: boolean,
        artifactName?: string,
        artifactTypeId?: number): ng.IPromise<IArtifact>;
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
    }

    private getArtifactById(artifactId: number, artifact?: IStatefulArtifact): ng.IPromise<IStatefulArtifact> {
        return artifact ?
            this.$q.resolve(artifact) :
            this.statefulArtifactFactory.createStatefulArtifactFromId(artifactId);
    }

    public createNewArtifact(
        parentArtifactId: number,
        parentArtifact?: IStatefulArtifact,
        useModal: boolean = true,
        artifactName?: string,
        artifactTypeId?: number): ng.IPromise<IArtifact> {

        let createNewArtifactLoadingId: number;
        let projectId: number;
        let parentId: number;

        return this.getArtifactById(parentArtifactId, parentArtifact)
            .then((parentArtifact: IStatefulArtifact) => {
                projectId = parentArtifact.projectId;
                parentId = parentArtifact.predefinedType !== ItemTypePredefined.ArtifactCollection ? parentArtifact.id : parentArtifact.parentId;

                if (useModal) {
                    return this.dialogService.open(
                        <IDialogSettings>{
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
                        }
                    );
                }

                return <ICreateNewArtifactReturn>{
                    artifactTypeId: artifactTypeId,
                    artifactName: artifactName
                };
            })
            .then((result: ICreateNewArtifactReturn) => {
                createNewArtifactLoadingId = this.loadingOverlayService.beginLoading();
                return this.artifactService.create(result.artifactName, projectId, parentId, result.artifactTypeId);
            })
            .finally(() => {
                if (createNewArtifactLoadingId) {
                    this.loadingOverlayService.endLoading(createNewArtifactLoadingId);
                }
            });
    }
}
