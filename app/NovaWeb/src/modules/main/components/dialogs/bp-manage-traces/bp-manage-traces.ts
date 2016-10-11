import * as _ from "lodash";
import { ILocalizationService } from "../../../../core";
import { BaseDialogController, IDialogSettings, IDialogService, Helper } from "../../../../shared";
import { Relationships, Models } from "../../../models";
import { IDialogRelationshipItem } from "../../../models/relationshipModels";
import { ArtifactPickerNodeVM, ArtifactNodeVM } from "../../bp-artifact-picker/bp-artifact-picker-node-vm";
import {
    IStatefulItem,
    IArtifactManager,
    IArtifactRelationships,
} from "../../../../managers/artifact-manager";

export interface IArtifactSelectedArtifactMap {
    [artifactId: number]: Relationships.IRelationship[];
}

export class ManageTracesDialogController extends BaseDialogController {
    public static $inject = ["$uibModalInstance", "dialogSettings", "localization",
        "artifactManager", "artifactRelationships", "dialogData", "dialogService"];

    public  traceDirection: Relationships.TraceDirection = 0;
    public  direction: Relationships.TraceDirection = 0;
    private selectedVMs: ArtifactPickerNodeVM<any>[];

    public item: IStatefulItem;
    public relationshipsList: IArtifactRelationships;
    public manualTraces: Relationships.IRelationshipView[];
    public allTraces: Relationships.IRelationship[];
    public otherTraces: Relationships.IRelationship[];
    public isLoading: boolean = false;
    public isItemReadOnly: boolean;
    public selectedTraces: IArtifactSelectedArtifactMap = {};
    public hasFlagged: boolean = false;
    public hasUnFlagged: boolean = false;
    public artifactId: number;
    public isChanged: boolean = false;

    public options = [
        { value: "1", label: this.localization.get("App_UP_Relationships_To") },
        { value: "2", label: this.localization.get("App_UP_Relationships_From") },
        { value: "3", label: this.localization.get("App_UP_Relationships_Bidirectional") },
    ];

    constructor($uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
                public dialogSettings: IDialogSettings,
                public localization: ILocalizationService,
                public artifactManager: IArtifactManager,
                public artifactRelationships: IArtifactRelationships,
                public data: IDialogRelationshipItem,
                private dialogService: IDialogService,
    ) {
        super($uibModalInstance, dialogSettings);
        this.getManualTraces();
        this.clearSelected();
    };

    public clearSelected() {
        if (this.selectedTraces[this.data.artifactId]) {
            this.selectedTraces[this.data.artifactId].forEach( (item) => {
                item.isSelected = false;
            });
        }

        this.selectedTraces[this.data.artifactId] = [];
    }

    public updateTraces() {
        this.data.manualTraces = this.manualTraces;
    }

    public getManualTraces() {
        if (this.data.manualTraces) {
            this.manualTraces = (this.data.manualTraces.map( (item: Relationships.IRelationshipView) => {
                let typeName = Models.ItemTypePredefined[item.primitiveItemTypePredefined];

                if (typeName) {
                    item.cssClass = "icon-" + _.kebabCase(typeName);
                }

                return item;
            })) as Relationships.IRelationshipView[];

            this.artifactId = this.data.artifactId;
            this.isItemReadOnly = this.data.isItemReadOnly;
        }
    }

