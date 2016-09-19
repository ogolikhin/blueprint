import { ILocalizationService } from "../../core";
import { Helper } from "../../shared";
import { Models } from "../../main";
import { IArtifactManager, SelectionSource, ISelection, IStatefulItem } from "../../managers/artifact-manager";
import { ItemTypePredefined } from "../../main/models/enums";
import { IBpAccordionController } from "../../main/components/bp-accordion/bp-accordion";

export enum PanelType {
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
        "artifactManager",
        "$element"
    ];

    private _subscribers: Rx.IDisposable[];
    private _currentItem: string;
    private _currentItemClass: string;
    private _currentItemType: number;
    private _currentItemIcon: number;

    public get currentItem() { 
        return this._currentItem;
    }

    public get currentItemClass() {
        return this._currentItemClass;
    }

    public get currentItemType() {
        return this._currentItemType;
    }

    public get currentItemIcon() {
        return this._currentItemIcon;
    }

    constructor(
        private localization: ILocalizationService,
        private artifactManager: IArtifactManager,
        private $element: ng.IAugmentedJQuery) {
        this._currentItem = null;
        this._currentItemClass = null;
        this._currentItemType = null;
        this._currentItemIcon = null;
    }

    //all subscribers need to be created here in order to unsubscribe (dispose) them later on component destroy life circle step
    public $onInit(o) {
        const selectionObservable = this.artifactManager.selection.selectionObservable
            .distinctUntilChanged()
            .subscribe(this.onSelectionChanged);

        const selectedItemSubscriber: Rx.IDisposable = this.artifactManager.selection.selectionObservable
            .map((selection: ISelection) => selection.subArtifact || selection.artifact)
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

    public getAccordionController(): IBpAccordionController {
        return angular.element(this.$element.find("bp-accordion")[0]).controller("bpAccordion");
    }

    private onItemChanged = (item: IStatefulItem) => {
        if (item != null) {
            this._currentItem = `${(item.prefix || "")}${item.id}: ${item.name}`;
            this._currentItemClass = "icon-" + Helper.toDashCase(Models.ItemTypePredefined[item.predefinedType] || "");
            this._currentItemType = item.itemTypeId;
            this._currentItemIcon = null;
            //TODO: (PP) Please fix this for both artifact and subartifact
            // if (item.predefinedType !== ItemTypePredefined.Project) {
            //     let artifactType = this.projectManager.getArtifactType(item as Models.IArtifact);
            //     if (artifactType && artifactType.iconImageId && angular.isNumber(artifactType.iconImageId)) {
            //         this._currentItemIcon = artifactType.iconImageId;
            //     }
            // }
        } else {
            this._currentItem = null;
            this._currentItemClass = null;
            this._currentItemType = null;
            this._currentItemIcon = null;
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
                selection.source === SelectionSource.Editor &&
                this.artifactManager.selection.getArtifact(SelectionSource.Explorer).predefinedType === ItemTypePredefined.UseCaseDiagram))) {

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
