import { ILocalizationService } from "../../core";
import { Helper } from "../../shared";
import { ISelectionManager, Models } from "../../main";
import { IBpAccordionController } from "../../main/components/bp-accordion/bp-accordion";

export class BPUtilityPanel implements ng.IComponentOptions {
    public template: string = require("./bp-utility-panel.html");
    public controller: Function = BPUtilityPanelController;
}

export class BPUtilityPanelController {
    public static $inject: [string] = [
        "localization",
        "selectionManager",
        "$element"
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
        private selectionManager: ISelectionManager,
        private $element: ng.IAugmentedJQuery) {
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

    public testHidePanel() {
        console.log("Test hide");
        let accordionCtrl: IBpAccordionController = this.getAccordionController();
        let panels = accordionCtrl.getPanels();
        accordionCtrl.hidePanel(panels[3]);
    }
    public testShowPanel() {
        console.log("Test show");
        let accordionCtrl: IBpAccordionController = this.getAccordionController();
        let panels = accordionCtrl.getPanels();
        accordionCtrl.showPanel(panels[3]);
    }

    private getAccordionController(): IBpAccordionController {
        return angular.element(this.$element.find("bp-accordion")[0]).controller("bpAccordion");
    }

    private onItemChanged = (item: Models.IItem) => {
        if (item != null) {
            this._currentItem = `${(item.prefix || "")}${item.id}: ${item.name}`;
            this._currentItemClass = "icon-" + Helper.toDashCase(Models.ItemTypePredefined[item.predefinedType]);
        } else {
            this._currentItem = null;
            this._currentItemClass = null;
        }
    }
}
