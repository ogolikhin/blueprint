import {BPButtonAction, IDialogSettings, IDialogService, BPDropdownAction, BPDropdownItemAction} from "../../../../shared";
import {IStatefulArtifact} from "../../../../managers/artifact-manager";
import {IProjectManager} from "../../../../managers";
import {IMessageService} from "../../../../core/messages/message.svc";
import {ILocalizationService} from "../../../../core/localization/localizationService";
import {
    MoveCopyArtifactPickerDialogController,
    MoveCopyArtifactResult,
    MoveCopyArtifactInsertMethod,
    IMoveCopyArtifactPickerOptions,
    MoveCopyActionType
} from "../../../../main/components/dialogs/move-copy-artifact/move-copy-artifact";
import {Models, Enums} from "../../../../main/models";
import {ItemTypePredefined} from "../../../../main/models/enums";
import {ILoadingOverlayService} from "../../../../core/loading-overlay/loading-overlay.svc";
import {INavigationService} from "../../../../core/navigation/navigation.svc";

export enum AddToCollectionActionType {
    Add
}

export class AddToCollectionAction extends BPDropdownAction {
    private actionType: AddToCollectionActionType;

    constructor(private $q: ng.IQService,
                private artifact: IStatefulArtifact,
                private localization: ILocalizationService,
                private messageService: IMessageService,
                private projectManager: IProjectManager,
                private dialogService: IDialogService,
                private navigationService: INavigationService,
                private loadingOverlayService: ILoadingOverlayService) {
        super();

        if (!localization) {
            throw new Error("Localization service not provided or is null");
        }

        if (!projectManager) {
            throw new Error("Project manager not provided or is null");
        }

        if (!dialogService) {
            throw new Error("Dialog service not provided or is null");
        }

        this.actions.push(
            new BPDropdownItemAction(
                "Add to Collection",
                () => this.executeAdd(),
                (): boolean => this.canAddToCollection(),
                "fonticon2-move"
            )
        );
    }

    public get tooltip(): string {
        return "Add to collection";
    }

    public executeAdd() {
        this.actionType = AddToCollectionActionType.Add;
        this.loadProjectIfNeeded();
    }

    private canAddToCollection() {
        const invalidTypes = [
            ItemTypePredefined.CollectionFolder,
            ItemTypePredefined.ArtifactCollection
        ];

        return invalidTypes.indexOf(this.artifact.predefinedType) === -1;
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
            .catch((err) => this.messageService.addError(err))
            .then(() => {
                this.openMoveCopyDialog();
            });
    }

    private openMoveCopyDialog(): ng.IPromise<void> {
        //next - open the move to dialog
        let okButtonLabel: string;
        let headerLabel: string;

        okButtonLabel = "App_Button_Add";
        headerLabel = "Artifact_Collection_Add_Artifacts_Picker_Header";


        const dialogSettings = <IDialogSettings>{
            okButton: this.localization.get(okButtonLabel),
            template: require("../../../../main/components/dialogs/add-artifact-to-collection/add-artifact-to-collection-dialog.html"),
            controller: MoveCopyArtifactPickerDialogController,
            css: "nova-open-project",
            header: this.localization.get(headerLabel)
        };

        const collectionTypes = [];

        const dialogData: any = {
            showProjects: true,
            showArtifacts: false,
            showSubArtifacts: false,
            showCollections: true,
            selectionMode: "single",
            currentArtifact: this.artifact,
            actionType: this.actionType,
            selectableItemTypes: [ItemTypePredefined.ArtifactCollection]
        };

        return this.dialogService.open(dialogSettings, dialogData).then((result: MoveCopyArtifactResult[]) => {
            console.log("MoveCopyArtifactResult");
        });
    }
}
