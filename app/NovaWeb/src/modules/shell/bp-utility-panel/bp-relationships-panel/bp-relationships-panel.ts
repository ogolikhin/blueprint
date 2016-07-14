import { ILocalizationService } from "../../../core";
import { IProjectManager, Relationships, Models} from "../../../main";
import {IArtifactRelationships, IArtifactRelationshipsResultSet} from "./artifact-relationships.svc";
import { IBpAccordionPanelController } from "../../../main/components/bp-accordion/bp-accordion";
import { BPBaseUtilityPanelController } from "../bp-base-utility-panel";

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

export class BPRelationshipsPanelController extends BPBaseUtilityPanelController {
    public static $inject: [string] = [
        "localization",
        "projectManager",
        "artifactRelationships"
    ];

    private artifactId: number;
    public options: IOptions[];
    public artifactList: IArtifactRelationshipsResultSet;
    public option: string = "1";
    public isLoading: boolean = false;

    constructor(
        private localization: ILocalizationService,
        protected projectManager: IProjectManager,
        private artifactRelationships: IArtifactRelationships,
        public bpAccordionPanel: IBpAccordionPanelController) {

        super(projectManager, bpAccordionPanel);

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
    }

    protected setArtifactId = (artifact: Models.IArtifact) => {     
        if (artifact !== null) {
            this.artifactId = artifact.id;
            this.getRelationships()
                .then((list: any) => {
                    this.artifactList = list;
                                     
                });
        }
    }

    private getRelationships(): ng.IPromise<IArtifactRelationshipsResultSet> {
        this.isLoading = true;
        return this.artifactRelationships.getRelationships(this.artifactId)
            .then((list: IArtifactRelationshipsResultSet) => {
                return list;
            })
            .finally(() => {
                this.isLoading = false;
            });
    }
}
