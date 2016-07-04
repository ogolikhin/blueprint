import { ILocalizationService } from "../../../core";
import { IProjectManager, Models} from "../../../main";
import {IArtifactRelationships, ITraceType} from "./artifact-relationships.svc";

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
    public artifactList: Models.IArtifactDetails[] = [];
    public option: string = "1";
    public traceTypes = ITraceType;

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
            this.getRelationships(ITraceType.Manual)
                .then((list: any) => {
                    this.artifactList = list;
                });
        }
    }

    private changeTraceType(traceType: ITraceType) {
        this.getRelationships(traceType)
            .then((list: any) => {
                this.artifactList = list;
            });
    }

    private getRelationships(traceType: ITraceType): ng.IPromise<Models.IArtifactDetails[]> {
        return this.artifactRelationships.getRelationships(this.artifactId, traceType)
            .then((list: Models.IArtifactDetails[]) => {
                return list;
            })
            .finally(() => {

            });
    }
}
