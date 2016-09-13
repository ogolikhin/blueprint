import { IMessageService, IStateManager, ItemState } from "../core";
import { Models } from "../main";

export class BpBaseEditor {
    public static $inject: [string] = ["messageService", "stateManager"];

    protected _subscribers: Rx.IDisposable[];
    public context: Models.IEditorContext;
    public artifactState: ItemState;
    public isLoading: boolean;

    constructor(
        public messageService: IMessageService,
        public stateManager: IStateManager
    ) {
    }

    public $onInit() {
        this._subscribers = [];
    }

    public $onChanges(obj: any) {
        try {
            if (this.onLoading(obj)) {
                this.onLoad(this.context);
            }
        } catch (ex) {
            this.messageService.addError(ex.message);
        }
    }

    public $onDestroy() {
        delete this.context;
        delete this.artifactState;

        this._subscribers = (this._subscribers || []).filter((it: Rx.IDisposable) => { it.dispose(); return false; });

    }

    public onLoading(obj: any): boolean {
        this.isLoading = true;
        let result = !!(this.context && angular.isDefined(this.context.artifact));
        return result;
    }

    public onLoad(context: Models.IEditorContext) {
        this.onUpdate(context);
    }

    public onUpdate(context: Models.IEditorContext) {
        this.isLoading = false;
    }

}


