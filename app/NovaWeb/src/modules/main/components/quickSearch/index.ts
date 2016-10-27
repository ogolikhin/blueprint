require("./_quickSearch.scss");
import {QuickSearchController} from "./quickSearchController";
import {QuickSearchComponent} from "./quickSearchComponent";
import {QuickSearchService} from "./quickSearchService";
import {QuickSearchModalController} from "./quickSearchModalController";

angular.module("bp.components.quickSearch", [])
    .controller("quickSearchController", QuickSearchController)
    .controller("quickSearchModalController", QuickSearchModalController)
    .component("quickSearch", new QuickSearchComponent())
    .service("quickSearchService", QuickSearchService);
