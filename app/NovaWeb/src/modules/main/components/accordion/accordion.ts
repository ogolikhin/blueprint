import {ILocalizationService} from "../../../core/localization";

interface IAccordionController {
    headerHeight: string;
}

export class Accordion implements ng.IComponentOptions {
    public template: string;
    public controller: Function;
    public bindings: any;
    public transclude: boolean = true;

    constructor() {
        this.template = require("./accordion.html");
        this.controller = AccordionCtrl;
        this.bindings = {
            headerHeight: "@"
        };
    }
}

class AccordionCtrl implements IAccordionController {
    static $inject: [string] = ["localization", "$element"];
    public headerHeight: string;
    private accordionHeadersHeight: number = 0;
    private accordionContainer: any;

    constructor(private localization: ILocalizationService, private $element) {
        if (this.headerHeight) {
            this.accordionHeadersHeight = parseInt(this.headerHeight, 10);
        }
        if (this.accordionHeadersHeight <= 0) {
            this.accordionHeadersHeight = 33;
        }

        if (this.$element[0] && this.$element[0].childNodes[0]) {
            this.accordionContainer = this.$element[0].childNodes[0];
            this.link();
        }
    }

    private link() {
        var accordionContainer = this.accordionContainer;
        var accordionHeadersHeight = this.accordionHeadersHeight;

        function redistributeHeight() {
            var hiddenRadioButtons = accordionContainer.querySelectorAll("input[type=radio].state");
            var numberOfAccordionElements = hiddenRadioButtons.length;
            var numberOfPinnedElements = accordionContainer.querySelectorAll("input[type=checkbox].pin:checked").length;
            var isCurrentElementAlsoPinned = accordionContainer.querySelectorAll("input[type=radio].state:checked ~ input[type=checkbox].pin:checked").length;
            var numberOfOpenElements = numberOfPinnedElements + (isCurrentElementAlsoPinned ? 0 : 1);
            var numberOfClosedElements = numberOfAccordionElements - numberOfOpenElements;

            var children = accordionContainer.childNodes;
            for (var i = 0; i < children.length; i++) {
                if (children[i].nodeType === 1 && children[i].tagName.toUpperCase() === "LI") {
                    var accordionElement = children[i];
                    if (accordionElement.querySelectorAll("input[type=radio].state:checked, input[type=checkbox].pin:checked").length) {
                        var accordionHeaderHeight = accordionHeadersHeight;
                        var compensationForClosedHeaders = accordionHeaderHeight * (numberOfClosedElements / numberOfOpenElements);
                        accordionElement.style.height = "calc(" + (100 / numberOfOpenElements).toString() +
                            "% - " + compensationForClosedHeaders.toString() + "px)";
                        accordionElement.querySelectorAll(".content")[0].style.height = "calc(100% - " + accordionHeaderHeight + "px)";
                    } else {
                        accordionElement.style.height = "auto";
                        accordionElement.querySelectorAll(".content")[0].style.height = "0";
                    }
                }
            }
        }

        if (accordionContainer.hasChildNodes()) {
            var children = accordionContainer.childNodes;
            for (var i = 0; i < children.length; i++) {
                if (children[i].nodeType === 1 && children[i].tagName.toUpperCase() === "LI") {
                    var accordionControllers = children[i].querySelectorAll("input[type=radio].state, input[type=checkbox].pin");
                    for (var j = 0; j < accordionControllers.length; j++) {
                        accordionControllers[j].addEventListener("click", redistributeHeight);
                    }
                }
            }
            redistributeHeight();
        }
    }
}