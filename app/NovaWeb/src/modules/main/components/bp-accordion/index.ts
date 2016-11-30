import * as angular from "angular";
import {BpAccordion, BpAccordionPanel} from "./bp-accordion";
import {UtilityPanelService} from "../../../shell/bp-utility-panel/bp-utility-panel";

angular.module("bp.components.accordion", [])
    .component("bpAccordion", new BpAccordion())
    .component("utilityPanelService", UtilityPanelService)
    .component("bpAccordionPanel", new BpAccordionPanel());
