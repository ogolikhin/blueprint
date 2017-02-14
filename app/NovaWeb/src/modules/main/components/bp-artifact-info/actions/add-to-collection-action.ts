import {IItemInfoResult, IItemInfoService} from "../../../../commonModule/itemInfo/itemInfo.service";
import {ILoadingOverlayService} from "../../../../commonModule/loadingOverlay/loadingOverlay.service";
import {ILocalizationService} from "../../../../commonModule/localization/localization.service";
import {INavigationService} from "../../../../commonModule/navigation/navigation.service";
import {ICollectionService} from "../../../../editorsModule/collection/collection.service";
import {AddArtifactToCollectionDialogController, IAddArtifactToCollectionResult} from "../../../../main/components/dialogs/add-artifact-to-collection";
import {IProjectManager} from "../../../../managers";
import {IStatefulArtifact} from "../../../../managers/artifact-manager";
import {BPDropdownAction, BPDropdownItemAction, IDialogService, IDialogSettings} from "../../../../shared";
import {ErrorCode} from "../../../../shell/error/error-code";
import {ItemTypePredefined} from "../../../models/itemTypePredefined.enum";
import {IMessageService} from "../../messages/message.svc";


export class AddToCollectionAction extends BPDropdownAction {

    public addDescendants: boolean = false;

    constructor(private $q: ng.IQService,
                private artifact: IStatefulArtifact,
                private localization: ILocalizationService,
                private messageService: IMessageService,
                private projectManager: IProjectManager,
                private dialogService: IDialogService,
                private navigationService: INavigationService,
                private loadingOverlayService: ILoadingOverlayService,
                private collectionService: ICollectionService,
                private itemInfoService: IItemInfoService) {
        super();

        this.actions.push(
            new BPDropdownItemAction(
                localization.get("Artifact_Add_To_Collection"),
                () => this.loadProjectIfNeeded(),
                (): boolean => this.canAddToCollection(),
                "fonticon fonticon2-add-artifact"
            )
        );
    }

    private canAddToCollection() {
        const invalidTypes = [
            ItemTypePredefined.Project,
            ItemTypePredefined.Collections,
            ItemTypePredefined.BaselinesAndReviews,
            ItemTypePredefined.BaselineFolder,
            ItemTypePredefined.ArtifactBaseline,
            ItemTypePredefined.ArtifactReviewPackage,
            ItemTypePredefined.CollectionFolder,
            ItemTypePredefined.ArtifactCollection
        ];

        return invalidTypes.indexOf(this.artifact.predefinedType) === -1 && !this.artifact.artifactState.deleted
            && !this.artifact.artifactState.historical;
    }

    private loadProjectIfNeeded() {
        //first, check if project is loaded, and if not - load it
        let loadProjectPromise: ng.IPromise<any>;
        if (!this.projectManager.getProject(this.artifact.projectId)) {
            loadProjectPromise = this.projectManager.add(this.artifact.projectId);
        } else {
            loadProjectPromise = this.$q.resolve();
        }

        loadProjectPromise
            .then(() => {
                this.openAddArtifactToCollectionDialog();
            })
            .catch((err) => this.messageService.addError(err));
    }

    private openAddArtifactToCollectionDialog(): ng.IPromise<void> {
        const dialogSettings = <IDialogSettings>{
            okButton: this.localization.get("App_Button_Add"),
            template: require("../../../../main/components/dialogs/add-artifact-to-collection/add-artifact-to-collection-dialog.html"),
            controller: AddArtifactToCollectionDialogController,
            css: "nova-open-project",
            header: this.localization.get("Artifact_Add_To_Collection_Picker_Header")
        };


        const collectionTypes = [];

        const dialogData: any = {
            showProjects: false,
            showArtifacts: false,
            showSubArtifacts: false,
            showCollections: true,
            selectionMode: "single",
            currentArtifact: this.artifact,
            selectableItemTypes: [ItemTypePredefined.ArtifactCollection]
        };

        return this.dialogService.open(dialogSettings, dialogData).then((result: IAddArtifactToCollectionResult) => {
            const loader = this.loadingOverlayService.beginLoading();
            this.collectionService.addArtifactToCollection(this.artifact.id, result.collectionId, result.addDescendants).then((artifactsAdded: number) => {
                this.messageService.addInfo(this.localization.get("Artifact_Add_To_Collection_Success"));
            }).catch((error: any) => {
                //ignore authentication errors here
                if (error && error.errorCode === ErrorCode.LockedByOtherUser) {
                    return this.itemInfoService.get(result.collectionId)
                        .then((collection: IItemInfoResult) => {
                            let error = this.localization.get("Artifact_Add_To_Collection_Filed_Because_Lock");
                            this.messageService.addError(error.replace("{userName}", collection.lockedByUser.displayName));
                        });
                } else if (error) {
                    this.messageService.addError(error["message"] || "Error occured during adding artifacts to collection.");
                }
                }).finally(() => {
                    this.loadingOverlayService.endLoading(loader);
            });
        });
    }
}
