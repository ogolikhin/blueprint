import * as angular from "angular";
import { BpSpecialLinkContainer } from "./bp-special-link";
import { BpLinksHelper } from "./bp-special-link";

angular.module("bp.widgets.speciallink", [])
    .service("bpLinksHelper", BpLinksHelper)
    .directive("body", BpSpecialLinkContainer.factory());

