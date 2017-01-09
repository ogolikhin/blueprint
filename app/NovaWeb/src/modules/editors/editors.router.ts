import {ItemStateController} from "./item-state/item-state.controller";
import {IArtifactManager} from "../managers";
import {IItemInfoService} from "../core/navigation/item-info.svc";
import {ILoadingOverlayService} from "../core/loading-overlay/loading-overlay.svc";

export class ArtifactRoutes {

    public static $inject = [
        "$stateProvider",
        "$urlMatcherFactoryProvider"
    ];

    constructor($stateProvider: ng.ui.IStateProvider,
                $urlMatcherFactoryProvider: any) {

        $urlMatcherFactoryProvider.caseInsensitive(true);

        // register states with the router
        $stateProvider
            .state("main.unpublished", <ng.ui.IState>{
                url: "/unpublished",
                template: "<unpublished></unpublished>",
                resolve: {
                    saved: ["artifactManager", (am: IArtifactManager) => am.autosave()]
                }
            })
            .state("main.jobs", <ng.ui.IState>{
                url: "/jobs",
                template: "<jobs></jobs>",
                resolve: {
                    saved: ["artifactManager", (am: IArtifactManager) => am.autosave()]
                }
            })
            .state("main.item", <ng.ui.IState>{
                url: "/{id:int}?{version:int}&{path:navpath}",
                template: require("./item-state/item-state.html"),
                reloadOnSearch: false,
                controller: ItemStateController,
                controllerAs: "$ctrl",
                params: {
                    // prevents array format from being split into path=1&path=2&...
                    path: {array : false}
                },
                resolve: {
                    itemInfo: ["$stateParams", "$q", "itemInfoService", "loadingOverlayService",
                        ($stateParams: ng.ui.IStateParamsService,
                         $q: ng.IQService,
                         itemInfoService: IItemInfoService,
                         loadingOverlayService: ILoadingOverlayService) => {

                        const id = parseInt($stateParams["id"], 10);
                        if (_.isFinite(id)) {
                            const loaderId = loadingOverlayService.beginLoading();
                            return itemInfoService.get(id).finally(() => {
                                loadingOverlayService.endLoading(loaderId);
                            });
                        } else {
                            return $q.reject();
                        }
                    }],
                    saved: ["artifactManager", (am: IArtifactManager) => am.autosave()]
                }
            });
    }
}
