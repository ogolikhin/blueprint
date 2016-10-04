import { ILocalizationService } from "../../../../core";
import { BaseDialogController, IDialogSettings, IDialogData } from "../../../../shared";
import { Relationships } from "../../../models";
import { ArtifactPickerNodeVM } from "../../bp-artifact-picker/bp-artifact-picker-node-vm";
import {
    IStatefulItem,
    IArtifactManager,
    IArtifactRelationships,
} from "../../../../managers/artifact-manager";

export interface IArtifactSelectedArtifactMap {
    [artifactId: number]: Relationships.IRelationship[];
}

export interface IDialogItem {
    item: IStatefulItem;
}

export class ManageTracesDialogController extends BaseDialogController {
    public static $inject = ["$uibModalInstance", "dialogSettings", "localization",
        "artifactManager", "artifactRelationships", "dialogData"];

    public  traceDirection: Relationships.TraceDirection = 0;
    private selectedVMs: ArtifactPickerNodeVM<any>[];

    public item: IStatefulItem;
    public manualTraces: Relationships.IRelationship[];
    public allTraces: Relationships.IRelationship[];
    public otherTraces: Relationships.IRelationship[];
    public isLoading: boolean = false;
    public isItemReadOnly: boolean;
    public selectedTraces: IArtifactSelectedArtifactMap;

    constructor($uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
                public dialogSettings: IDialogSettings,
                public localization: ILocalizationService,
                public artifactManager: IArtifactManager,
                public artifactRelationships: IArtifactRelationships,
                public dialogData: IDialogItem
    ) {
        super($uibModalInstance, dialogSettings);
        this.tracesArray();
    };

    public tracesArray() {
        if (this.dialogData.item) {
            this.dialogData.item.relationships.get().then((relationships: Relationships.IRelationship[]) => {
                this.manualTraces = relationships
                    .filter((relationship: Relationships.IRelationship) =>
                    relationship.traceType === Relationships.LinkType.Manual);
                return relationships;
            });
        } else {
            return [];
        }
    }

    public trace(): void {
        let selected = [];

        for (let i = 0; i < this.selectedVMs.length; i++) {
            this.selectedVMs[i].model.traceType = Relationships.LinkType.Manual;
            selected.push(this.selectedVMs[i].model);
        }

        this.dialogData.item.relationships.relationships = this.dialogData.item.relationships.relationships.concat(selected);
    }

    public setTraceDirection(direction: Relationships.TraceDirection): void {
        this.traceDirection = direction;
    }

    public toggleTraces(artifacts: Relationships.IRelationship[]): void {
        alert("run fn toggleTraces");
    }

    public deleteTraces(artifacts: Relationships.IRelationship[]): void {
        alert("run fn deleteTraces");
    }

    public deleteTrace(artifacts: Relationships.IRelationship): void {
        alert("run fn deleteTrace");
    }

    public onSelectionChanged(selectedVMs: ArtifactPickerNodeVM<any>[]): void {
        this.selectedVMs = selectedVMs;
    }
}
