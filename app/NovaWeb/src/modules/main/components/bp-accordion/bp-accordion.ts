import {ILocalizationService} from "../../../core";

/*
 Sample template. The following parameters are optional: 
 accordion-heading-height
 accordion-panel-id
 accordion-panel-class 
 accordion-panel-heading-height

 <bp-accordion accordion-heading-height="33">
 <bp-accordion-panel accordion-panel-heading="Panel heading">
 Panel content
 </bp-accordion-panel>
 <bp-accordion-panel accordion-panel-heading="Panel heading" accordion-panel-id="custom-panel">
 Panel content
 </bp-accordion-panel>
 <bp-accordion-panel accordion-panel-heading="Panel heading" accordion-panel-heading-height="66" accordion-panel-class="custom-class">
 Panel content
 </bp-accordion-panel>
 </bp-accordion>
 */

export interface IBpAccordionController {
    accordionId?: string;
    accordionHeadingHeight?: number;
    panels: IBpAccordionPanelController[];
    openPanels: IBpAccordionPanelController[];

    addPanel(panel: IBpAccordionPanelController);
    getPanels(): IBpAccordionPanelController[];
    openPanel(panel: IBpAccordionPanelController);
    hidePanel(panel: IBpAccordionPanelController);
    showPanel(panel: IBpAccordionPanelController);
    cleanUpOpenPanels();
    recalculateLayout();
}

export interface IBpAccordionPanelController {
    accordionPanelId?: string;
    accordionPanelHeading?: string;
    accordionPanelHeadingHeight?: any;
    accordionPanelClass?: string;
    accordionGroup: IBpAccordionController;

    isOpen: boolean;
    isActiveObservable: Rx.Observable<boolean>;
    isPinned: boolean;
    isVisible: boolean;

    openPanel();
    closePanel();
    pinPanel();
    getElement();
}

export class BpAccordion implements ng.IComponentOptions {
    public template: string = require("./bp-accordion.group.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BpAccordionCtrl;
    public controllerAs: string = "bpAccordion";
    public bindings: any = {
        accordionId: "@",
        accordionHeadingHeight: "@"
    };
    public transclude: boolean = true;
}

export class BpAccordionPanel implements ng.IComponentOptions {
    public template: string = require("./bp-accordion.panel.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BpAccordionPanelCtrl;
    public bindings: any = {
        accordionPanelId: "@",
        accordionPanelHeading: "@",
        accordionPanelHeadingHeight: "@",
        accordionPanelClass: "@"
    };
    public transclude: boolean = true;
    public replace: boolean = true;
    public require: any = {
        accordionGroup: "^bpAccordion"
    };
}

export class BpAccordionCtrl implements IBpAccordionController {
    static $inject: [string] = ["$element", "$timeout"];

    public accordionId: string;
    public accordionHeadingHeight: number;
    public panels: IBpAccordionPanelController[] = [];
    public openPanels: IBpAccordionPanelController[] = [];

    private defaultHeadingHeight: number = 31; // default heading height for all the accordion panels
    private lastOpenedPanel: IBpAccordionPanelController;

    constructor(private $element, private $timeout) {
        // the accordionId is needed in case multiple accordions are present in the same page
        this.accordionId = this.accordionId || "bp-accordion-" + Math.floor(Math.random() * 10000);
        this.accordionHeadingHeight = this.accordionHeadingHeight || this.defaultHeadingHeight;
    }

    public addPanel = (accordionPanel: IBpAccordionPanelController) => {
        this.panels.push(accordionPanel);
    };

    public getPanels = (): IBpAccordionPanelController[] => {
        return this.panels;
    };

    public hidePanel = (panel: IBpAccordionPanelController) => {
        let otherOpenPanels = this.openPanels.filter((p: IBpAccordionPanelController) => p.isOpen && p !== panel);
        panel.isVisible = false;

        if (panel.isOpen && otherOpenPanels.length === 0) {
            this.openNextAvailablePanel(panel);
        } else {
            this.recalculateLayout();
        }
    }

    public showPanel = (panel: IBpAccordionPanelController) => {
        panel.isVisible = true;
        this.recalculateLayout();
    }

    private openNextAvailablePanel(currentPanel: IBpAccordionPanelController): void {
        let curLoc = this.panels.indexOf(currentPanel);
        let panelToOpen: IBpAccordionPanelController;

        // [1, 2, x, 4, 5] => [4, 5] + [2, 1]
        let availablePanelsToOpen = this.panels.slice(curLoc + 1).concat(this.panels.slice(0, curLoc).reverse());

        panelToOpen = availablePanelsToOpen.filter((p: IBpAccordionPanelController) => p.isVisible)[0];

        if (panelToOpen && !panelToOpen.isOpen) {
            panelToOpen.openPanel();
        } else {
            this.recalculateLayout();
        }
    }

    public openPanel = (panel: IBpAccordionPanelController) => {
        this.lastOpenedPanel = panel;
        this.openPanels = this.openPanels
            .map((p: IBpAccordionPanelController) => {
                if (!p.isPinned) {
                    p.closePanel();
                }
                return p;
            })
            .filter((p: IBpAccordionPanelController) => p.isPinned);
        this.openPanels.push(panel);
        this.recalculateLayout();
    }

