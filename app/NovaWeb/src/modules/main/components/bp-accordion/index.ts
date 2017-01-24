import * as angular from "angular";
import {BpAccordion, BpAccordionPanel} from "./bp-accordion";

angular.module("bp.components.accordion", ["localization"])
    .component("bpAccordion", new BpAccordion())
    .component("bpAccordionPanel", new BpAccordionPanel());
