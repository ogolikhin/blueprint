import {IMessageService} from "../main/components/messages/message.svc";
import {IStatefulArtifact} from "../managers/artifact-manager";
import {ISelectionManager} from "../managers/selection-manager/selection-manager";

export class BpBaseEditor {
    protected subscribers: Rx.IDisposable[];
    protected isDestroyed: boolean;
    public artifact: IStatefulArtifact;
    public isLoading: boolean;

    constructor(public messageService: IMessageService,
                public selectionManager: ISelectionManager) {
        this.subscribers = [];
    }

    public $onInit() {
        this.isDestroyed = false;
        this.isLoading = true;

        this.artifact = this.selectionManager.getArtifact();
        if (this.artifact) {
            const selectedArtifactSub = this.artifact.getObservable()
                .subscribeOnNext(this.onArtifactChanged);

            this.subscribers.push(selectedArtifactSub);
        }
    }

    public $onDestroy() {
        this.isDestroyed = true;
        this.destroy();
    }

    protected destroy(): void {
        this.artifact = undefined;

        if (this.subscribers) {
            this.subscribers.forEach(
                (subscriber: Rx.IDisposable) => {
                    subscriber.dispose();
                }
            );
        }

        this.subscribers = undefined;
    }

    protected onArtifactChanged = () => {
        this.onArtifactReady();
    };

    protected onArtifactReady() {
        this.isLoading = false;
    }
}
