require("./_quickSearch.scss");
import {QuickSearchController, IQuickSearchController} from "./quickSearchController";
import {QuickSearchComponent} from "./quickSearchComponent";
import {QuickSearchService, IQuickSearchService} from "./quickSearchService";
import {QuickSearchModalController, IQuickSearchModalController} from "./quickSearchModalController";

angular.module("bp.components.quickSearch", [])
    .controller("quickSearchController", QuickSearchController)
    .controller("quickSearchModalController", QuickSearchModalController)
    .component("quickSearch", new QuickSearchComponent())
    .service("quickSearchService", QuickSearchService);

export {
    IQuickSearchService,
    IQuickSearchModalController,
    IQuickSearchController
};
