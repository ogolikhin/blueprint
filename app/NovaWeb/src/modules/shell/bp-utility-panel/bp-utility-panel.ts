import {ILocalizationService} from "../../commonModule/localization/localization.service";
import {IBpAccordionController} from "../../main/components/bp-accordion/bp-accordion";
import {ItemTypePredefined} from "../../main/models/itemTypePredefined.enum";
import {IItemChangeSet, ISelection, IStatefulItem, StatefulArtifact} from "../../managers/artifact-manager";
import {ISelectionManager} from "../../managers/selection-manager/selection-manager";
import {IUtilityPanelContext, IUtilityPanelController, PanelType, UtilityPanelService} from "./utility-panel.svc";

export class BPUtilityPanel implements ng.IComponentOptions {
    public template: string = require("./bp-utility-panel.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BPUtilityPanelController;
}

interface IActivePanel {
    isActive: boolean;
    panelType: PanelType;
}

export class BPUtilityPanelController implements IUtilityPanelController {
    public static $inject: [string] = [
        "localization",
        "selectionManager",
        "$element",
        "utilityPanelService"
    ];

    private _subscribers: Rx.IDisposable[];
    private propertySubscriber: Rx.IDisposable;
    private selection: ISelection;
    private activePanelContexts: IUtilityPanelContext[];
    public itemDisplayName: string;
    public itemClass: string;
    public itemTypeId: number;
    public itemTypeIconId: number;
    public hasCustomIcon: boolean;
    public isAnyPanelVisible: boolean;


    constructor(private localization: ILocalizationService,
                private selectionManager: ISelectionManager,
                private $element: ng.IAugmentedJQuery,
                private utilityPanelService: UtilityPanelService) {
        this.isAnyPanelVisible = true;
        this.utilityPanelService.initialize(this);
        this.activePanelContexts = [];
    }

    //all subscribers need to be created here in order to unsubscribe (dispose) them later on component destroy life circle step
    public $postLink() {
        const selectionObservable = this.selectionManager.selectionObservable
            .distinctUntilChanged()
            .subscribe(this.onSelectionChanged);

        this._subscribers = [selectionObservable];
        const accordionCtrl: IBpAccordionController = this.getAccordionController();
        if (accordionCtrl) {
            for (let panelType = 0; panelType <  accordionCtrl.getPanels().length; panelType++) {
                let panelCtrl = accordionCtrl.getPanels()[panelType];
                this._subscribers.push(panelCtrl.isActiveObservable
                    .map(isActive => {
                        return {panelType: panelType, isActive: isActive};
                    })
                    .subscribeOnNext(this.activatePanel));
            }
        }
    }

    public $onDestroy() {
        //dispose all subscribers
        this._subscribers = this._subscribers.filter((it: Rx.IDisposable) => {
            it.dispose();
            return false;
        });
    }

    public getUtilityPanelContext(panelType: PanelType|string): IUtilityPanelContext {
        const effectivePanelType: PanelType = _.isString(panelType) ? PanelType[panelType] : panelType;
        return _.find(this.activePanelContexts, (context) => {
            return context.panelType === effectivePanelType;
        });
    }

    private hidePanel(panelType: PanelType) {
        const accordionCtrl: IBpAccordionController = this.getAccordionController();
        if (accordionCtrl) {
            accordionCtrl.hidePanel(accordionCtrl.getPanels()[panelType]);
        }
    }

