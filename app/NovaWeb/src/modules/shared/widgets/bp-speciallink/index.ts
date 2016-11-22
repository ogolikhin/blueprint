import * as angular from "angular";
import {BpSpecialLinkContainer, BpLinksHelper} from "./bp-special-link";

angular.module("bp.widgets.speciallink", [])
    .service("bpLinksHelper", BpLinksHelper)
    .directive("body", BpSpecialLinkContainer.factory());

