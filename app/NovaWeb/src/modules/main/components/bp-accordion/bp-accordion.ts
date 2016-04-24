import {ILocalizationService} from "../../../core/localization";
/*
tslint:disable
*/ /*
Sample template. The parameters accordion-heading-height, accordion-open-top, accordion-panel-id, accordion-panel-class, and accordion-panel-heading-height are optional.
<bp-accordion accordion-heading-height="33" accordion-open-top>
    <bp-accordion-panel accordion-panel-heading="Panel heading">Panel content</bp-accordion-panel>
    <bp-accordion-panel accordion-panel-heading="Panel heading" accordion-panel-id="custom-panel">Panel content</bp-accordion-panel>
    <bp-accordion-panel accordion-panel-heading="Panel heading" accordion-panel-heading-height="66" accordion-panel-class="custom-class">Panel content</bp-accordion-panel>
</bp-accordion>
*/ /*
tslint:enable
*/

interface IBpAccordionController {
    accordionId?: string;
    accordionHeadingHeight?: number;
    accordionOpenTop?: any;
}

interface IBpAccordionPanelController {
    accordionPanelId?: string;
    accordionPanelHeading?: string;
    accordionPanelHeadingHeight?: number;
    accordionPanelClass?: string;
}

export class BpAccordion implements ng.IComponentOptions {
    public template: string = require("./bp-accordion.group.html");
    public controller: Function = BpAccordionCtrl;
    public controllerAs: string = "bpAccordion";
    public bindings: any = {
        accordionId: "@",
        accordionHeadingHeight: "@",
        accordionOpenTop: "@"
    };
    public transclude: boolean = true;
}

export class BpAccordionPanel implements ng.IComponentOptions {
    public template: string = require("./bp-accordion.panel.html");
    public controller: Function = BpAccordionPanelCtrl;
    public controllerAs: string = "bpAccordionPanel";
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
    public accordionOpenTop: any;
    public accordionPanels = [];

    private defaultHeadingHeight: number = 40; // default heading height for all the accordion panels
    private currentPanel: string;
    private openAtTheTop: boolean;

    constructor(private $element, private $timeout) {
        // the accordionId is needed in case multiple accordions are present in the same page
        this.accordionId = this.accordionId || "bp-accordion-" + Math.floor(Math.random() * 10000);

        this.accordionHeadingHeight = this.accordionHeadingHeight || this.defaultHeadingHeight;

        this.openAtTheTop = typeof this.accordionOpenTop !== "undefined";
    }

    public addPanelAndHeight = (accordionPanelId: string, accordionPanelHeight: number) => {
        this.accordionPanels.push([accordionPanelId, accordionPanelHeight]);
        this.currentPanel = this.accordionPanels[0][0]; // the current panel is always the first one on init
    };

    public activateAnotherPinnedPanel = (accordionPanelId: string) => {
        // check if other panels are open, so we can re-assign the current panel
        var accordion = this.$element[0].firstChild;
        /* tslint:disable */
        var otherPinnedPanels = accordion.querySelectorAll(".bp-accordion-panel:not([id='" + accordionPanelId + "']) input[type=checkbox].bp-accordion-panel-pin:checked");
        /* tslint:enable */
        if (otherPinnedPanels.length !== 0) {
            var firstPinnedPanel = otherPinnedPanels[0].parentNode;
            var trigger = firstPinnedPanel.querySelector("input[type=radio].bp-accordion-panel-state");
            trigger.checked = true;
            this.currentPanel = firstPinnedPanel.id;
        }
    };

    public tryToToggle = (accordionPanelId: string) => {
        var accordion = this.$element[0].firstChild;
        var isPanelPinned = accordion.querySelectorAll("#" + accordionPanelId + " input[type=checkbox].bp-accordion-panel-pin:checked").length;
        if (accordionPanelId !== this.currentPanel) {
            // if the panel wasn't pinned and it wasn't the current one, it means it was previously closed
            if (isPanelPinned === 0 && this.openAtTheTop) {
                var panel = accordion.querySelector("#" + accordionPanelId).parentNode;
                accordion.insertBefore(panel, accordion.firstChild);
            }
            this.currentPanel = accordionPanelId;
        } else {
            if (isPanelPinned === 0) {
                this.activateAnotherPinnedPanel(accordionPanelId);
            }
        }
        this.redistributeHeight();
    };

    public pinUnpin = (accordionPanelId: string, isPinned: number) => {
        if (isPinned === 0) {
            this.activateAnotherPinnedPanel(accordionPanelId);
        }
        this.redistributeHeight();
    };

    public moveToBottom = () => {
        var accordion = this.$element[0].firstChild;
        var lastOpenPanel = null;
        var firstClosedPanel = null;
        var children = accordion.querySelectorAll("bp-accordion-panel");
        for (var i = children.length - 1; i >= 0; i--) {
            if (children[i].className.indexOf("bp-accordion-panel-open") > -1) {
                lastOpenPanel = children[i];
                break;
            }
        }
        if (lastOpenPanel && lastOpenPanel.nextSibling) {
            firstClosedPanel = lastOpenPanel.nextSibling; // may be a text node, but we don't care

            children = accordion.querySelectorAll("bp-accordion-panel.bp-accordion-panel-closed");
            for (var i = children.length - 1; i >= 0; i--) {
                accordion.insertBefore(children[i], firstClosedPanel);
                firstClosedPanel = children[i]; //
            }
        }
    };

