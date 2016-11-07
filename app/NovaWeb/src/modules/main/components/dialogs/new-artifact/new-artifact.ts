import { IDialogSettings, BaseDialogController, IDialogData } from "../../../../shared";
import { IMetaDataService } from "../../../../managers/artifact-manager";
import { ItemTypePredefined } from "../../../../main/models/enums";
import { IProjectMeta, IItemType } from "../../../../main/models/models";
import { IMessageService, ILocalizationService } from "../../../../core";

export interface ICreateNewArtifactDialogData extends IDialogData {
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

    private _projectId: number;
    private _parentId: number;
    private _parentType: number;

    private _projectMeta: IProjectMeta;

    static $inject = [
        "$uibModalInstance",
        "dialogSettings",
        "dialogData",
        "localization",
        "messageService",
        "metadataService"
    ];

    constructor (
        $instance: ng.ui.bootstrap.IModalServiceInstance,
        dialogSettings: IDialogSettings,
        public dialogData: ICreateNewArtifactDialogData,
        private localization: ILocalizationService,
        private messageService: IMessageService,
        private metadataService: IMetaDataService
    ) {
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
    }

    //Dialog return value
    public get returnValue(): ICreateNewArtifactReturn {
        return {
                artifactTypeId: this.newArtifactType.id,
                artifactName: this.newArtifactName
            };
    };

    public filterItemTypePredefinedByParent = (): ItemTypePredefined[] => {
        const allowedItemType: ItemTypePredefined[] = [
            ItemTypePredefined.TextualRequirement,
            ItemTypePredefined.Process,
            ItemTypePredefined.Actor,
            ItemTypePredefined.Document,
            ItemTypePredefined.PrimitiveFolder,
            ItemTypePredefined.ArtifactCollection,
            ItemTypePredefined.CollectionFolder
        ];

        return allowedItemType.filter((itemType: ItemTypePredefined) => {
            if (this._parentType === ItemTypePredefined.CollectionFolder) {
                return itemType === ItemTypePredefined.ArtifactCollection ||
                    itemType === ItemTypePredefined.CollectionFolder;
            } else if (this._parentType === ItemTypePredefined.ArtifactCollection) {
                return itemType === ItemTypePredefined.ArtifactCollection;
            } else if (this._parentType === ItemTypePredefined.Project || this._parentType === ItemTypePredefined.PrimitiveFolder) {
                return itemType !== ItemTypePredefined.ArtifactCollection &&
                    itemType !== ItemTypePredefined.CollectionFolder;
            } else {
                return itemType !== ItemTypePredefined.PrimitiveFolder &&
                    itemType !== ItemTypePredefined.ArtifactCollection &&
                    itemType !== ItemTypePredefined.CollectionFolder;
            }
        });
    };

    public availableItemTypes = (): IItemType[] => {
        const availableItemTypePredefined = this.filterItemTypePredefinedByParent();
        let _availableItemTypes: IItemType[] = [];
        if (this._projectMeta) {
            _availableItemTypes = this._projectMeta.artifactTypes.filter((artifactType: IItemType) => {
                return availableItemTypePredefined.indexOf(artifactType.predefinedType) !== -1 &&
                    artifactType.id > 0; // this is needed for filtering out Project and (main) Collections artifact types
            });

        }
        return _availableItemTypes;
    };

    public get isCreateButtonDisabled(): boolean {
        return _.isUndefined(this.newArtifactName) || !_.isString(this.newArtifactName) || this.newArtifactName.length === 0 ||
            _.isUndefined(this.newArtifactType) || _.isNull(this.newArtifactType);
    }
}