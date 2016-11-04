import { IDialogSettings, BaseDialogController, IDialogData } from "../../../../shared";
import { IProjectService } from "../../../../managers/project-manager";
import { ItemTypePredefined } from "../../../../main/models/enums";
import { IProjectMeta } from "../../../../main/models/models";

export interface ICreateNewArtifactController {
    errorMessage: string;
    hasError: boolean;
}

export interface ICreateNewArtifactDialogData extends IDialogData {
    projectId: number;
    parentId: number;
    parentPredefinedType: number;
}

export class CreateNewArtifactController extends BaseDialogController implements ICreateNewArtifactController {
    private _errorMessage: string;

    private _projectId: number;
    private _parentId: number;
    private _parentType: number;

    private _projectMeta: IProjectMeta;
    private _acceptableItemType: ItemTypePredefined[];

    static $inject = [
        "$uibModalInstance",
        "dialogSettings",
        "projectService",
        "dialogData"
    ];

    constructor (
        $instance: ng.ui.bootstrap.IModalServiceInstance,
        dialogSettings: IDialogSettings,
        projectService: IProjectService,
        public dialogData: ICreateNewArtifactDialogData
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

        projectService.getProjectMeta(this._projectId).then(
            (projectMeta: IProjectMeta) => {
                this._projectMeta = projectMeta;
            },
            (error) => {
                throw new Error("project meta error");
            }
        );
    }

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

    public availableItemTypes = (): ItemTypePredefined[] => {
        const availableItemTypePredefined = this.filterItemTypePredefinedByParent();
        return [];
    };

    public get hasError(): boolean {
        return Boolean(this._errorMessage);
    }

    public get errorMessage(): string {
        return this._errorMessage;
    }
}