    public redistributeHeight = () => {
        var accordion = this.$element[0].firstChild;
        var numberOfPinnedElements = accordion.querySelectorAll("input[type=checkbox].bp-accordion-panel-pin:checked").length;
        /* tslint:disable */
        var isCurrentElementAlsoPinned = accordion.querySelectorAll("input[type=radio].bp-accordion-panel-state:checked ~ input[type=checkbox].bp-accordion-panel-pin:checked").length;
        /* tslint:enable */
        var numberOfOpenElements = numberOfPinnedElements + (isCurrentElementAlsoPinned ? 0 : 1);

        var compensationForClosedHeaders = 0;

        var children = accordion.querySelectorAll(".bp-accordion-panel");
        for (var i = 0; i < children.length; i++) {
            var accordionElement = children[i];
            /* tslint:disable */
            accordionElement.parentNode.className = accordionElement.parentNode.className.replace(" bp-accordion-panel-open", "").replace(" bp-accordion-panel-closed", "");
            /* tslint:enable */
            var accordionHeaderHeight = 0;
            for (var p = 0; p < this.accordionPanels.length; p++) {
                if (accordionElement.id === this.accordionPanels[p][0]) {
                    accordionHeaderHeight = parseInt(this.accordionPanels[p][1], 10);
                    break;
                }
            }
            /* tslint:disable */
            if (accordionElement.querySelectorAll("input[type=radio].bp-accordion-panel-state:checked, input[type=checkbox].bp-accordion-panel-pin:checked").length) {
                accordionElement.querySelector(".bp-accordion-panel-content").style.height = "calc(100% - " + accordionHeaderHeight + "px)";
                accordionElement.parentNode.className += " bp-accordion-panel-open";
            } else {
                compensationForClosedHeaders += accordionHeaderHeight;
                accordionElement.querySelector(".bp-accordion-panel-content").style.height = "0";
                accordionElement.parentNode.style.height = accordionHeaderHeight + "px";
                accordionElement.parentNode.className += " bp-accordion-panel-closed";
            }
            /* tslint:enable */
        }

        children = accordion.querySelectorAll(".bp-accordion-panel-open");
        for (var i = 0; i < children.length; i++) {
            // 100% / N - H * (T - N) / N
            // T: total number of panels
            // N: number of opened panels
            // H: height of heading
            children[i].style.height = "calc(" + (100 / numberOfOpenElements) + "% - " + (compensationForClosedHeaders / numberOfOpenElements) + "px)";
        }

        if (this.openAtTheTop) {
            this.moveToBottom();
        }
    };

    public $postLink = () => {
        // we need to redistribute the height after all the panels have been added
        this.$timeout(this.redistributeHeight, 0);
    };
}

export class BpAccordionPanelCtrl implements IBpAccordionPanelController {
    static $inject: [string] = ["localization", "$element"];
    public accordionGroup: any;
    public accordionGroupId: string;
    public accordionPanelId: string;
    public accordionPanelHeadingHeight: number;
    public accordionPanelClass: string;
    public accordionPanelHasIcon: string;
    public accordionPanelIsOpen: boolean;

    constructor(private localization: ILocalizationService, private $element) {
        // the accordionPanelId is/may be needed to target specific panels/nested elements
        this.accordionPanelId = this.accordionPanelId || "bp-accordion-panel-" + Math.floor(Math.random() * 10000);
    }

    public tryToToggle = () => {
        var panel = this.$element[0].firstChild;
        this.accordionGroup.tryToToggle(panel.id);
    };

    public pinUnpin = () => {
        var panel = this.$element[0].firstChild;
        var isPinned = panel.querySelectorAll("input[type=checkbox].bp-accordion-panel-pin:checked").length;
        this.accordionGroup.pinUnpin(panel.id, isPinned);
    };

    public $onInit = () => {
        this.accordionGroupId = this.accordionGroup.accordionId;

        // if not specific heading heigh is set, use the one defined in the container
        if (!this.accordionPanelHeadingHeight) {
            this.accordionPanelHeadingHeight = this.accordionGroup.accordionHeadingHeight;
        }
        // automatically set the first panel to be opened
        if (this.accordionGroup.accordionPanels.length === 0) {
            this.accordionPanelIsOpen = true;
        }
        if (!!this.accordionPanelClass) {
            this.accordionPanelHasIcon = "bp-accordion-panel-icon";
        } else {
            this.accordionPanelHasIcon = "";
        }
    };

    public $postLink = () => {
        var panel = this.$element[0];
        var trigger = panel.querySelector(".bp-accordion-panel-state");
        var pinner = panel.querySelector(".bp-accordion-panel-pin");
        var heading = panel.querySelector(".bp-accordion-panel-heading");

        trigger.style.height = this.accordionPanelHeadingHeight + "px";
        heading.style.height = this.accordionPanelHeadingHeight + "px";

        trigger.addEventListener("click", this.tryToToggle);
        pinner.addEventListener("click", this.pinUnpin);

        this.accordionGroup.addPanelAndHeight(this.accordionPanelId, this.accordionPanelHeadingHeight);
    };
}