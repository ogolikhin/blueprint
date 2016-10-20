require("./_quickSearch.scss");
import {QuickSearchController} from "./quickSearchController";
import {QuickSearchComponent} from "./quickSearchComponent";
angular.module("bp.components.quickSearch", [])
    .controller("quickSearchController", QuickSearchController)
    .component("quickSearch", new QuickSearchComponent());
