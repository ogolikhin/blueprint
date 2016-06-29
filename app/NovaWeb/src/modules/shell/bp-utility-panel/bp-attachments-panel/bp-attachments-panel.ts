import { ILocalizationService } from "../../../core";
import { IProjectManager, Models} from "../../../main";
import { IArtifactAttachmentsResultSet, IArtifactAttachments } from "./artifact-attachments.svc";

interface IAddOptions {
    value: string;
    label: string;
}

export class BPAttachmentsPanel implements ng.IComponentOptions {
    public template: string = require("./bp-attachments-panel.html");
    public controller: Function = BPAttachmentsPanelController;
}

export class BPAttachmentsPanelController {
    public static $inject: [string] = [
        "$log", 
        "localization",
        "projectManager",
        "artifactAttachments"
    ];

    private _subscribers: Rx.IDisposable[];

    public artifactAttachmentsList: IArtifactAttachmentsResultSet;
    public addOptions: IAddOptions[];
    public categoryFilter: number;
    public isLoading: boolean = false;
    
    constructor(
        private $log: ng.ILogService,
        private localization: ILocalizationService,
        private projectManager: IProjectManager,
        private artifactAttachments: IArtifactAttachments) {

        this.addOptions = [
            { value: "attachment", label: this.localization.get("App_UP_Attachments_Add_Attachment") },
            { value: "document", label: this.localization.get("App_UP_Attachments_Add_Ref_Doc") },
        ];
    }

    //all subscribers need to be created here in order to unsubscribe (dispose) them later on component destroy life circle step
    public $onInit(o) {
        let selectedArtifactSubscriber: Rx.IDisposable = this.projectManager.currentArtifact
            .distinctUntilChanged()
            .asObservable()
            .subscribe(this.setArtifactId);

        this._subscribers = [ selectedArtifactSubscriber ];
    }

    public $onDestroy() {
        //dispose all subscribers
        this._subscribers = this._subscribers.filter((it: Rx.IDisposable) => { it.dispose(); return false; });
    }

    private setArtifactId = (artifact: Models.IArtifact) => {
        this.artifactAttachmentsList = null;

        if (artifact !== null) {
            this.getAttachments(artifact.id)
                .then( (result: IArtifactAttachmentsResultSet) => {
                    this.artifactAttachmentsList = result;
                });
        }
    }

    private getAttachments(artifactId: number, subArtifactId: number = null): ng.IPromise<IArtifactAttachmentsResultSet> {
        this.isLoading = true;
        return this.artifactAttachments.getArtifactAttachments(artifactId)
            .then( (result: IArtifactAttachmentsResultSet) => {
                return result;
            })
            .finally( () => {
                this.isLoading = false;
            });
    }
}
