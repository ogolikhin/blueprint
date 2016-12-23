import {BPButtonAction, IDialogSettings, IDialogService, BPDropdownAction, BPDropdownItemAction} from "../../../../shared";
import {IStatefulArtifact} from "../../../../managers/artifact-manager";
import {IProjectManager} from "../../../../managers";
import {IMessageService} from "../../../../core/messages/message.svc";
import {ILocalizationService} from "../../../../core/localization/localizationService";
import {
    AddArtifactToCollectionDialogController,
    IAddArtifactToCollectionResult
} from "../../../../main/components/dialogs/add-artifact-to-collection";

import {Models, Enums} from "../../../../main/models";
import {ItemTypePredefined} from "../../../../main/models/enums";
import {ILoadingOverlayService} from "../../../../core/loading-overlay/loading-overlay.svc";
import {INavigationService} from "../../../../core/navigation/navigation.svc";

export class AddToCollectionAction extends BPDropdownAction {

    public addDescendants: boolean = false;

    constructor(private $q: ng.IQService,
                private artifact: IStatefulArtifact,
                private localization: ILocalizationService,
                private messageService: IMessageService,
                private projectManager: IProjectManager,
                private dialogService: IDialogService,
                private navigationService: INavigationService,
                private loadingOverlayService: ILoadingOverlayService) {
        super();

        this.actions.push(
            new BPDropdownItemAction(
                localization.get("Artifact_Add_To_Collection_Picker_Header"),
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
            //this part will be implemented in US4214 [Collection] Artifact View - Add to a collection
        });
    }
}
