﻿import { ILocalizationService } from "../../../core";
import { ISelectionManager, Models, Relationships } from "../../../main";
import { IRelationship, LinkType } from "../../../main/models/relationshipModels";
import { IArtifactRelationships, IArtifactRelationshipsResultSet } from "./artifact-relationships.svc";
import { IBpAccordionPanelController } from "../../../main/components/bp-accordion/bp-accordion";
import { BPBaseUtilityPanelController } from "../bp-base-utility-panel";
import { Helper } from "../../../shared/utils/helper";

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
        "localization",
        "selectionManager",
        "artifactRelationships"
    ];

    private artifactId: number;
    public options: IOptions[];
    public artifactList: IArtifactRelationshipsResultSet;
    public associations: IRelationship[];
    public actorInherits: IRelationship[];
    public documentReferences: IRelationship[];
    public option: string = "1";
    public isLoading: boolean = false;
    public selectedTraces: IArtifactSelectedArtifactMap;

    constructor(
        private localization: ILocalizationService,
        protected selectionManager: ISelectionManager,
        private artifactRelationships: IArtifactRelationships,
        public bpAccordionPanel: IBpAccordionPanelController) {

        super(selectionManager, bpAccordionPanel);

        this.options = [     
            { value: "1", label: "Add new" }           
        ];
    }

    public $onInit() {
        super.$onInit();
    }

    public $onDestroy() {
        super.$onDestroy();   
        this.artifactList = null;
        this.selectedTraces = null;
        this.associations = null;
        this.documentReferences = null;
        this.actorInherits = null;
    }

    protected onSelectionChanged = (artifact: Models.IArtifact, subArtifact: Models.ISubArtifact) => {     
        if (Helper.canUseSelectedArtifact(artifact)) {
            this.artifactId = artifact.id;
            this.getRelationships(artifact.id, subArtifact ? subArtifact.id : null)
                .then((list: any) => {
                    this.artifactList = list;
                    this.selectedTraces = {};
                    this.selectedTraces[this.artifactId] = [];
                    this.populateOtherTraceLists();
                });
        } else {
            this.artifactId = null;
            this.artifactList = null;
        }
    }

    private getRelationships(artifactId, subArtifactId: number = null): ng.IPromise<IArtifactRelationshipsResultSet> {
        this.isLoading = true;
        return this.artifactRelationships.getRelationships(artifactId, subArtifactId)
            .then((list: IArtifactRelationshipsResultSet) => {
                return list;
            })
            .finally(() => {
                this.isLoading = false;
            });
    }

    private populateOtherTraceLists() {
        let associations = new Array<IRelationship>();
        let actorInherits = new Array<IRelationship>();
        let documentReferences = new Array<IRelationship>();

        for (let otherTrace of this.artifactList.otherTraces)
        {
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
