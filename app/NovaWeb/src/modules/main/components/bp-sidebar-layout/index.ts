import * as angular from "angular";
import {BpSidebarLayout} from "./bp-sidebar-layout";
import {UtilityPanelService} from "../../../shell/bp-utility-panel/bp-utility-panel";

angular.module("bp.components.sidebar", [])
    .component("bpSidebarLayout", new BpSidebarLayout())
    .service("utilityPanelService", UtilityPanelService);
