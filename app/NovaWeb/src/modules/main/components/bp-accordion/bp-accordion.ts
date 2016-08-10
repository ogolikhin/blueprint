import { ILocalizationService } from "../../../core";

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
    isOpenObservable: Rx.Observable<boolean>;
    isPinned: boolean;
    isVisible: boolean;

    openPanel();
    closePanel();
    pinPanel();
    getElement();
}

export class BpAccordion implements ng.IComponentOptions {
    public template: string = require("./bp-accordion.group.html");
    public controller: Function = BpAccordionCtrl;
    public controllerAs: string = "bpAccordion";
    public bindings: any = {
        accordionId: "@",
        accordionHeadingHeight: "@"
    };
    public transclude: boolean = true;
}

export class BpAccordionPanel implements ng.IComponentOptions {
    public template: string = require("./bp-accordion.panel.html");
    public controller: Function = BpAccordionPanelCtrl;
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

    // public getPinnedPanels = () => {
    //     return this.openPanels.filter((p: IBpAccordionPanelController) => p.isPinned);
    // }

    public hidePanel = (panel: IBpAccordionPanelController) => {
        let otherPinnedPanels = this.openPanels.filter((p: IBpAccordionPanelController) => p.isPinned && p !== panel);
        panel.isVisible = false;

        if (otherPinnedPanels.length === 0 && panel.isOpen) {
            this.openNextAvailablePanel(panel);

        } else if (!panel.isOpen) {
            this.recalculateLayout();

        } else {
            this.cleanUpOpenPanels();
            this.recalculateLayout();
        }
    }
    
    public showPanel = (panel: IBpAccordionPanelController) => {
        panel.isVisible = true;
        if (panel.isPinned) {
            this.openPanels.push(panel);
        }
        this.recalculateLayout();
    }

    private openNextAvailablePanel(currentPanel: IBpAccordionPanelController): void {
        // find location of the panel
        let curLoc = this.panels.indexOf(currentPanel);
        let panelToOpen: number = 0;

        // TODO: loop forwards to find next panel (some panels may not be visible)
        if (curLoc === this.panels.length - 1) {
            panelToOpen = this.panels.length - 2;
        } else {
            panelToOpen = curLoc + 1;
        }

        // TODO: loop backwards if forward found nothing (some panels may not be visible)

        this.panels[panelToOpen].openPanel();
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
            .filter( (p: IBpAccordionPanelController) => p.isPinned);
        this.openPanels.push(panel);
        this.recalculateLayout();
    }

    public cleanUpHiddenPanels = () => {
        const numPinnedPanels = this.openPanels.filter( (p: IBpAccordionPanelController) => p.isPinned && p.isVisible).length;
        // const openPanels = this.openPanels.filter( (p: IBpAccordionPanelController) => p.isVisible);

        // if (openPanels.length > 1) {
        if (this.openPanels.length > 1) {
            this.openPanels = this.openPanels
                .map( (panel: IBpAccordionPanelController) => {
                    if (numPinnedPanels > 0 && !panel.isPinned) {
                        panel.closePanel();
                    } else if (numPinnedPanels === 0 && panel !== this.lastOpenedPanel) { //} && !panel.isPinned) {
                        panel.closePanel();
                    }
                    return panel;
                })
                .filter( (panel: IBpAccordionPanelController) => panel.isOpen && panel.isVisible);
            this.recalculateLayout();
        }
    };

    public cleanUpOpenPanels = () => {
        const numPinnedPanels = this.openPanels.filter( (p: IBpAccordionPanelController) => p.isPinned && p.isVisible).length;
        // const openPanels = this.openPanels.filter( (p: IBpAccordionPanelController) => p.isVisible);

        // if (openPanels.length > 1) {
        if (this.openPanels.length > 1) {
            this.openPanels = this.openPanels
                .map( (panel: IBpAccordionPanelController) => {
                    if (numPinnedPanels > 0 && !panel.isPinned) {
                        panel.closePanel();
                    } else if (numPinnedPanels === 0 && panel !== this.lastOpenedPanel) { //} && !panel.isPinned) {
                        panel.closePanel();
                    }
                    return panel;
                })
                .filter( (panel: IBpAccordionPanelController) => panel.isOpen && panel.isVisible);
            this.recalculateLayout();
        }
    };

    public recalculateLayout = () => {
        const numberOfOpenElements: number = this.openPanels.filter((p: IBpAccordionPanelController) => p.isVisible).length;
        const closedHeadersHeight: number = <number>this.panels.reduce( (prev: number, cur: IBpAccordionPanelController) => {
            return prev + (cur.isOpen || !cur.isVisible ? 0 : parseInt(<any>cur.accordionPanelHeadingHeight, 10));
        }, 0);
        
        // set height of content of components
        this.panels.map( (p: IBpAccordionPanelController) => {
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
        this.openPanels.map( (p: IBpAccordionPanelController) => {
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
    
    private isOpenSubject: Rx.BehaviorSubject<boolean>;
    private _isVisible: boolean;

    public accordionGroup: BpAccordionCtrl;
    public accordionPanelId: string;
    public accordionPanelHeadingHeight: number;
    public accordionPanelClass: string;
    public isPinned: boolean;

    constructor(private localization: ILocalizationService, private $element) {
        // the accordionPanelId is/may be needed to target specific panels/nested elements
        this.accordionPanelId = this.accordionPanelId || "bp-accordion-panel-" + Math.floor(Math.random() * 10000);
        this.isOpenSubject = new Rx.BehaviorSubject<boolean>(false);
        this.isOpen = false;
        this.isPinned = false;
        this._isVisible = true;
    }

    public get isOpen(): boolean {
        return this.isOpenSubject.getValue();
    }

    public set isOpen(value: boolean) {
        this.isOpenSubject.onNext(value);
    }

    public get isOpenObservable(): Rx.Observable<boolean> {
        return this.isOpenSubject.asObservable();
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
