import {IProjectManager, Models} from "../..";

export class BPContentSelector implements ng.IComponentOptions {
    public template: string = require("./bp-content-selector.html");
    public controller: Function = BPContentSelectorController;
}


export class BPContentSelectorController {
    public static $inject: [string] = [
        "projectManager"
    ];

    public contentType: string = "details";

    private subscribers: Rx.IDisposable[];

    constructor(private projectManager: IProjectManager) {
    }

    public $onInit() {
        //use context reference as the last parameter on subscribe...
        this.subscribers = [
            //subscribe for current artifact change (need to distinct artifact)
            this.projectManager.currentArtifact.subscribeOnNext(this.selectView, this),
        ];
    }

    public $onDestroy() {
        //dispose all subscribers
        this.subscribers = this.subscribers.filter((it: Rx.IDisposable) => { it.dispose(); return false; });
    }

    private selectView(artifact: Models.IArtifactDetails) {
        if (!artifact) {
            return;
        }
        this.contentType = this.getContentType(artifact);
    }

    private getContentType(artifact: Models.IArtifact): string {
        switch (artifact.predefinedType) {
            case <Models.ArtifactTypeEnum>4108:
            case <Models.ArtifactTypeEnum>4105:
                return "graphic";
            default:
                return "other";
        }
    }
}