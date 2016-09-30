import { ILocalizationService } from "../../../core";
import { Relationships } from "../../../main";
import { 
    IArtifactManager, 
    IStatefulItem,
    IStatefulArtifact, 
    IStatefulSubArtifact, 
    IArtifactRelationships
} from "../../../managers/artifact-manager";
import { IRelationship, LinkType } from "../../../main/models/relationshipModels";
// import { IArtifactRelationships, IArtifactRelationshipsResultSet } from "./artifact-relationships.svc";
import { IBpAccordionPanelController } from "../../../main/components/bp-accordion/bp-accordion";
import { BPBaseUtilityPanelController } from "../bp-base-utility-panel";
// import { Helper } from "../../../shared/utils/helper";

interface IOptions {
    value: string;
    label: string;
}

export class BPRelationshipsPanel implements ng.IComponentOptions {
    public template: string = require("./bp-relationships-panel.html");
    public controller: Function = BPRelationshipsPanelController;
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
        "artifactRelationships"
    ];

    public item: IStatefulItem;
    public options: IOptions[];
    public manualTraces: Relationships.IRelationship[];
    public otherTraces: Relationships.IRelationship[];
    public associations: IRelationship[];
    public actorInherits: IRelationship[];
    public documentReferences: IRelationship[];
    public option: string = "1";
    public isLoading: boolean = false;
    public selectedTraces: IArtifactSelectedArtifactMap;
    public isItemReadOnly: boolean;
    public canCreate: boolean = false;

    constructor(
        $q: ng.IQService,
        private localization: ILocalizationService,
        protected artifactManager: IArtifactManager,
        private artifactRelationships: IArtifactRelationships,
        public bpAccordionPanel: IBpAccordionPanelController) {

        super($q, artifactManager.selection, bpAccordionPanel);

        this.options = [     
            { value: "1", label: "Add new" }           
        ];
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

    protected onSelectionChanged (artifact: IStatefulArtifact, subArtifact: IStatefulSubArtifact, timeout: ng.IPromise<void>): ng.IPromise<any> {
        this.item = subArtifact || artifact;
        this.getRelationships();

        return super.onSelectionChanged(artifact, subArtifact, timeout);
    }

    private getRelationships() {
        this.manualTraces = null;
        this.otherTraces = null;

        if (this.item) {
            this.isLoading = true;            
            this.item.relationships.get().then((relationships: Relationships.IRelationship[]) => {
                this.manualTraces = relationships
                    .filter((relationship: Relationships.IRelationship) => 
                        relationship.traceType === Relationships.LinkType.Manual);
                this.otherTraces = relationships
                    .filter((relationship: Relationships.IRelationship) => 
                        relationship.traceType !== Relationships.LinkType.Manual);

                this.selectedTraces = {};
                this.selectedTraces[this.item.id] = [];
                this.populateOtherTraceLists();

                return relationships;
            }).finally(() => {
                this.isItemReadOnly = this.item.artifactState.readonly || this.item.deleted || !this.item.relationships.canEdit;
                this.isLoading = false;
            });
        }
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
