import { ILocalizationService } from "../../core";
import { Helper } from "../../shared";
import { ISelectionManager, Models } from "../../main";

export class BPUtilityPanel implements ng.IComponentOptions {
    public template: string = require("./bp-utility-panel.html");
    public controller: Function = BPUtilityPanelController;
}

export class BPUtilityPanelController {
    public static $inject: [string] = [
        "localization",
        "selectionManager"
    ];

    private _subscribers: Rx.IDisposable[];
    private _currentItem: string;
    private _currentItemClass: string;

    public get currentItem() { 
        return this._currentItem;
    }

    public get currentItemClass() {
        return this._currentItemClass;
    }

    constructor(
        private localization: ILocalizationService,
        private selectionManager: ISelectionManager) {
    }

    //all subscribers need to be created here in order to unsubscribe (dispose) them later on component destroy life circle step
    public $onInit(o) {
        let selectedItemSubscriber: Rx.IDisposable = this.selectionManager.selectedItemObservable
            .distinctUntilChanged()
            .subscribe(this.onItemChanged);

        this._subscribers = [
            selectedItemSubscriber
        ];
    }

    public $onDestroy() {
        //dispose all subscribers
        this._subscribers = this._subscribers.filter((it: Rx.IDisposable) => { it.dispose(); return false; });
    }

    private onItemChanged = (item: Models.IItem) => {
        this._currentItem = item ? `${(item.prefix || "")}${item.id}: ${item.name}` : null;
        this._currentItemClass = item ?
        "icon-" + Helper.toDashCase(Models.ItemTypePredefined[item.predefinedType] || "document") :
            "icon-document";
    }
}
