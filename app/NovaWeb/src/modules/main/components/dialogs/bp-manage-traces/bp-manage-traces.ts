import * as _ from "lodash";
import {BaseDialogController, IDialogSettings, IDialogService} from "../../../../shared";
import {IArtifactPickerAPI} from "../../../../main/components/bp-artifact-picker/bp-artifact-picker";
import {Relationships, Models, AdminStoreModels, TreeModels} from "../../../models";
import {IDialogRelationshipItem} from "../../../models/relationshipModels";
import {IStatefulItem, IArtifactRelationships} from "../../../../managers/artifact-manager";
import {ILocalizationService} from "../../../../commonModule/localization/localization.service";

export interface IArtifactSelectedArtifactMap {
    [artifactId: number]: Relationships.IRelationshipView[];
}

interface ISelectedItem extends TreeModels.ITreeNodeVM<any> {
    parentArtifact?: Models.IArtifact;
    project?: any;
}

export class ManageTracesDialogController extends BaseDialogController {
    public static $inject = ["$uibModalInstance", "dialogSettings", "localization",
        "artifactRelationships", "dialogData", "dialogService", "$timeout"];

    public traceDirection: Relationships.TraceDirection = 0;
    public direction: Relationships.TraceDirection = 0;
    private selectedVMs: TreeModels.ITreeNodeVM<any>[];

    public item: IStatefulItem;
    public otherTraces: Relationships.IRelationshipView[];
    /*fixme: what is this variable used for?*/
    public scroller;
    public isLoading: boolean = false;
    public isItemReadOnly: boolean;
    public isTraceDisabled: boolean;
    public selectedTraces: IArtifactSelectedArtifactMap = {};
    public hasFlagged: boolean = false;
    public hasUnFlagged: boolean = false;
    public artifactId: number;
    public isChanged: boolean = false;
    public disabledSave: boolean = true;
    public initialArray: any[];
    public api: IArtifactPickerAPI;

    public options = [
        {value: "1", label: this.localization.get("App_UP_Relationships_To")},
        {value: "2", label: this.localization.get("App_UP_Relationships_From")},
        {value: "3", label: this.localization.get("App_UP_Relationships_Bidirectional")}
    ];

    constructor($uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
                public dialogSettings: IDialogSettings,
                public localization: ILocalizationService,
                public artifactRelationships: IArtifactRelationships,
                public data: IDialogRelationshipItem,
                private dialogService: IDialogService,
                private $timeout: ng.ITimeoutService) {

        super($uibModalInstance, dialogSettings);
        this.getManualTraces();
        this.clearSelected();
    };

    public clearSelected() {
        if (this.selectedTraces[this.data.artifactId]) {
            this.selectedTraces[this.data.artifactId].forEach((item) => {
                item.isSelected = false;
            });
        }
        this.selectedTraces[this.data.artifactId] = [];
    }

    public getManualTraces() {
        if (this.data.manualTraces) {
            this.data.manualTraces = (this.data.manualTraces.map((item: Relationships.IRelationshipView) => {
                let typeName = Models.ItemTypePredefined[item.primitiveItemTypePredefined];

                if (typeName) {
                    item.cssClass = "icon-" + _.kebabCase(typeName);
                }

                item.isSelected = false;

                return item;
            }));

            this.artifactId = this.data.artifactId;
            this.isItemReadOnly = this.data.isItemReadOnly;

            this.initialArray = _.map(this.data.manualTraces, (trace) => {
                return _.pick(trace, ["artifactId", "suspect", "traceDirection"]);
            });
        }
    }

    public toggleSave() {
        const lastVersion = _.map(this.data.manualTraces, (trace) => {
            return _.pick(trace, ["artifactId", "suspect", "traceDirection"]);
        });

        this.disabledSave = _.isEqual(this.initialArray, lastVersion);
    }

    public trace(): void {
        let selected = [],
            selectedVMs = this.selectedVMs,
            selectedVMsLength = selectedVMs.length;

        for (let i = 0; i < selectedVMsLength; i++) {

            let currentItem = selectedVMs[i] as ISelectedItem,
                currentItemModel = (currentItem.model) as Relationships.IRelationshipView;

            currentItemModel.itemId = currentItemModel.id;
            currentItemModel.artifactId = currentItem instanceof TreeModels.ArtifactNodeVM ? currentItemModel.id : currentItemModel.parentId;

            let res = _.find(this.data.manualTraces, {itemId: currentItemModel.itemId});

            let typeName = Models.ItemTypePredefined[currentItemModel.predefinedType];

            let cssClass;

            if (typeName) {
                cssClass = "icon-" + _.kebabCase(typeName);
            }

            if (!res) {
                currentItemModel.traceType = Relationships.LinkType.Manual;
                currentItemModel.itemName = currentItemModel.name || currentItemModel.displayName || currentItemModel.itemLabel;
                currentItemModel.itemTypePrefix = currentItemModel.prefix;
                currentItemModel.traceDirection = this.direction;
                currentItemModel.projectName = currentItem.project && currentItem.project.name;
                currentItemModel.hasAccess = true;
                currentItemModel.suspect = false;
                currentItemModel.cssClass = cssClass;
                selected.push(currentItemModel);

                if (currentItem.parentArtifact) {
                    currentItemModel.artifactTypePrefix = currentItem.parentArtifact.prefix;
                    currentItemModel.artifactId = currentItem.parentArtifact.id;
                    currentItemModel.artifactName = currentItem.parentArtifact.name;
                }
            }
        }

        this.data.manualTraces = this.data.manualTraces.concat(selected);

        this.$timeout(() => {
            this.scroller = document.getElementById("trace-manager-wrapper");
            this.scroller.scrollTop = this.scroller.scrollHeight;
        });

        this.api.deselectAll();
        this.disableTrace();
        this.toggleSave();
    }

