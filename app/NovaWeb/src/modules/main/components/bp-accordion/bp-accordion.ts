import {ILocalizationService} from "../../../core/localization";

interface IBpAccordionController {
    accordionId?: string;
    accordionHeadingHeight?: number;
}

interface IBpAccordionPanelController {
    accordionPanelId?: string;
    accordionPanelHeading?: string;
    accordionPanelHeadingHeight?: number;
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
    public controllerAs: string = "bpAccordionPanel";
    public bindings: any = {
        accordionPanelId: "@",
        accordionPanelHeading: "@",
        accordionPanelHeadingHeight: "@"
    };
    public transclude: boolean = true;
    public replace: boolean = true;
    public require: any = {
        accordionGroup: "^bpAccordion"
    };
}

class BpAccordionCtrl implements IBpAccordionController {
    static $inject: [string] = ["$element", "$timeout"];
    public accordionId: string;
    public accordionHeadingHeight: number;

    private defaultHeadingHeight: number = 40; // default heading height for all the accordion panels
    private accordionPanels = [];

    constructor(private $element, private $timeout) {
        // the accordionId is needed in case multiple accordions are present in the same page
        this.accordionId = this.accordionId || "bp-accordion-" + Math.floor(Math.random() * 10000);

        this.accordionHeadingHeight = this.accordionHeadingHeight || this.defaultHeadingHeight;
    }

    public addPanelAndHeight = (accordionPanelId: string, accordionPanelHeight: number) => {
        this.accordionPanels.push([accordionPanelId, accordionPanelHeight]);
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
            var accordionHeaderHeight = 0;
            for (var p = 0; p < this.accordionPanels.length; p++) {
                if(accordionElement.id === this.accordionPanels[p][0]) {
                    accordionHeaderHeight = parseInt(this.accordionPanels[p][1], 10);
                    break;
                }
            }
            /* tslint:disable */
            // the element is not opened or pinned
            if (accordionElement.querySelectorAll("input[type=radio].bp-accordion-panel-state:checked, input[type=checkbox].bp-accordion-panel-pin:checked").length) {
                /* tslint:enable */
                accordionElement.querySelectorAll(".bp-accordion-panel-content")[0].style.height = "calc(100% - " + accordionHeaderHeight + "px)";
            } else {
                compensationForClosedHeaders += accordionHeaderHeight;
                accordionElement.querySelectorAll(".bp-accordion-panel-content")[0].style.height = "0";
                accordionElement.parentNode.style.height = accordionHeaderHeight + "px";
            }
        }

        /* tslint:disable */
        var children = accordion.querySelectorAll("input[type=radio].bp-accordion-panel-state:checked, input[type=checkbox].bp-accordion-panel-pin:checked");
        /* tslint:enable */
        for (var i = 0; i < children.length; i++) {
            var accordionElement = children[i].parentNode.parentNode;
            /* tslint:disable */
            accordionElement.style.height = "calc(" + (100 / numberOfOpenElements) + "% - " + (compensationForClosedHeaders / numberOfOpenElements) + "px)";
            /* tslint:enable */
        }
    };

    public $postLink = () => {
        // we need to redistribute the height after all the panels have been added
        this.$timeout(this.redistributeHeight, 0);
    };
}

class BpAccordionPanelCtrl implements IBpAccordionPanelController {
    static $inject: [string] = ["localization", "$element"];
    public accordionGroup: any;
    public accordionGroupId: string;
    public accordionPanelId: string;
    public accordionPanelHeadingHeight: number;
    public accordionPanelIsOpen: boolean;

    constructor(private localization: ILocalizationService, private $element) {
        // the accordionPanelId is/may be needed to target specific panels/nested elements
        this.accordionPanelId = this.accordionPanelId || "bp-accordion-panel-" + Math.floor(Math.random() * 10000);
    }

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
    };

    public $postLink = () => {
        var panel = this.$element[0];
        var trigger = panel.querySelectorAll(".bp-accordion-panel-state")[0];
        var pinner = panel.querySelectorAll(".bp-accordion-panel-pin")[0];
        var heading = panel.querySelectorAll(".bp-accordion-panel-heading")[0];

        trigger.style.height = this.accordionPanelHeadingHeight + "px";
        heading.style.height = this.accordionPanelHeadingHeight + "px";

        trigger.addEventListener("click", this.accordionGroup.redistributeHeight);
        pinner.addEventListener("click", this.accordionGroup.redistributeHeight);

        this.accordionGroup.addPanelAndHeight(this.accordionPanelId, this.accordionPanelHeadingHeight);
    };
}