    public openPanel(panel: PanelType) {
        const accordionCtrl: IBpAccordionController = this.getAccordionController();

        if (accordionCtrl) {
            const panelToOpen = accordionCtrl.getPanels()[panel];
            panelToOpen.openPanel();
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

    private updateItem = (changes: IItemChangeSet) => {
        if (changes && changes.item) {
            const item: IStatefulItem = changes.item;
            this.itemDisplayName = `${(item.prefix || "")}${item.id}: ${item.name || ""}`;
            this.itemTypeId = item.itemTypeId;
            if (item.itemTypeId === ItemTypePredefined.Collections && item.predefinedType === ItemTypePredefined.CollectionFolder) {
                this.itemClass = "icon-" + _.kebabCase(ItemTypePredefined[ItemTypePredefined.Collections] || "");
            } else if (item.itemTypeId === ItemTypePredefined.BaselinesAndReviews && item.predefinedType === ItemTypePredefined.BaselineFolder) {
                this.itemClass = "icon-" + _.kebabCase(ItemTypePredefined[ItemTypePredefined.BaselinesAndReviews] || "");
            } else {
                this.itemClass = "icon-" + _.kebabCase(ItemTypePredefined[item.predefinedType] || "");
            }
            if (item.predefinedType !== ItemTypePredefined.Project && item instanceof StatefulArtifact) {
                this.hasCustomIcon = _.isFinite(item.itemTypeIconId);
                this.itemTypeIconId = item.itemTypeIconId;
            }
        }
    }

    private clearItem() {
        this.itemDisplayName = undefined;
        this.itemClass = undefined;
        this.itemTypeId = undefined;
        this.hasCustomIcon = false;
    }

    private onSelectionChanged = (selection: ISelection) => {
        this.clearItem();
        this.selection = selection;
        const item: IStatefulItem = selection.subArtifact || selection.artifact;
        if (this.propertySubscriber) {
            this.propertySubscriber.dispose();
        }
        if (this.emptySelection(selection) || selection.multiSelect) {
            this.hidePanels();
        } else if (item) {
            this.propertySubscriber = item.getPropertyObservable().subscribeOnNext(this.updateItem);
            this.toggleHistoryPanel(selection);
            this.togglePropertiesPanel(selection);
            this.toggleFilesPanel(selection);
            this.toggleRelationshipsPanel(selection);
            this.toggleDiscussionsPanel(selection);
        }
        this.setAnyPanelIsVisible();
        this.updateActivePanelContexts(selection);
    }

    private emptySelection(selection: ISelection) {
        return !selection.artifact;
    }

    private updateActivePanelContexts(selection: ISelection) {
        const accordionCtrl: IBpAccordionController = this.getAccordionController();
        if (accordionCtrl) {
            this.activePanelContexts = [];
            for (let panelType = 0; panelType <  accordionCtrl.getPanels().length; panelType++) {
                const panelCtrl = accordionCtrl.getPanels()[panelType];
                if (panelCtrl.isActive) {
                    const context: IUtilityPanelContext = {
                        artifact: selection.artifact,
                        subArtifact: selection.subArtifact,
                        panelType: panelType
                    };
                    this.activePanelContexts.push(context);
                }
            }
        }
    }

    private activatePanel = (panel: IActivePanel) => {
        this.activePanelContexts = this.activePanelContexts || [];
        if (panel.isActive) {
            const context: IUtilityPanelContext = {
                artifact: this.selection.artifact,
                subArtifact: this.selection.subArtifact,
                panelType: panel.panelType
            };
            this.activePanelContexts.push(context);
        } else {
            _.remove(this.activePanelContexts, context => context.panelType === panel.panelType);
        }
    };

    private hidePanels() {
        this.hidePanel(PanelType.Discussions);
        this.hidePanel(PanelType.Files);
        this.hidePanel(PanelType.History);
        this.hidePanel(PanelType.Properties);
        this.hidePanel(PanelType.Relationships);
        this.isAnyPanelVisible = false;
    }

    private toggleDiscussionsPanel(selection: ISelection) {
        const artifact = selection.artifact;
        if (artifact && (artifact.predefinedType === ItemTypePredefined.ArtifactBaseline
            || (artifact.predefinedType === ItemTypePredefined.BaselineFolder && artifact.itemTypeId !== ItemTypePredefined.Baseline)
            || artifact.predefinedType === ItemTypePredefined.ArtifactReviewPackage
            || artifact.predefinedType === ItemTypePredefined.CollectionFolder
            || artifact.predefinedType === ItemTypePredefined.ArtifactCollection
            || artifact.predefinedType === ItemTypePredefined.Project)) {
            this.hidePanel(PanelType.Discussions);
        } else {
            this.showPanel(PanelType.Discussions);
        }
    }

    private toggleHistoryPanel(selection: ISelection) {
        const artifact = selection.artifact;
        const subArtifact = selection.subArtifact;
        if (subArtifact
            || (artifact &&
            (artifact.predefinedType === ItemTypePredefined.ArtifactReviewPackage
            || (artifact.predefinedType === ItemTypePredefined.BaselineFolder && artifact.itemTypeId !== ItemTypePredefined.Baseline)
            || artifact.predefinedType === ItemTypePredefined.CollectionFolder
            || artifact.predefinedType === ItemTypePredefined.ArtifactCollection
            || artifact.predefinedType === ItemTypePredefined.Project))) {
            this.hidePanel(PanelType.History);
        } else {
            this.showPanel(PanelType.History);
        }
    }

    private togglePropertiesPanel(selection: ISelection) {
        const artifact = selection.artifact;
        const explorerArtifact = this.selectionManager.getExplorerArtifact();

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
            explorerArtifact &&
            explorerArtifact.predefinedType === ItemTypePredefined.UseCaseDiagram))) {
            this.showPanel(PanelType.Properties);
        } else {
            this.hidePanel(PanelType.Properties);
        }
    }

    private toggleFilesPanel(selection: ISelection) {
        const artifact = selection.artifact;

        if (artifact && (artifact.predefinedType === ItemTypePredefined.Document
            || (artifact.predefinedType === ItemTypePredefined.BaselineFolder && artifact.itemTypeId !== ItemTypePredefined.Baseline)
            || artifact.predefinedType === ItemTypePredefined.ArtifactReviewPackage
            || artifact.predefinedType === ItemTypePredefined.CollectionFolder
            || artifact.predefinedType === ItemTypePredefined.ArtifactCollection
            || artifact.predefinedType === ItemTypePredefined.Project)) {
            this.hidePanel(PanelType.Files);
        } else {
            this.showPanel(PanelType.Files);
        }
    }

    private toggleRelationshipsPanel(selection: ISelection) {
        const artifact = selection.artifact;

        if (artifact && (artifact.predefinedType === ItemTypePredefined.BaselineFolder
            || artifact.predefinedType === ItemTypePredefined.ArtifactBaseline
            || artifact.predefinedType === ItemTypePredefined.ArtifactReviewPackage
            || artifact.predefinedType === ItemTypePredefined.CollectionFolder
            || artifact.predefinedType === ItemTypePredefined.ArtifactCollection
            || artifact.predefinedType === ItemTypePredefined.Project)) {
            this.hidePanel(PanelType.Relationships);
        } else {
            this.showPanel(PanelType.Relationships);
        }
    }

    private setAnyPanelIsVisible() {
        const accordionCtrl: IBpAccordionController = this.getAccordionController();
        if (accordionCtrl) {
            this.isAnyPanelVisible = accordionCtrl.panels.filter((p) => { return p.isVisible === true; }).length > 0;
        }
    }
}
