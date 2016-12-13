import {IBpAccordionPanelController} from "../../main/components/bp-accordion/bp-accordion";
import {ISelectionManager, IStatefulArtifact, IStatefulSubArtifact} from "../../managers/artifact-manager";
import {IUtilityPanelContext, IUtilityPanelService, IOnPanelChangesObject} from "./utility-panel.svc";

export class BPBaseUtilityPanelController {
    private timeout: ng.IDeferred<void>;
    protected context: IUtilityPanelContext;

    constructor(protected $q: ng.IQService) {
    }

    public $onChanges(onChangesObj: IOnPanelChangesObject) {
        const contextChangesObject = onChangesObj.context;
        let artifact: IStatefulArtifact;
        let subArtifact: IStatefulSubArtifact;
        if (contextChangesObject) {
            this.context = contextChangesObject.currentValue;
            if (this.context) {
                artifact = this.context.artifact;
                subArtifact = this.context.subArtifact;
            }
        }
        this.selectionChanged(artifact, subArtifact);
    }

    public $onDestroy() {
        delete this.context;
    }

    private selectionChanged(artifact: IStatefulArtifact, subArtifact: IStatefulSubArtifact) {
        if (this.timeout) {
            this.timeout.resolve();
        }
        this.timeout = this.$q.defer<any>();
        const selectionChangedResult = this.onSelectionChanged(artifact, subArtifact, this.timeout.promise);
        if (selectionChangedResult) {
            selectionChangedResult.then(() =>
                this.timeout = undefined
            );
        }
    }

    protected onSelectionChanged(artifact: IStatefulArtifact, subArtifact: IStatefulSubArtifact, timeout: ng.IPromise<void>): ng.IPromise<any> {
        return this.$q.resolve();
    }
}
