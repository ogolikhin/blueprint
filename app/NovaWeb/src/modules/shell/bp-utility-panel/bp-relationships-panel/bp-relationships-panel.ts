import * as angular from "angular";
import * as _ from "lodash";
import {ILocalizationService} from "../../../core";
import {Relationships} from "../../../main";
import {IDialogSettings, IDialogService} from "../../../shared";
import {
    IArtifactManager,
    IStatefulItem,
    IStatefulArtifact,
    IStatefulSubArtifact,
    IArtifactRelationships
} from "../../../managers/artifact-manager";
import {IRelationship, LinkType, IDialogRelationshipItem} from "../../../main/models/relationshipModels";
import {IBpAccordionPanelController} from "../../../main/components/bp-accordion/bp-accordion";
import {BPBaseUtilityPanelController} from "../bp-base-utility-panel";
import {Helper} from "../../../shared/utils/helper";
import {ManageTracesDialogController} from "../../../main/components/dialogs/bp-manage-traces";

interface IOptions {
    value: string;
    label: string;
}

export class BPRelationshipsPanel implements ng.IComponentOptions {
    public template: string = require("./bp-relationships-panel.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BPRelationshipsPanelController;
    public require: any = {
        bpAccordionPanel: "^bpAccordionPanel"
    };
}

export interface IArtifactSelectedArtifactMap {
    [artifactId: number]: Relationships.IRelationship[];
}

export class BPRelationshipsPanelController extends BPBaseUtilityPanelController {
    public static $inject: [string] = [
        "$q",
        "localization",
        "artifactManager",
        "artifactRelationships",
        "dialogService"
    ];

    public item: IStatefulItem;
    public options: IOptions[];
    public manualTraces: Relationships.IRelationship[];
    public allTraces: Relationships.IRelationship[];
    public otherTraces: Relationships.IRelationship[];
    public associations: IRelationship[];
    public actorInherits: IRelationship[];
    public documentReferences: IRelationship[];
    public option: string = "1";
    public isLoading: boolean = false;
    public selectedTraces: IArtifactSelectedArtifactMap;
    public hasFlagged: boolean = false;
    public hasUnFlagged: boolean = false;
    private subscribers: Rx.IDisposable[];

    constructor($q: ng.IQService,
                private localization: ILocalizationService,
                protected artifactManager: IArtifactManager,
                private artifactRelationships: IArtifactRelationships,
                private dialogService: IDialogService,
                public bpAccordionPanel: IBpAccordionPanelController) {

        super($q, artifactManager.selection, bpAccordionPanel);

        this.options = [
            {value: "1", label: "Add new"}
        ];

        this.subscribers = [];
    }

    public $onInit() {
        super.$onInit();
    }

    public $onDestroy() {
        super.$onDestroy();
        this.manualTraces = null;
        this.otherTraces = null;
        this.selectedTraces = null;
        this.associations = null;
        this.documentReferences = null;
        this.actorInherits = null;
    }

    protected onSelectionChanged(artifact: IStatefulArtifact, subArtifact: IStatefulSubArtifact,
                                 timeout: ng.IPromise<void>): ng.IPromise<any> {
        this.hasFlagged = false;
        this.hasUnFlagged = false;

        this.subscribers = this.subscribers.filter(subscriber => {
            subscriber.dispose();
            return false;
        });

        this.item = subArtifact || artifact;
        this.getRelationships();

        if (this.item) {
            const relationshipSubscriber = this.item.relationships.getObservable().subscribe(this.onRelationshipUpdate);
            this.subscribers.push(relationshipSubscriber);
        }

        return super.onSelectionChanged(artifact, subArtifact, timeout);
    }

    public onRelationshipUpdate = (relationships: Relationships.IRelationship[]) => {
        this.setRelationships(relationships);
    }

    public get manualTraces2(): Relationships.IRelationship[] {
        if (this.allTraces) {
            return this.allTraces.filter((relationship: Relationships.IRelationship) =>
            relationship.traceType === Relationships.LinkType.Manual);
        } else {
            return [];
        }
    }


    private getRelationships(refresh?: boolean) {
        this.manualTraces = null;
        this.otherTraces = null;

        if (this.item && Helper.hasArtifactEverBeenSavedOrPublished(this.item)) {
            this.isLoading = true;
            this.item.relationships.get(refresh).then((relationships: Relationships.IRelationship[]) => {
                this.setRelationships(relationships);

                this.selectedTraces = {};
                this.selectedTraces[this.item.id] = [];
                this.populateOtherTraceLists();

                return relationships;
            }).finally(() => {
                this.isLoading = false;
            });
        } else {
            this.reset();
        }
    }

