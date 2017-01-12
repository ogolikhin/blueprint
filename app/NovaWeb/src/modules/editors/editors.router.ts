import {ItemStateController} from "./item-state/item-state.controller";
import {IArtifactManager} from "../managers";
import {IItemStateService} from "./item-state/item-state.svc";

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
                controller: ItemStateController,
                controllerAs: "$ctrl",
                params: {
                    // prevents array format from being split into path=1&path=2&...
                    path: {array : false}
                },
                resolve: {
                    itemInfo: ["$stateParams", "itemStateService",
                        ($stateParams: ng.ui.IStateParamsService, itemStateService: IItemStateService) => {

                        const id = parseInt($stateParams["id"], 10);
                        return itemStateService.getItemInfoResult(id);
                    }],
                    saved: ["artifactManager", (am: IArtifactManager) => am.autosave()]
                }
            });
    }
}
