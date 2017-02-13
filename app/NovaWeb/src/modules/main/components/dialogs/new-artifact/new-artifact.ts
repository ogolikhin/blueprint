import {IDialogSettings, BaseDialogController} from "../../../../shared";
import {IMetaDataService} from "../../../../managers/artifact-manager/metadata/metadata.svc";
import {ItemTypePredefined} from "../../../models/enums";
import {IProjectMeta, IItemType} from "../../../models/models";
import {ILocalizationService} from "../../../../commonModule/localization/localization.service";
import {IMessageService} from "../../messages/message.svc";

export interface ICreateNewArtifactDialogData {
    projectId: number;
    parentId: number;
    parentPredefinedType: number;
}

export interface ICreateNewArtifactReturn {
    artifactTypeId: number;
    artifactName: string;
}

export class CreateNewArtifactController extends BaseDialogController {
    public newArtifactName: string;
    public newArtifactType: IItemType;
    public hasFocus: boolean;
    public isDropdownOpen: boolean;

    private _projectId: number;
    private _parentId: number;
    private _parentType: number;

    private _projectMeta: IProjectMeta;

    // TODO: remove after modals are refactored
    private isIE11: boolean;

    static $inject = [
        "$uibModalInstance",
        "dialogSettings",
        "dialogData",
        "localization",
        "messageService",
        "metadataService",
        "$timeout"
    ];

    constructor($instance: ng.ui.bootstrap.IModalServiceInstance,
                dialogSettings: IDialogSettings,
                public dialogData: ICreateNewArtifactDialogData,
                private localization: ILocalizationService,
                private messageService: IMessageService,
                private metadataService: IMetaDataService,
                private $timeout: ng.ITimeoutService) {
        super($instance, dialogSettings);

        if (!dialogData) {
            throw new Error("Parent data not provided");
        }

        if (!_.isNumber(dialogData.projectId) || dialogData.projectId <= 0) {
            throw new Error("Project not provided");
        }
        this._projectId = dialogData.projectId;

        if (!_.isNumber(dialogData.parentId) || dialogData.parentId <= 0 || !ItemTypePredefined[dialogData.parentPredefinedType]) {
            // something is wrong with the parent data, we will create the artifact as child of the project
            this._parentId = dialogData.projectId;
            this._parentType = ItemTypePredefined.Project;
        } else {
            this._parentId = dialogData.parentId;
            this._parentType = dialogData.parentPredefinedType;
        }

        metadataService.get(this._projectId).then(
            (results) => {
                this._projectMeta = results.data;
            },
            (error) => {
                this.cancel();
                if (error && error.message) {
                    this.messageService.addError(error.message || "Project_MetaDataNotFound");
                } else {
                    throw new Error("Project metadata missing");
                }
            }
        );

        this.hasFocus = false;
        this.isDropdownOpen = false;
    }

    public $onInit = () => {
        // TODO: remove after modals are refactored
        this.isIE11 = false;
        if (window && window.navigator) {
            const ua = window.navigator.userAgent;
            this.isIE11 = !!(ua.match(/Trident/) && ua.match(/rv[ :]11/)) && !ua.match(/edge/i);
        }
    };

    //Dialog return value
    public get returnValue(): ICreateNewArtifactReturn {
        return {
            artifactTypeId: this.newArtifactType.id,
            artifactName: this.newArtifactName
        };
    };

    public filterItemTypePredefinedByParent = (): ItemTypePredefined[] => {
        const allowedProjectItemType: ItemTypePredefined[] = [
            ItemTypePredefined.TextualRequirement,
            ItemTypePredefined.PrimitiveFolder,
            ItemTypePredefined.Actor,
            ItemTypePredefined.Document,
            ItemTypePredefined.Process
        ];
        const allowedBaselinesAndReviewsItemType: ItemTypePredefined[] = [
            ItemTypePredefined.BaselineFolder,
            ItemTypePredefined.ArtifactBaseline,
            ItemTypePredefined.ArtifactReviewPackage
        ];
        const allowedCollectionsItemType: ItemTypePredefined[] = [
            ItemTypePredefined.CollectionFolder,
            ItemTypePredefined.ArtifactCollection
        ];

        switch (this._parentType) {
            case ItemTypePredefined.Project:
            case ItemTypePredefined.PrimitiveFolder:
                return allowedProjectItemType;
            case ItemTypePredefined.BaselinesAndReviews:
            case ItemTypePredefined.BaselineFolder:
                return allowedBaselinesAndReviewsItemType;
            case ItemTypePredefined.Collections:
            case ItemTypePredefined.CollectionFolder:
                return allowedCollectionsItemType;
            case ItemTypePredefined.ArtifactBaseline:
            case ItemTypePredefined.ArtifactReviewPackage:
                return allowedBaselinesAndReviewsItemType.filter((itemType: ItemTypePredefined) => itemType !== ItemTypePredefined.BaselineFolder);
            case ItemTypePredefined.ArtifactCollection:
                return allowedCollectionsItemType.filter((itemType: ItemTypePredefined) => itemType !== ItemTypePredefined.CollectionFolder);
            default:
                return allowedProjectItemType.filter((itemType: ItemTypePredefined) => itemType !== ItemTypePredefined.PrimitiveFolder);
        }
    };

    public availableItemTypes = (): IItemType[] => {
        const availableItemTypePredefined = this.filterItemTypePredefinedByParent();
        let _availableItemTypes: IItemType[] = [];
        if (this._projectMeta) {
            _availableItemTypes = this._projectMeta.artifactTypes.filter((artifactType: IItemType) => {
                return availableItemTypePredefined.indexOf(artifactType.predefinedType) !== -1 &&
                    artifactType.usedInThisProject &&
                    artifactType.id > 0; // this is needed for filtering out Project and (main) Collections artifact types
            });

        }
        return _availableItemTypes.sort((a, b) => { // case-insensitive sort
            if (a.name.toLowerCase() > b .name.toLowerCase()) {
                return 1;
            }
            if (a.name.toLowerCase() < b.name.toLowerCase()) {
                return -1;
            }
            return 0;
        });
    };

    public onKeyUp = (event: KeyboardEvent) => {
        if (event.keyCode === 13) { // ENTER
            if (!this.isCreateButtonDisabled) {
                this.ok();
            }
        }
    };

    public get isCreateButtonDisabled(): boolean {
        return _.isUndefined(this.newArtifactName) || !_.isString(this.newArtifactName) || this.newArtifactName.trim().length === 0 ||
            _.isUndefined(this.newArtifactType) || _.isNull(this.newArtifactType) || this.newArtifactType.toString().length === 0;
    }

    public setFocus = (focusState: boolean = true): void => {
        this.hasFocus = focusState;
        this.refreshView();
    };

    // TODO: remove after modals are refactored as this is a workaround to force re-rendering of the dialog in IE11
    public refreshView() {
        if (this.isIE11) {
            const modal = document.getElementsByClassName("new-artifact").item(0) as HTMLElement;
            const labels = document.getElementsByClassName("new-artifact__label");

            if (!modal || !labels || !labels.length) {
                return;
            }

            for (let i = 0; i < labels.length; i++) {
                const label = labels[i] as HTMLElement;
                const text = label.innerText;
                label.innerText = text + " ";
                // label.getBoundingClientRect();
                // window.getComputedStyle(label);
                this.$timeout(() => {
                    // label.getBoundingClientRect();
                    // window.getComputedStyle(label);
                    label.innerText = text;
                }, 300, false);
            }

            this.$timeout(() => {
                modal.getBoundingClientRect();
            }, 275, false);
        }
    }
}
