import { ILocalizationService } from "../../../../core";
import { BaseDialogController, IDialogSettings, IDialogService } from "../../../../shared";
import { Relationships } from "../../../models";
import { IDialogItem } from "../../../models/relationshipModels";
import { ArtifactPickerNodeVM } from "../../bp-artifact-picker/bp-artifact-picker-node-vm";
import {
    IStatefulItem,
    IArtifactManager,
    IArtifactRelationships,
} from "../../../../managers/artifact-manager";

export interface IArtifactSelectedArtifactMap {
    [artifactId: number]: Relationships.IRelationship[];
}

interface IResult {
    found: boolean;
    index: number;
}

export class ManageTracesDialogController extends BaseDialogController {
    public static $inject = ["$uibModalInstance", "dialogSettings", "localization",
        "artifactManager", "artifactRelationships", "dialogData", "dialogService"];

    public  traceDirection: Relationships.TraceDirection = 0;
    public  direction: Relationships.TraceDirection = 0;
    private selectedVMs: ArtifactPickerNodeVM<any>[];

    public item: IStatefulItem;
    public relationshipsList: IArtifactRelationships;
    public manualTraces: any;
    public allTraces: Relationships.IRelationship[];
    public otherTraces: Relationships.IRelationship[];
    public isLoading: boolean = false;
    public isItemReadOnly: boolean;
    public selectedTraces: IArtifactSelectedArtifactMap = {};
    public hasFlagged: boolean = false;
    public hasUnFlagged: boolean = false;
    public artifactId: number;

    public options = [
        { value: "1", label: "To" },
        { value: "2", label: "From" },
        { value: "3", label: "Bidirectional" },
    ];

    constructor($uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
                public dialogSettings: IDialogSettings,
                public localization: ILocalizationService,
                public artifactManager: IArtifactManager,
                public artifactRelationships: IArtifactRelationships,
                public data: IDialogItem,
                private dialogService: IDialogService,
    ) {
        super($uibModalInstance, dialogSettings);
        this.getManualTraces();
        this.clearSelected();
    };

    public $onChanges(changesObj) {
        console.log(changesObj);
    }

    public clearSelected() {
        this.selectedTraces[this.data.artifactId] = [];
    }

    public getManualTraces() {
        if (this.data.manualTraces) {
            this.manualTraces = this.data.manualTraces;
            this.artifactId = this.data.artifactId;
            this.isItemReadOnly = this.data.isItemReadOnly;
        }
    }

    public trace(): void {
        let selected = [];

        for (let i = 0; i < this.selectedVMs.length; i++) {

            let currentItem = this.selectedVMs[i];

            currentItem.model.itemId = currentItem.model.id;
            currentItem.model.artifactId = currentItem.model.id;

            let res = this.inArray(this.manualTraces, currentItem.model);

            if (!res.found) {
                currentItem.model.traceType = Relationships.LinkType.Manual;
                currentItem.model.artifactName = currentItem.model.name;
                currentItem.model.itemName = currentItem.model.name;
                currentItem.model.itemTypePrefix = currentItem.model.prefix;
                currentItem.model.traceDirection = this.direction;
                currentItem.model.projectName = currentItem.model.parent.name;
                currentItem.model.hasAccess = true;
                currentItem.model.suspect = false;
                selected.push(currentItem.model);
            }
        }

        this.manualTraces = this.manualTraces.concat(selected);
        this.data.manualTraces = this.manualTraces;
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
            if (traces[i].hasAccess === true) {
                if (traces[i].suspect === true) {
                    this.hasFlagged = true;
                }else {
                    this.hasUnFlagged = true;
                }
            }
        }

        for (let i = 0; i < selectedTracesLength; i++) {
            if (traces[i].hasAccess === true) {
                if (this.hasFlagged === true && this.hasUnFlagged !== true) {
                    traces[i].suspect = false;
                } else {
                    traces[i].suspect = true;
                }
            }
        }
    }

    public deleteTraces(): void {
        let selectedTracesLength = this.selectedTraces[this.data.artifactId].length;

        let confirmation = this.localization.get("Confirmation_Delete_Traces")
            .replace("{0}", selectedTracesLength.toString());

        this.dialogService.confirm(confirmation).then( (confirmed) => {
            if (confirmed) {
                this.remove(this.selectedTraces[this.data.artifactId], this.manualTraces);
                this.clearSelected();
            }
        });
    }

    public deleteTrace(artifact: Relationships.IRelationship): void {
        this.dialogService.confirm(this.localization.get("Confirmation_Delete_Trace")).then( (confirmed) => {
            if (confirmed) {
                this.remove([artifact], this.manualTraces);
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
    }

    public inArray(array, item) {
        let found = false,
            index = -1;
        if (array) {
            for (let i = 0; i < array.length; i++) {
                if (array[i].itemId === item.itemId) {
                    found = true;
                    index = i;
                    break;
                }
            }
        }

        return <IResult>{ "found": found, "index": index };
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
