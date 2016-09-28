import * as angular from "angular";
import { BpSpecialLinkContainer } from "./bp-special-link";

angular.module("bp.widjets.speciallink", [])
    .directive("body", BpSpecialLinkContainer.factory());

