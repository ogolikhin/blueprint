import { ILocalizationService } from "../../../core";
import { IProjectManager, Relationships, Models} from "../../../main";
import {IArtifactRelationships} from "./artifact-relationships.svc";

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
    public artifactList: Relationships.Relationship[] = [];
    public option: string = "1";
    public traceTypes = Relationships.ITraceType;
    public currentTraceType = Relationships.ITraceType.Manual;

    constructor(
        private $log: ng.ILogService,
        private localization: ILocalizationService,
        private projectManager: IProjectManager,
        private artifactRelationships: IArtifactRelationships) {

        this.options = [     
            { value: "1", label: "Add new" }           
        ];
    }

    //all subscribers need to be created here in order to unsubscribe (dispose) them later on component destroy life circle step
    public $onInit(o) {
        let selectedArtifactSubscriber: Rx.IDisposable = this.projectManager.currentArtifact.subscribe(this.setArtifactId);
        this._subscribers = [selectedArtifactSubscriber];
    }

    public $onDestroy() {
        //dispose all subscribers
        this._subscribers = this._subscribers.filter((it: Rx.IDisposable) => { it.dispose(); return false; });
        // clean up history list
        this.artifactList = null;
    }

    private setArtifactId = (artifact: Models.IArtifactDetails) => {
        this.artifactList = [];

        if (artifact !== null) {
            this.artifactId = artifact.id;
            this.getRelationships(Relationships.ITraceType.Manual)
                .then((list: any) => {
                    this.artifactList = list;
                    this.currentTraceType = Relationships.ITraceType.Manual;
                  
                });
        }
    }

    //public setActive() {
    //    return this.currentTraceType == type;
    //}

    private changeTraceType(traceType: Relationships.ITraceType) {
        this.getRelationships(traceType)
            .then((list: any) => {
                this.artifactList = list;
                this.currentTraceType = traceType;
            });
    }

    private getRelationships(traceType: Relationships.ITraceType): ng.IPromise<Relationships.Relationship[]> {
        return this.artifactRelationships.getRelationships(this.artifactId, traceType)
            .then((list: Relationships.Relationship[]) => {
                return list;
            })
            .finally(() => {

            });
    }
}