    public cleanUpOpenPanels = () => {
        const numPinnedPanels = this.openPanels.filter((p: IBpAccordionPanelController) => p.isPinned && p.isVisible).length;

        if (this.openPanels.length > 1) {
            this.openPanels = this.openPanels
                .map((panel: IBpAccordionPanelController) => {
                    if (!panel.isVisible) {
                        return panel;
                    } else if (numPinnedPanels > 0 && !panel.isPinned) {
                        panel.closePanel();
                    } else if (numPinnedPanels === 0 && panel !== this.lastOpenedPanel) {
                        panel.closePanel();
                    }
                    return panel;
                })
                .filter((panel: IBpAccordionPanelController) => panel.isOpen);
            this.recalculateLayout();
        }
    };

    public recalculateLayout = () => {
        const numberOfOpenElements: number = this.openPanels.filter((p: IBpAccordionPanelController) => p.isVisible).length;
        const closedHeadersHeight: number = <number>this.panels.reduce((prev: number, cur: IBpAccordionPanelController) => {
            return prev + (cur.isOpen || !cur.isVisible ? 0 : parseInt(<any>cur.accordionPanelHeadingHeight, 10));
        }, 0);

        // set height of content of components
        this.panels.map((p: IBpAccordionPanelController) => {
            const panelEl: any = p.getElement();
            const panelContentEl = panelEl.querySelector(".bp-accordion-panel-content");

            if (p.isOpen && p.isVisible) {
                panelContentEl.style.height = "calc(100% - " + p.accordionPanelHeadingHeight + "px)";
            } else {
                panelEl.style.height = p.accordionPanelHeadingHeight + "px";
                panelContentEl.style.height = "0";
            }
        });

        // set height of components
        this.openPanels.map((p: IBpAccordionPanelController) => {
            const panelEl: any = p.getElement();
            panelEl.style.height = "calc(" + (100 / numberOfOpenElements) + "% - " + (closedHeadersHeight / numberOfOpenElements) + "px)";
        });
    };

    public $postLink = () => {
        this.$timeout(() => {
            // open the first panel on load
            if (this.panels && this.panels.length) {
                this.panels[0].openPanel();
            }

            // we need to redistribute the height after all the panels have been added
            this.recalculateLayout();
        }, 0);
    };
}

export class BpAccordionPanelCtrl implements IBpAccordionPanelController {
    static $inject: [string] = ["localization", "$element"];

    private _isOpen: boolean;
    private _isVisible: boolean;
    private isActiveSubject: Rx.BehaviorSubject<boolean>;

    public accordionGroup: BpAccordionCtrl;
    public accordionPanelId: string;
    public accordionPanelHeadingHeight: number;
    public accordionPanelClass: string;
    public isPinned: boolean;

    constructor(private localization: ILocalizationService, private $element) {
        // the accordionPanelId is/may be needed to target specific panels/nested elements
        this.accordionPanelId = this.accordionPanelId || "bp-accordion-panel-" + Math.floor(Math.random() * 10000);
        this.isActiveSubject = new Rx.BehaviorSubject<boolean>(true);
        this.isOpen = false;
        this.isPinned = false;
        this._isVisible = true;
    }

    public get isOpen(): boolean {
        return this._isOpen;
    }

    public set isOpen(value: boolean) {
        if (this._isOpen !== value) {
            this._isOpen = value;
            this.isActive = this.isVisible && this._isOpen;
        }
    }

    public get isActive(): boolean {
        return this.isActiveSubject.getValue();
    }

    public set isActive(value: boolean) {
        this.isActiveSubject.onNext(value);
    }

    public get isActiveObservable(): Rx.Observable<boolean> {
        return this.isActiveSubject.asObservable();
    }

    public get isVisible(): boolean {
        return this._isVisible;
    }

    public set isVisible(value: boolean) {
        if (this._isVisible !== value) {
            this._isVisible = value;

            if (this._isVisible) {
                this.getElement().className = this.getElement().className.replace(" bp-accordion-panel-hidden", "");
            } else {
                this.getElement().className += " bp-accordion-panel-hidden";
            }

            this.isActive = this.isVisible && this.isOpen;
        }
    }

    public getElement() {
        return this.$element[0];
    }

    public openPanel = () => {
        if (!this.isOpen) {
            this.isOpen = true;
            this.getElement().className += " bp-accordion-panel-open";
            this.accordionGroup.openPanel(this);
        } else {
            this.accordionGroup.cleanUpOpenPanels();
        }
    };

    public closePanel = () => {
        this.isOpen = false;
        this.getElement().className = this.getElement().className.replace(" bp-accordion-panel-open", "");
    };

    public pinPanel = () => {
        this.accordionGroup.cleanUpOpenPanels();
        this.accordionGroup.recalculateLayout();
    };

    public $onInit = () => {
        // if not specific heading heigh is set, use the one defined in the container
        if (!this.accordionPanelHeadingHeight) {
            this.accordionPanelHeadingHeight = this.accordionGroup.accordionHeadingHeight;
        }
    };

    public $postLink = () => {
        this.accordionGroup.addPanel(this);
    };
}
