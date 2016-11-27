import * as angular from "angular";
import {BpAccordion, BpAccordionPanel, BpAccordionPanelService} from "./bp-accordion";

angular.module("bp.components.accordion", [])
    .component("bpAccordion", new BpAccordion())
    .service("bpAccordionPanelService", BpAccordionPanelService)
    .component("bpAccordionPanel", new BpAccordionPanel());
