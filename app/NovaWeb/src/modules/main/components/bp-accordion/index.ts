import * as angular from "angular";
import {BpAccordion, BpAccordionPanel} from "./bp-accordion";

angular.module("bp.components.accordion", [])
    .component("bpAccordion", new BpAccordion())
    .component("bpAccordionPanel", new BpAccordionPanel());