    public trace(): void {
        let selected = [],
            selectedVMs = this.selectedVMs,
            selectedVMsLength = selectedVMs.length;

        for (let i = 0; i < selectedVMsLength; i++) {

            let currentItem = selectedVMs[i],
                currentItemModel = currentItem.model;

            currentItemModel.itemId = currentItemModel.id;

            currentItemModel.artifactId = currentItem instanceof ArtifactNodeVM ? currentItemModel.id : currentItemModel.parentId;

            let res = _.find(this.manualTraces, {itemId: currentItemModel.itemId});

            let typeName = Models.ItemTypePredefined[currentItemModel.predefinedType];

            let cssClass;

            if (typeName) {
                cssClass = "icon-" + _.kebabCase(typeName);
            }

            if (!res) {
                currentItemModel.traceType = Relationships.LinkType.Manual;
                currentItemModel.artifactName = currentItemModel.name || currentItemModel.displayName;
                currentItemModel.itemName = currentItemModel.name || currentItemModel.displayName;
                currentItemModel.itemTypePrefix = currentItemModel.prefix;
                currentItemModel.traceDirection = this.direction;
                currentItemModel.projectName = currentItemModel.parent ? currentItemModel.parent.name :
                    currentItem["options"].project.name;
                currentItemModel.hasAccess = true;
                currentItemModel.suspect = false;
                currentItemModel.cssClass = cssClass;
                selected.push(currentItemModel);
            }
        }

        this.manualTraces = this.manualTraces.concat(selected);
        this.updateTraces();
    }


    public setTraceDirection(direction: Relationships.TraceDirection): void {
        this.traceDirection = direction;
    }

    public setDirection(direction: Relationships.TraceDirection): void {
        this.direction = direction;
    }

    public toggleTraces(artifacts: Relationships.IRelationship[]): void {
        //2 flags to change state of the flag icon on the frontend;
        this.hasFlagged = false;
        this.hasUnFlagged = false;

        let traces = this.selectedTraces[this.data.artifactId],
            selectedTracesLength = traces.length;

        for (let i = 0; i < selectedTracesLength; i++) {
            if (traces[i].hasAccess) {
                if (traces[i].suspect === true) {
                    this.hasFlagged = true;
                }else {
                    this.hasUnFlagged = true;
                }
            }
        }

        for (let i = 0; i < selectedTracesLength; i++) {
            if (traces[i].hasAccess) {
                if (this.hasFlagged === true && this.hasUnFlagged !== true) {
                    traces[i].suspect = false;
                } else {
                    traces[i].suspect = true;
                }
            }
        }
        this.updateTraces();
    }

    public deleteTraces(): void {
        let selectedTracesLength = this.selectedTraces[this.data.artifactId].length;

        let confirmation = this.localization.get("Confirmation_Delete_Traces")
            .replace("{0}", selectedTracesLength.toString());

        this.dialogService.confirm(confirmation).then( (confirmed) => {
            if (confirmed) {
                this.remove(this.selectedTraces[this.data.artifactId], this.manualTraces);
                this.updateTraces();
                this.clearSelected();
            }
        });
    }

    public deleteTrace(artifact: Relationships.IRelationship): void {
        this.dialogService.confirm(this.localization.get("Confirmation_Delete_Trace")).then( (confirmed) => {
            if (confirmed) {
                this.remove([artifact], this.manualTraces);

                let index = _.findIndex(this.selectedTraces[this.data.artifactId], {itemId: artifact.itemId});

                if (index > -1) {
                    this.selectedTraces[this.data.artifactId].splice(index, 1);
                }

                this.updateTraces();
            }
        });
    }

    public onSelectionChanged(selectedVMs: ArtifactPickerNodeVM<any>[]): void {
        this.selectedVMs = selectedVMs;
    }

    public setSelectedDirection(direction: Relationships.TraceDirection): void {
        let traces = this.selectedTraces[this.data.artifactId],
            selectedTracesLength = traces.length;

        for (let i = 0; i < selectedTracesLength; i++) {
                traces[i].traceDirection = direction;
        }
        this.updateTraces();
    }

    public remove(relationships: Relationships.IRelationship[],
                  traces: Relationships.IRelationship[]): Relationships.IRelationship[] {
        if (relationships) {
            relationships.forEach((relationship: Relationships.IRelationship) => {
                const foundRelationshipIndex = traces.indexOf(relationship);

                if (foundRelationshipIndex > -1) {
                    traces.splice(foundRelationshipIndex, 1);
                }
            });
        }

        return traces;
    }
}
