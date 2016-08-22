import { IMessageService, IStateManager, ItemState } from "../core";
import { IWindowManager, Models, IMainWindow } from "../main";

export class BpBaseEditor {
    public static $inject: [string] = ["messageService", "stateManager", "windowManager"];

    protected _subscribers: Rx.IDisposable[];
    public context: Models.IEditorContext;
    public artifactState: ItemState;
    public isLoading: boolean;

    constructor(
        public messageService: IMessageService,
        public stateManager: IStateManager,
        public windowManager: IWindowManager
    ) {
    }

    public $onInit() {
        this._subscribers = [
            this.windowManager.mainWindow.subscribeOnNext(this.setArtifactEditorLabelsWidth, this)
        ];
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

    public setArtifactEditorLabelsWidth(mainWindow?: IMainWindow) {
        // MUST match $property-width in styles/partials/_properties.scss plus various padding/margin
        const minimumWidth: number = 392 + ((20 + 1 + 15 + 1 + 10) * 2);

        let pageBodyWrapper = document.querySelector(".page-body-wrapper") as HTMLElement;
        if (pageBodyWrapper) {
            let avaliableWidth: number = mainWindow ? mainWindow.contentWidth : pageBodyWrapper.offsetWidth;

            if (avaliableWidth < minimumWidth) {
                pageBodyWrapper.classList.add("single-column-property");
            } else {
                pageBodyWrapper.classList.remove("single-column-property");
            }
        }
    };
}


