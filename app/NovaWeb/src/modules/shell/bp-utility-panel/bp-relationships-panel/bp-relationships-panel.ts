import { ILocalizationService } from "../../../core";
import { IProjectManager, Relationships, Models} from "../../../main";
import {IArtifactRelationships, IArtifactRelationshipsResultSet} from "./artifact-relationships.svc";

interface IOptions {
    value: string;
    label: string;
}

export class BPRelationshipsPanel implements ng.IComponentOptions {
    public template: string = require("./bp-relationships-panel.html");
    public controller: Function = BPRelationshipsPanelController;
}

export class BPRelationshipsPanelController {
    public static $inject: [string] = [
        "$log",
        "localization",
        "projectManager",
        "artifactRelationships"
    ];

    private artifactId: number;
    private _subscribers: Rx.IDisposable[];
    public options: IOptions[];
    public artifactList: IArtifactRelationshipsResultSet;
    public option: string = "1";
    public isLoading: boolean = false;

    constructor(
        private $log: ng.ILogService,
        private localization: ILocalizationService,
        private projectManager: IProjectManager,
        private artifactRelationships: IArtifactRelationships) {

        this.options = [     
            { value: "1", label: "Add new" }           
        ];
    }

    public $onInit(o) {
        let selectedArtifactSubscriber: Rx.IDisposable = this.projectManager.currentArtifact.subscribe(this.setArtifactId);
        this._subscribers = [selectedArtifactSubscriber];
    }

    public $onDestroy() {
        this._subscribers = this._subscribers.filter((it: Rx.IDisposable) => { it.dispose(); return false; });   
        this.artifactList = null;
    }

    private setArtifactId = (artifact: Models.IArtifactDetails) => {     
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