    public setDirection(direction: Relationships.TraceDirection): void {
        this.direction = direction;
        this.toggleSave();
    }

    public toggleTraces(): void {
        //2 flags to change state of the flag icon on the frontend;
        this.hasFlagged = false;
        this.hasUnFlagged = false;

        let traces = this.selectedTraces[this.data.artifactId];
        let selectedTracesLength = traces.length;

        for (let i = 0; i < selectedTracesLength; i++) {
            if (traces[i].hasAccess) {
                if (traces[i].suspect === true) {
                    this.hasFlagged = true;
                } else {
                    this.hasUnFlagged = true;
                }
            }
        }

        for (let i = 0; i < selectedTracesLength; i++) {
            if (traces[i].hasAccess) {
                traces[i].suspect = !(this.hasFlagged === true && this.hasUnFlagged !== true);
                traces[i].traceIcon = traces[i].suspect ? "trace-icon-suspect" : "trace-icon-regular";
            }
        }

        this.toggleSave();
    }

    public deleteTraces(): void {
        let selectedTracesLength = this.selectedTraces[this.data.artifactId].length;

        let confirmation = this.localization.get("Confirmation_Delete_Traces")
            .replace("{0}", selectedTracesLength.toString());

        this.dialogService.confirm(confirmation)
            .then(() => {
                this.remove(this.selectedTraces[this.data.artifactId], this.data.manualTraces);
                this.clearSelected();
                this.toggleSave();
            });
    }

    public deleteTrace(artifact: Relationships.IRelationship): void {
        this.dialogService.confirm(this.localization.get("Confirmation_Delete_Trace")).then(() => {
            this.remove([artifact], this.data.manualTraces);

            let index = _.findIndex(this.selectedTraces[this.data.artifactId], {itemId: artifact.itemId});

            if (index > -1) {
                this.selectedTraces[this.data.artifactId].splice(index, 1);
            }

            this.toggleSave();
        });
    }

    public onSelectionChanged(selectedVMs: TreeModels.ITreeNodeVM<any>[]): void {
        this.selectedVMs = selectedVMs;

        this.disableTrace();
        this.toggleSave();
    }

    /**
     * Disable trace button if selected artfiact is a folder of artifact already in manual traces or
     * selected artifact is the same as current artifact
     */
    private disableTrace() {
        this.isTraceDisabled = false;

        if (_.find(this.selectedVMs, (o) => {
                return o.model.id === this.data.artifactId || o.model.type === AdminStoreModels.InstanceItemType.Folder;
            })) {
            this.isTraceDisabled = true;
            return false;
        }

        _.each(this.data.manualTraces, (trace) => {
            if (_.find(this.selectedVMs, (o) => {
                    return o.model.id === trace.itemId;
                })) {
                this.isTraceDisabled = true;
                return false;
            }
        });
    }

    public setSelectedDirection(direction: Relationships.TraceDirection): void {
        let traces = this.selectedTraces[this.data.artifactId],
            selectedTracesLength = traces.length;

        for (let i = 0; i < selectedTracesLength; i++) {
            traces[i].traceDirection = direction;
            traces[i].directionIcon = this.getDirectionIcon(traces[i].traceDirection);
        }

        this.toggleSave();
    }

    public toggleFlag(artifact: Relationships.IRelationshipView) {
        if (artifact.hasAccess) {
            artifact.suspect = !artifact.suspect;
            artifact.traceIcon = artifact.suspect ? "trace-icon-suspect" : "trace-icon-regular";

            this.toggleSave();
        }
    }

    public setTraceDirection(artifact: Relationships.IRelationshipView): void {
        if (artifact.hasAccess) {
            artifact.directionIcon = this.getDirectionIcon(artifact.traceDirection);
            this.toggleSave();
        }
    }

    private remove(relationships: Relationships.IRelationship[],
                   traces: Relationships.IRelationship[]): Relationships.IRelationship[] {
        if (relationships) {
            relationships.forEach((relationship: Relationships.IRelationship) => {
                const foundRelationshipIndex = traces.indexOf(relationship);

                if (foundRelationshipIndex > -1) {
                    traces.splice(foundRelationshipIndex, 1);
                }
            });
        }
        this.disableTrace();
        return traces;
    }

    public getDirectionIcon(direction: Relationships.TraceDirection) {
        let icon = "fonticon2-relationship-";

        switch (direction) {
            case 0:
                icon += "right";
                break;
            case 1:
                icon += "left";
                break;
            case 2:
                icon += "bi";
                break;
            default:
                icon += "right";
                break;
        }

        return icon;
    }
}