    public setRelationships(relationships: Relationships.IRelationship[]) {
        this.allTraces = relationships;
        this.manualTraces = relationships
            .filter((relationship: Relationships.IRelationship) =>
            relationship.traceType === Relationships.LinkType.Manual);
        this.otherTraces = relationships
            .filter((relationship: Relationships.IRelationship) =>
            relationship.traceType !== Relationships.LinkType.Manual);
    }

    private reset() {
        this.otherTraces = [];
        this.allTraces = [];
        this.manualTraces = [];
        this.associations = [];
        this.actorInherits = [];
        this.documentReferences = [];
    }

    public canManageTraces(): boolean {
        // if artifact is locked by other user we still can add/manage traces
        return !this.item.artifactState.deleted &&
            this.item.relationships.canEdit;
    }

    public setSelectedDirection(direction: Relationships.TraceDirection): void {
        let traces = this.selectedTraces[this.item.id],
            selectedTracesLength = traces.length;

        for (let i = 0; i < selectedTracesLength; i++) {
            if (traces[i].hasAccess === true) {
                traces[i].traceDirection = direction;
            }
        }
    }

    public toggleFlag() {
        //2 flags to change state of the flag icon on the frontend;
        this.hasFlagged = false;
        this.hasUnFlagged = false;

        let traces = this.selectedTraces[this.item.id],
            selectedTracesLength = traces.length;

        for (let i = 0; i < selectedTracesLength; i++) {
            if (traces[i].hasAccess === true) {
                if (traces[i].suspect === true) {
                    this.hasFlagged = true;
                } else {
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

    public deleteTraces(artifacts: Relationships.IRelationship[]): void {
        let selectedTracesLength = this.selectedTraces[this.item.id].length;

        let confirmation = this.localization.get("Confirmation_Delete_Traces")
            .replace("{0}", selectedTracesLength.toString());

        this.dialogService.confirm(confirmation).then((confirmed) => {
            if (confirmed) {
                this.item.relationships.remove(artifacts);

                this.selectedTraces[this.item.id].length = 0;
            }
        });
    }

    public deleteTrace(artifact: Relationships.IRelationship): void {
        this.dialogService.confirm(this.localization.get("Confirmation_Delete_Trace")).then((confirmed) => {
            if (confirmed) {
                this.item.relationships.remove([artifact]);
                this.getRelationships(false);
            }
        });
    }

    public setItemDirection(trace: IRelationship): void { 
        // this is called after we update direction. all it does is trigger dirty/save state.
        this.item.relationships.updateManual(this.manualTraces);
    }

    public toggleItemFlag(trace: IRelationship) {
        if (trace.hasAccess) {
            trace.suspect = trace.suspect === true ? false : true;
            this.item.relationships.updateManual(this.manualTraces);
        }
    }

    public openManageTraces() {
        const dialogSettings: IDialogSettings = {
            okButton: this.localization.get("App_Button_Ok"),
            template: require("../../../main/components/dialogs/bp-manage-traces/bp-manage-traces.html"),
            controller: ManageTracesDialogController,
            css: "nova-open-project manage-traces-wrapper",
            header: this.localization.get("App_UP_Relationships_Manage_Traces")
        };

        let data: IDialogRelationshipItem = {
            manualTraces: angular.copy(this.manualTraces2),
            artifactId: this.item.id,
            isItemReadOnly: false
        };

        this.dialogService.open(dialogSettings, data).then((result) => {

            data.manualTraces = data.manualTraces.map((trace) => {
                trace.isSelected = false;
                return trace;
            });

            this.manualTraces = data.manualTraces;
            this.item.relationships.updateManual(data.manualTraces);
            this.getRelationships(false);
        });
    }

    private populateOtherTraceLists() {
        const associations = new Array<IRelationship>();
        const actorInherits = new Array<IRelationship>();
        const documentReferences = new Array<IRelationship>();

        for (const otherTrace of this.otherTraces) {
            if (otherTrace.traceType === LinkType.Association) {
                associations.push(otherTrace);
            } else if (otherTrace.traceType === LinkType.ActorInheritsFrom) {
                actorInherits.push(otherTrace);
            } else if (otherTrace.traceType === LinkType.DocumentReference) {
                documentReferences.push(otherTrace);
            }
        }
        this.associations = associations;
        this.actorInherits = actorInherits;
        this.documentReferences = documentReferences;
    }
}
