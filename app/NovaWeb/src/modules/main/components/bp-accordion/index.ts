import * as angular from "angular";
import {BpAccordion} from "./bp-accordion";
import {BpAccordionPanel} from "./bp-accordion";

angular.module("bp.components.accordion", [])
    .component("bpAccordion", new BpAccordion())
    .component("bpAccordionPanel", new BpAccordionPanel());
