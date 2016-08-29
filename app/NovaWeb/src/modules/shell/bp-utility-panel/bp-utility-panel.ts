import { ILocalizationService } from "../../core";
import { Helper } from "../../shared";
import { ISelectionManager, Models, ISelection, SelectionSource } from "../../main";
import { ItemTypePredefined } from "../../main/models/enums";
import { IBpAccordionController } from "../../main/components/bp-accordion/bp-accordion";

enum PanelType {
    Properties,
    Relationships,    
    Discussions,
    Files,    
    History
}

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
        const selectionObservable = this.selectionManager.selectionObservable
            .distinctUntilChanged()
            .subscribe(this.onSelectionChanged);

        const selectedItemSubscriber: Rx.IDisposable = this.selectionManager.selectedItemObservable
            .distinctUntilChanged()
            .subscribe(this.onItemChanged);

        this._subscribers = [
            selectionObservable,
            selectedItemSubscriber
        ];
    }

    public $onDestroy() {
        //dispose all subscribers
        this._subscribers = this._subscribers.filter((it: Rx.IDisposable) => { it.dispose(); return false; });
    }

    private hidePanel(panelType: PanelType) {
        const accordionCtrl: IBpAccordionController = this.getAccordionController();
        if (accordionCtrl) {
            accordionCtrl.hidePanel(accordionCtrl.getPanels()[panelType]);
        }
    }

    private showPanel(panelType: PanelType) {
        const accordionCtrl: IBpAccordionController = this.getAccordionController();
        if (accordionCtrl) {
            accordionCtrl.showPanel(accordionCtrl.getPanels()[panelType]);
        }
    }

    private getAccordionController(): IBpAccordionController {
        return angular.element(this.$element.find("bp-accordion")[0]).controller("bpAccordion");
    }

    private onItemChanged = (item: Models.IItem) => {
        if (item != null) {
            this._currentItem = `${(item.prefix || "")}${item.id}: ${item.name}`;
            this._currentItemClass = "icon-" + Helper.toDashCase(Models.ItemTypePredefined[item.predefinedType] || "");
        } else {
            this._currentItem = null;
            this._currentItemClass = null;
        }
    }

    private onSelectionChanged = (selection: ISelection) => {
        if (selection) {
            this.toggleHistoryPanel(selection);
            this.togglePropertiesPanel(selection);
            this.toggleFilesPanel(selection);
        }
    }

    private toggleHistoryPanel(selection: ISelection) {
        if (selection.subArtifact) {
            this.hidePanel(PanelType.History);
        } else {
            this.showPanel(PanelType.History);
        }
    }
    
    private togglePropertiesPanel(selection: ISelection) {
        const artifact = selection.artifact;        
        
        if (artifact && (selection.subArtifact 
            || artifact.predefinedType === ItemTypePredefined.Glossary
            || artifact.predefinedType === ItemTypePredefined.GenericDiagram
            || artifact.predefinedType === ItemTypePredefined.BusinessProcess
            || artifact.predefinedType === ItemTypePredefined.DomainDiagram
            || artifact.predefinedType === ItemTypePredefined.Storyboard
            || artifact.predefinedType === ItemTypePredefined.UseCaseDiagram
            || artifact.predefinedType === ItemTypePredefined.UseCase
            || artifact.predefinedType === ItemTypePredefined.UIMockup
            || artifact.predefinedType === ItemTypePredefined.Process
            || (artifact.predefinedType === ItemTypePredefined.Actor &&
                selection.source === SelectionSource.Editor))) {

            this.showPanel(PanelType.Properties);
        } else {
            this.hidePanel(PanelType.Properties);
        }
    }

    private toggleFilesPanel(selection: ISelection) {
        const artifact = selection.artifact;

        if (artifact && (artifact.predefinedType === ItemTypePredefined.Document
            || artifact.predefinedType === ItemTypePredefined.CollectionFolder
            || artifact.predefinedType === ItemTypePredefined.Project)) {
            this.hidePanel(PanelType.Files);
        } else {
            this.showPanel(PanelType.Files);
        }
    }
}
