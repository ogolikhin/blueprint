import * as angular from "angular";
import { BpSpecialLinkContainer } from "./bp-special-link";

angular.module("bp.widgets.speciallink", [])
    .directive("body", BpSpecialLinkContainer.factory());

